using Ara3D.Logging;
using Ara3D.Memory;
using Ara3D.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ara3D.IO.StepParser;

public sealed unsafe class StepDocument : IDisposable
{
    public readonly FilePath FilePath;
    public readonly byte* DataStart;
    public readonly byte* DataEnd;
    public readonly IBuffer Data;
    public readonly UnmanagedList<StepDefinition> Definitions = new();
    public readonly StepHeader Header;
    public UnmanagedList<StepRawValue> Values = new();
    public UnmanagedList<StepToken> Tokens = new();

    public StepDocument(FilePath filePath, ILogger logger = null)
        : this(Serializer.ReadAllBytesAligned(filePath), filePath, logger)
    { }

    public StepDocument(IBuffer data, string filePath = "", ILogger logger = null)
    {
        FilePath = filePath;
        Data = data;
        DataStart = Data.GetPointer();
        DataEnd = DataStart + Data.NumBytes();

        logger?.Log($"Parsing definition");

        // Estimate average token length of 8 bytes
        var estimateNumTokens = Data.NumBytes() / 8;
        Tokens.Accomodate((int)estimateNumTokens);

        // Estimate one value for every two tokens 
        var estimateNumValues = estimateNumTokens / 2;
        Values.Accomodate((int)estimateNumValues);

        // Estimate about 8 tokens per definition on average per definition
        var estimateNumDefs = estimateNumTokens / 8; 
        Definitions = new UnmanagedList<StepDefinition>((int)estimateNumDefs);

        // Initialize the token list with a capacity of the longest line we hope to encounter
        using var tokens = new UnmanagedList<StepToken>(32000);

        var cur = DataStart;

        logger?.Log($"Parsing header");
        Header = StepHeader.Parse(ref cur, DataEnd);

        logger?.Log($"Parsing definitions");
        while (true)
        {
            tokens.Clear();
            if (!StepTokenizer.AdvanceToAndTokenizeDefinition(ref cur, DataEnd, out var idToken, tokens))
                break;

            var curToken = tokens.Begin();
            var endToken = tokens.End();
            var valueIndex = Values.Count;
            AddTokens(ref curToken, endToken);

            var entityValue = Values[valueIndex];
            var entityName = Tokens[entityValue.Index];
            var entityAttr = Values[valueIndex + 1];

            var definition = new StepDefinition(idToken.AsId(), entityName, entityAttr);

            Definitions.Add(definition);
                
            tokens.Clear();
        }

        logger?.Log($"# tokens = {Tokens.Count}, estimated was {estimateNumTokens}");
        logger?.Log($"# definitions = {Definitions.Count}, estimated was {estimateNumDefs}");
        logger?.Log($"# values = {Values.Count}, estimated was {estimateNumValues}");
    }

    public static bool Assert(bool condition, string text)
    {
        if (!condition)
            throw new Exception($"Assertion failed: {text}");
        return true;
    }
        
    public void Dispose()
    {
        Trace.WriteLine($"Disposing data");
        if (Data is IDisposable d)
            d.Dispose();
    }

    public static StepDocument Create(FilePath fp) 
        => new(fp);

    //==
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTokens(ref StepToken* cur, StepToken* end)
    {
        while (cur < end)
        {
            ProcessNextToken(ref cur, end);
            cur++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessNextToken(ref StepToken* cur, StepToken* end)
    {
        switch (cur->Type)
        {
            case StepTokenType.Identifier:
                AddEntity(*cur);
                break;

            case StepTokenType.SingleQuotedString:
            case StepTokenType.DoubleQuotedString:
                AddString(*cur);
                break;

            case StepTokenType.Number:
                AddNumber(*cur);
                break;

            case StepTokenType.Symbol:
                AddSymbol(*cur);
                break;

            case StepTokenType.Id:
                AddId(*cur);
                break;

            case StepTokenType.Unassigned:
                AddUnassigned();
                break;

            case StepTokenType.Redeclared:
                AddRedeclared();
                break;

            case StepTokenType.BeginGroup:
                AddList(ref cur, end);
                if (cur == end || cur->Type != StepTokenType.EndGroup)
                    throw new Exception("Expected EndGroup token after BeginGroup");
                break;

            case StepTokenType.None:
            case StepTokenType.Whitespace:
            case StepTokenType.Separator:
            case StepTokenType.Comment:
            case StepTokenType.Unknown:
            case StepTokenType.EndGroup:
            case StepTokenType.EndOfLine:
            case StepTokenType.Definition:
                throw new Exception($"Unhandled token type {cur->Type}");

            default:
                throw new Exception($"Out of range token type {cur->Type}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddId(StepToken token)
        => AddTokenAndValue(token, StepKind.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddEntity(StepToken token)
    {
        var tokenIndex = AddToken(token);
        var valueIndex = Values.Count;
        var val = new StepRawValue(StepKind.Entity, tokenIndex, valueIndex + 1);
        AddValue(val);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNumber(StepToken token)
        => AddTokenAndValue(token, StepKind.Number);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddString(StepToken token)
        => AddTokenAndValue(token, StepKind.String);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddSymbol(StepToken token)
        => AddTokenAndValue(token, StepKind.Symbol);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddTokenAndValue(StepToken token, StepKind kind)
    {
        Debug.Assert(kind is StepKind.Entity or StepKind.Id or StepKind.Number or StepKind.String or StepKind.Symbol);
        var id = AddToken(token);
        AddValue(new StepRawValue(kind, id));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddList(ref StepToken* cur, StepToken* end)
    {
        // Advance past the begin list 
        cur++;

        AddValue(new StepRawValue(StepKind.List, 0));
        var curIndex = Values.Count;

        while (cur != end && cur->Type != StepTokenType.EndGroup)
        {
            ProcessNextToken(ref cur, end);
            cur++;
        }

        var listCount = Values.Count - curIndex;
        Values[curIndex - 1] = new StepRawValue(StepKind.List, curIndex, listCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRedeclared()
        => AddValue(new StepRawValue(StepKind.Redeclared, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddUnassigned()
        => AddValue(new StepRawValue(StepKind.Unassigned, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddValue(StepRawValue value)
        => Values.Add(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int AddToken(StepToken token)
    {
        Tokens.Add(token);
        return Tokens.Count - 1;
    }

    //== 
    // Entity tests 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetEntityName(StepRawValue entity)
        => Tokens[entity.Index].ToString();

    //== 
    // String building methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(StepRawValue value)
        => BuildString(value, new StringBuilder()).ToString();

    public StringBuilder BuildString(StepRawValue value, StringBuilder sb)
    {
        return value.Kind switch
        {
            StepKind.Id => sb.Append(Tokens[value.Index]),
            StepKind.Entity => sb.Append(Tokens[value.Index]),
            StepKind.Number => sb.Append(Tokens[value.Index]),
            StepKind.List => BuildStringFromList(value, sb),
            StepKind.Symbol => sb.Append(Tokens[value.Index]),
            StepKind.String => sb.Append(Tokens[value.Index]),
            StepKind.Redeclared => sb.Append("*"),
            StepKind.Unassigned => sb.Append("$"),
            _ => sb.Append("_UNKNOWN_"),
        };
    }

    public StringBuilder BuildStringFromList(StepRawValue value, StringBuilder sb)
    {
        var vals = AsFlatArray(value);
        sb.Append('(');

        var index = 0;
        while (index < vals.Length)
        {
            var val = vals[index];

            BuildString(val, sb);
            if (val.Kind == StepKind.List)
            {
                index += val.Count + 1;
            }
            else
                index++;

            if (index >= vals.Length)
                break;

            if (val.Kind != StepKind.Entity)
                sb.Append(", ");
        }

        sb.Append(')');
        return sb;
    }

    //==
    // StepValue methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StepToken AsToken(StepRawValue value)
    {
        Debug.Assert(value.Kind is StepKind.Entity or StepKind.Id or StepKind.Number or StepKind.String or StepKind.Symbol);
        return Tokens[value.Index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double AsNumber(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.Number);
        return AsToken(value).AsNumber();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsId(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.Id);
        return AsToken(value).AsId();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsString(StepRawValue value)
        => value.IsRedeclared ? "*"
        : value.IsUnassigned ? "$"
        : Encoding.ASCII.GetString(AsToken(value).Span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsTrimmedString(StepRawValue value)
    {
        Debug.Assert(value.Kind is StepKind.String or StepKind.Symbol);
        return Encoding.ASCII.GetString(AsToken(value).Span[1..^1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StepRawValue[] AsFlatArray(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.List || value.IsUnassignedOrRedeclared);
        var n = value.Count;
        var offset = value.Index;
        var r = new StepRawValue[n];
        for (var i = 0; i < n; ++i)
            r[i] = Values[i + offset];
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<StepRawValue> AsArray(StepRawValue value)
    {
        var rawArray = AsFlatArray(value);
        var i = 0;
        var j = 0;
        while (i < rawArray.Length)
        {
            var rawEl = rawArray[i];
            if (j != i)
                rawArray[j] = rawEl;
            j++;

            if (rawEl.IsList)
            {
                i += rawEl.Count + 1;
            }
            else if (rawEl.IsEntity)
            {
                i += 1;
                if (i >= rawArray.Length || !rawArray[i].IsList)
                    throw new Exception("Expected a list to follow an entity");
                i += rawArray[i].Count + 1;
            }
            else
            {
                i++;
            }
        }
        return rawArray.AsSpan().Slice(0, j);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double[] AsNumbers(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.List || value.IsUnassignedOrRedeclared);
        var vals = AsFlatArray(value);
        var r = new double[vals.Length];
        for (var i = 0; i < vals.Length; ++i)
            r[i] = AsNumber(vals[i]);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] AsIds(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.List || value.IsUnassignedOrRedeclared);
        var vals = AsFlatArray(value);
        var r = new int[vals.Length];
        for (var i = 0; i < vals.Length; ++i)
            r[i] = AsId(vals[i]);
        return r;
    }

    public StepToken[] AsTokens(StepRawValue value)
    {
        Debug.Assert(value.Kind == StepKind.List);
        var vals = AsFlatArray(value);
        var r = new StepToken[vals.Length];
        for (var i = 0; i < vals.Length; ++i)
            r[i] = AsToken(vals[i]);
        return r;
    }
}