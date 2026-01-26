using Ara3D.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Ara3D.Studio.API;

namespace Ara3D.ScriptService;

public enum ScriptType
{
    Generator,
    Modifier,
    Command
}

public class Script
{
    public Script(Type type, FilePath source)
    {
        if (type == null) 
            throw new ArgumentNullException(nameof(type));
        Type = type;

        if (typeof(IGenerator).IsAssignableFrom(type)) 
            ScriptType = ScriptType.Generator;
        if (typeof(IModifier).IsAssignableFrom(type)) 
            ScriptType = ScriptType.Modifier;
        if (typeof(IScriptedCommand).IsAssignableFrom(type)) 
            ScriptType = ScriptType.Command;

        Source = source;
        Name = Type.Name.SplitCamelCase(); ;

        var catAttr = Type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
        Category = catAttr?.Category ?? "";

        var nameAttr = Type.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
        Name = nameAttr?.DisplayName ?? Name;

        if (HasDefaultCtor)
        {
            try
            {
                DefaultValue = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }

    public ScriptType ScriptType { get; }
    public string Category { get; }
    public string Name { get; }
    public bool HasDefaultCtor => Type.HasDefaultConstructor();
    public string ErrorMessage { get; }
    public object DefaultValue { get; }
    public Type Type { get; }
    public FilePath Source { get; }
    public bool HasValue => DefaultValue != null;
}