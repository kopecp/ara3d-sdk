using System;
using System.Runtime.CompilerServices;

namespace Ara3D.IO.StepParser;

public static class StepDocumentExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepToken[] AsTokens(this StepRawValue value, StepDocument data)
        => data.AsTokens(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] AsNumbers(this StepRawValue value, StepDocument data)
        => data.AsNumbers(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] AsIds(this StepRawValue value, StepDocument data)
        => data.AsIds(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<StepRawValue> AsArray(this StepRawValue value, StepDocument data)
        => data.AsArray(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepToken AsToken(this StepRawValue value, StepDocument data)
        => data.AsToken(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AsNumber(this StepRawValue value, StepDocument data)
        => data.AsNumber(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsId(this StepRawValue value, StepDocument data)
        => data.AsId(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(this StepRawValue value, StepDocument data)
        => data.AsString(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsTrimmedString(this StepRawValue value, StepDocument data)
        => data.AsTrimmedString(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StepRawValue[] AsFlatArray(this StepRawValue value, StepDocument data)
        => data.AsFlatArray(value);
}