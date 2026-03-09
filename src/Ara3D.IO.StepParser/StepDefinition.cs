using System;
using System.Runtime.CompilerServices;

namespace Ara3D.IO.StepParser
{
    public readonly struct StepDefinition
    {
        public readonly int Id;
        public readonly StepToken NameToken;
        public readonly StepRawValue AttributesValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StepDefinition(int id, StepToken name, StepRawValue attributesValue)
        {
            Id = id;
            NameToken = name;
            AttributesValue = attributesValue;
        }

        public string GetName()
            => NameToken.ToString();

        public StepRawValue[] GetAttributes(StepDocument document)
            => GetAttributes(document.RawValueData);

        public StepRawValue[] GetAttributes(StepRawValueData data)
            => data.AsArray(AttributesValue);
    }
}