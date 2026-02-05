using Ara3D.IO.StepParser;

namespace Ara3D.IfcGeometry;

public readonly struct StepAttrReader
{
    private readonly StepValue _attrList;

    public StepAttrReader(StepValue entity)
    {
        _attrList = entity.GetEntityAttributesValue();
    }

    public StepValue? StepOrNull(int index)
    {
        if (!TryGet(index, out var v)) return null;
        return v.IsMissing() ? null : v;
    }

    public string StringOrEmpty(int index)
    {
        if (!TryGet(index, out var v)) return string.Empty;
        if (v.IsMissing()) return string.Empty;
        return v.IsString() ? v.AsString() : v.ToString();
    }

    public int IdOrZero(int index)
    {
        if (!TryGet(index, out var v)) return 0;
        if (v.IsMissing()) return 0;
        return v.IsId() ? v.AsId() : 0;
    }

    public IReadOnlyList<int> IdArrayOrEmpty(int index)
    {
        if (!TryGet(index, out var v)) return [];
        if (v.IsMissing() || !v.IsList()) return [];
        return v.AsIdList();
    }

    private bool TryGet(int index, out StepValue value)
    {
        var i = 0;
        foreach (var el in _attrList.GetElements())
        {
            if (i == index)
            {
                value = el;
                return true;
            }

            i++;
        }

        value = null;
        return false;
    }
}