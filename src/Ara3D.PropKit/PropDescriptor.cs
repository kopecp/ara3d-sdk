using Ara3D.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ara3D.PropKit;

public record PropDescriptor
{
    public Type Type { get; init; }
    public string Name { get; init; }
    public bool IsReadOnly { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }

    public PropDescriptor(Type type, string name, bool isReadOnly,
        string displayName = null, string description = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name required") : name;
        IsReadOnly = isReadOnly;
        DisplayName = displayName ?? name.SplitCamelCase();
        Description = description ?? "";
    }

    public bool CanSupportNull 
        => !Type.IsValueType;

    public bool HasValidType(object o)
    {
        if (o == null) return CanSupportNull;
        return o.GetType() == Type;
    }
}

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
        if (constraints.HasMinMax)
            return new PropConstraintsValidator(constraints);
        return null;
    }

    public static IPropValidator CreateValidator(object host, FieldInfo fi)
    {
        var constraints = CreateConstraints(host, fi);
        if (constraints.HasMinMax)
            return new PropConstraintsValidator(constraints);
        return null;
    }
}

public static class PropAccessorFactory
{
    public static IPropAccessor CreatePropAccessor(
        this PropDescriptor descriptor,
        IPropValidator validator,
        Type targetType, Type valueType,
        Delegate getterRef, Delegate? setterRef)
    {
        var open = typeof(PropAccessor<,>);
        var closed = open.MakeGenericType(targetType, valueType);
        return (IPropAccessor)Activator.CreateInstance(closed, descriptor, validator, getterRef, setterRef)!;
    }

    public static IPropAccessor CreatePropAccessor(Type type, object hostObj, Type targetType, MemberInfo mi, Delegate getter, Delegate setter)
    {
        var name = mi.Name;
        var displayName = mi.Name.SplitCamelCase();
        var description = "";
        var units = "";

        var displayNameAttr = mi.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr != null)
            displayName = displayNameAttr.DisplayName;

        var rangeAttr = mi.GetCustomAttribute<RangeAttribute>();
        var optionsAttr = mi.GetCustomAttribute<OptionsAttribute>();

        return CreatePropAccessor(type, hostObj, targetType, rangeAttr, optionsAttr, name, displayName,
            description, units, getter, setter);
    }

    public static IEnumerable<IPropAccessor> GetPropAccessors(this Type type, object hostObj = null)
    {
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            if (!prop.CanRead)
                continue;
            if (prop.GetIndexParameters().Length != 0)
                continue;

            var setMeth = prop.GetSetMethod(false);
            var isReadOnly = !prop.CanWrite || setMeth == null || setMeth.IsPrivate;

            var getter = prop.GetFastGetter();
            var setter = !isReadOnly ? prop.GetFastSetter() : null;

            yield return CreatePropAccessor(prop.PropertyType, hostObj, type, prop, getter, setter);
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            var isReadOnly = field.IsInitOnly;

            var getter = field.GetFastGetter();
            var setter = !isReadOnly ? field.GetFastSetter() : null;

            yield return CreatePropAccessor(field.FieldType, hostObj, type, field, getter, setter);

        }
    }
}