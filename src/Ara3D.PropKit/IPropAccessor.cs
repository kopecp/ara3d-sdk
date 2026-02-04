namespace Ara3D.PropKit;

public interface IPropAccessor
{
    PropDescriptor Descriptor { get; }
    PropConstraints Constraints { get; }
    IPropValidator Validator { get; }
    bool HasSetter { get; }
    object GetValue(object host);
    object SetValue(object host, object value);
}

public static class PropAccessorExtensions
{
    public static PropValue GetPropValue(this IPropAccessor self, object host)
        => new(self.GetValue(host), self.Descriptor);

    public static void SetPropValue(this IPropAccessor self, ref object host, PropValue propValue)
    {
        if (propValue.Descriptor != self.Descriptor)
            throw new Exception("Incorrect descriptor");
        self.SetValue(ref host, propValue.Value);
    }
}