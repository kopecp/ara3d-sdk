using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ara3D.Memory;

namespace Ara3D.IO.StepParser;

public static unsafe class StepTokenizer
{
    public static readonly StepTokenType[] TokenLookup =
        StepTokenizerLookupHelpers.CreateTokenLookup();

    public static readonly bool[] IsNumberLookup =
        StepTokenizerLookupHelpers.CreateNumberLookup();

    public static readonly bool[] IsIdentLookup =
        StepTokenizerLookupHelpers.CreateIdentLookup();

    public static readonly bool[] IsDigitLookup =
        StepTokenizerLookupHelpers.CreateDigitLookup();

    public static readonly bool[] IsWhiteSpaceLookup =
        StepTokenizerLookupHelpers.CreateWhiteSpaceLookup();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldStoreType(StepTokenType type)
    {
        switch (type)
        {
            case StepTokenType.Identifier:
            case StepTokenType.SingleQuotedString:
            case StepTokenType.DoubleQuotedString:
            case StepTokenType.Number:
            case StepTokenType.Symbol:
            case StepTokenType.Id:
            case StepTokenType.Unassigned:
            case StepTokenType.Redeclared:
                return true;

            case StepTokenType.BeginGroup:
            case StepTokenType.EndGroup:
            case StepTokenType.EndOfLine:
            case StepTokenType.Definition:
                return true;

            case StepTokenType.None:
            case StepTokenType.Whitespace:
            case StepTokenType.Separator:
            case StepTokenType.Comment:
            case StepTokenType.Unknown:
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static StepTokenType ParseToken(ref byte* cur, byte* end)
    {
        Debug.Assert(cur < end);

        byte c = *cur++;

        // Fast path: single-byte tokens via lookup (make sure TokenLookup is set for these)
        // e.g. '(' ')' '=' ';' ',' '*' etc.
        var t = TokenLookup[c];
        if (t != StepTokenType.None)
            return t;

        // Whitespace (including \r\n etc.)
        if (IsWhiteSpaceLookup[c])
        {
            while (cur < end && IsWhiteSpaceLookup[*cur]) cur++;
            return StepTokenType.Whitespace;
        }

        // Identifier start (you should have a separate start table)
        // If you don't, you can approximate with: IsIdentLookup[c] && !IsDigitLookup[c]
        if (IsIdentLookup[c] && !IsDigitLookup[c])
        {
            while (cur < end && IsIdentLookup[*cur]) cur++;
            return StepTokenType.Identifier;
        }

        // Number start (digit; optionally also + - . depending on STEP number grammar)
        if (IsDigitLookup[c])
        {
            while (cur < end && IsNumberLookup[*cur]) cur++;
            return StepTokenType.Number;
        }

        // Multi-byte/special starters
        switch (c)
        {
            case (byte)'"':
                ScanQuoted(ref cur, end, (byte)'"');
                return StepTokenType.DoubleQuotedString;

            case (byte)'\'':
                ScanQuoted(ref cur, end, (byte)'\'');
                return StepTokenType.SingleQuotedString;

            case (byte)'.':
                // STEP symbol like .T. .F. .ENUM.
                while (cur < end && IsIdentLookup[*cur]) cur++;
                if (cur < end && *cur == (byte)'.') cur++; // only consume if present
                return StepTokenType.Symbol;

            case (byte)'#':
                while (cur < end && IsDigitLookup[*cur]) cur++;
                return StepTokenType.Id;

            case (byte)'/':
                // Comment: /* ... */
                if (cur >= end) return StepTokenType.Unknown;
                if (*cur != (byte)'*') return StepTokenType.Unknown; // if you only support /* */
                cur++; // consume '*'
                ScanBlockComment(ref cur, end);
                return StepTokenType.Comment;

            default:
                return StepTokenType.Unknown;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScanQuoted(ref byte* cur, byte* end, byte q)
    {
        // cur points just after opening quote
        while (cur < end)
        {
            if (*cur != q) { cur++; continue; }

            // quote found
            if (cur + 1 < end && cur[1] == q)
            {
                cur += 2; // escaped quote
                continue;
            }

            cur++; // closing quote
            return;
        }
        // Unterminated: leave cur at end
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScanBlockComment(ref byte* cur, byte* end)
    {
        // cur points after '/*'
        while (cur < end)
        {
            byte prev = *cur++;
            if (prev == (byte)'*' && cur < end && *cur == (byte)'/')
            {
                cur++; // consume '/'
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhiteSpace(StepTokenType type)
    {
        return type == StepTokenType.Whitespace || type == StepTokenType.Comment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AdvancePast(ref byte* cur, byte* end, StepTokenType type)
    {
        if (cur >= end) return false;
        var r = ParseToken(ref cur, end);
        while (IsWhiteSpace(r))
        {
            if (cur >= end) return false;
            r = ParseToken(ref cur, end);
        }
        return r == type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AdvanceTo(ref byte* cur, byte* end, out StepToken token, StepTokenType type)
    {
        while (cur < end)
        {
            var begin = cur;
            var r = ParseToken(ref cur, end);
            if (r == type)
            {
                token = new StepToken(begin, cur);
                return true;
            }
        }
        token = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AdvanceToDefinition(ref byte* cur, byte* end, out StepToken id)
    {
        id = default;
        while (cur < end)
        {
            if (!AdvanceTo(ref cur, end, out id, StepTokenType.Id))
                return false;
            if (AdvancePast(ref cur, end, StepTokenType.Definition))
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AdvanceToAndTokenizeDefinition(ref byte* cur, byte* end, out StepToken id, UnmanagedList<StepToken> tokens)
    {
        Debug.Assert(tokens.Count == 0);

        if (!AdvanceToDefinition(ref cur, end, out id))
            return false;
            
        while (cur < end)
        {
            var begin = cur;
            var type = ParseToken(ref cur, end);
            if (type == StepTokenType.EndOfLine)
                return true;
            if (ShouldStoreType(type))
                tokens.Add(new StepToken(begin, cur));
        }

        return false;
    }
}