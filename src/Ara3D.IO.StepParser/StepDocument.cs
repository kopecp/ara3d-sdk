using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Ara3D.Logging;
using Ara3D.Memory;
using Ara3D.Utils;

namespace Ara3D.IO.StepParser;

public sealed unsafe class StepDocument : IDisposable
{
    public readonly FilePath FilePath;
    public readonly byte* DataStart;
    public readonly byte* DataEnd;
    public readonly IBuffer Data;
    public readonly UnmanagedList<StepDefinition> Definitions = new();
    public readonly StepRawValueData RawValueData;
    public readonly StepHeader Header;

    public StepDocument(FilePath filePath, ILogger logger = null)
        : this(Serializer.ReadAllBytesAligned(filePath), filePath, logger)
    { }

    public StepDocument(IBuffer data, string filePath = "", ILogger logger = null)
    {
        FilePath = filePath;
        logger ??= Logger.Null;
        Data = data;
        DataStart = Data.GetPointer();
        DataEnd = DataStart + Data.NumBytes();

        logger.Log($"Parsing definition");
        // Estimate average token length of 8 bytes
        var estimateNumTokens = Data.NumBytes() / 8;
        RawValueData = new StepRawValueData((int)estimateNumTokens);
        // Estimate about 8 tokens per definition on average
        var estimateNumDefs = estimateNumTokens / 8; 
        Definitions = new UnmanagedList<StepDefinition>((int)estimateNumDefs);

        // Initialize the token list with a capacity of the longest line we hope to encounter
        using var tokens = new UnmanagedList<StepToken>(32000);

        var cur = DataStart;

        logger.Log($"Parsing header");
        Header = StepHeader.Parse(ref cur, DataEnd);

        logger.Log($"Parsing definitions");
        while (true)
        {
            tokens.Clear();
            if (!StepTokenizer.AdvanceToAndTokenizeDefinition(ref cur, DataEnd, out var idToken, tokens))
                break;

            var curToken = tokens.Begin();
            var endToken = tokens.End();
            var valueIndex = RawValueData.Values.Count;
            RawValueData.AddTokens(ref curToken, endToken);

            var entityValue = RawValueData.Values[valueIndex];
            var entityName = RawValueData.Tokens[entityValue.Index];
            var entityAttr = RawValueData.Values[valueIndex + 1];

            var definition = new StepDefinition(idToken.AsId(), entityName, entityAttr);

            Definitions.Add(definition);
                
            tokens.Clear();
        }
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
}