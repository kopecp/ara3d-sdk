using System.ComponentModel;
using System.Reflection;

namespace Ara3D.PropKit;

public static class PropDescriptorFactory
{
    public static PropDescriptor CreateDescriptor(this MemberInfo mi, Type type, bool isReadOnly)
    {
        var displayNameAttr = mi.GetCustomAttribute<DisplayNameAttribute>();
        var descAttr = mi.GetCustomAttribute<DescriptionAttribute>();
        return new(type, mi.Name, isReadOnly, displayNameAttr?.DisplayName, descAttr?.Description);
    }

    public static PropDescriptor CreateDescriptor(this FieldInfo fi)
    {
        return CreateDescriptor(fi, fi.FieldType, fi.IsInitOnly);
    }

    public static PropDescriptor CreateDescriptor(this PropertyInfo pi)
    {
        return CreateDescriptor(pi, pi.PropertyType, pi.CanWrite);
    }
}