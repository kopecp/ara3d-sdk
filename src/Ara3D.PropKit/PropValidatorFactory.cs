using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ara3D.PropKit;

public static class PropValidatorFactory
{
    public static PropConstraints CreateConstraints(MemberInfo mi, object val)
    {
        var rangeAttr = mi.GetCustomAttribute<RangeAttribute>();
        if (rangeAttr == null) return default;
        return new(val, rangeAttr?.Minimum, rangeAttr?.Maximum);
    }

    public static PropConstraints CreateConstraints(object host, FieldInfo fi)
    {
        return CreateConstraints(fi, fi.GetValue(host));
    }

    public static PropConstraints CreateConstraints(object host, PropertyInfo pi)
    {
        return CreateConstraints(pi, pi.GetValue(host));
    }

    public static IPropValidator CreateValidator(object host, PropertyInfo pi)
    {
        var constraints = CreateConstraints(host, pi);
        return constraints.HasMinMax ? new PropConstraintsValidator(constraints) : null;
    }

    public static IPropValidator CreateValidator(object host, FieldInfo fi)
    {
        var constraints = CreateConstraints(host, fi);
        return constraints.HasMinMax ? new PropConstraintsValidator(constraints) : null;
    }
}