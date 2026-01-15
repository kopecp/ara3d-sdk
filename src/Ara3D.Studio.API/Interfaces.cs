using Ara3D.Geometry;
using Ara3D.Logging;
using Ara3D.Models;
using Ara3D.Utils;

namespace Ara3D.Studio.API;

public interface IExporter
{
    public void Export(IReadOnlyList<Model3D> models, string filePath);
    public string FileType { get; }
    public string FileExtension { get; }
}

public interface IHostApplication
{
    ILogger Logger { get; }
    void Invalidate(object obj);
    void RefreshUI(object obj);
}

// Implementing this interface assures that your script is called on a regular phases
public interface IAnimated
{ }

public interface IAnimatedModelGenerator : IScriptedCommand, IAnimated
{
    Model3D Eval(EvalContext context);
}

public interface IAnimatedModelModifier : IScriptedComponent, IAnimated
{
    Model3D Eval(Model3D model3D, EvalContext context);
}

/// <summary>
/// A scripted component, is one that is loaded from a plug-in DLL or a C# source file 
/// </summary>
public interface IScriptedComponent
{ }

/// <summary>
/// An asset is a piece of data that was loaded from disk 
/// </summary>
public interface IAsset
{
}

/// <summary>
/// An asset that contains geometry (a mesh, a scene graph, a set of poly-lines) and that is ready to be rendered 
/// </summary>
public interface IRenderableAsset : IAsset, IDisposable
{
    RenderModelData RenderData { get; }
}

/// <summary>
/// An asset that contains a Model3D, which is a collection of instanced triangle meshes.
/// Currently: this excludes point clouds, poly lines, and quad meshes.
/// </summary>
public interface IModelAsset : IAsset
{
    IModel3D Model { get; }
}

/// <summary>
/// An asset that contains a LineMesh3D, which is a collection of lines.
/// </summary>
public interface ILineMeshAsset : IAsset
{
    LineMesh3D Lines { get; }
}

/// <summary>
/// This is an object that can appear in a graph and represents a loaded asset. 
/// </summary>
public interface IAssetSource : IDisposable
{
    IAsset Eval(EvalContext context);
    Task<IAsset> InitialLoad(ILogger logger);
}

/// <summary>
/// This is a 
/// </summary>
public interface ILoader : IScriptedComponent
{
    Task<IRenderableAsset> Load(FilePath filePath, ILogger logger);
}

/// <summary>
/// A script that generates objects
/// </summary>
public interface IGenerator : IScriptedComponent
{ }

/// <summary>
/// A script that generates 3D models  
/// </summary>
public interface IModelGenerator : IGenerator
{
    IModel3D Eval(EvalContext context);
}

/// <summary>
/// A script that generates line meshes
/// </summary>
public interface ILineMeshGenerator : IGenerator
{
    LineMesh3D Eval(EvalContext context);
}

/// <summary>
/// A script that generates line meshes
/// </summary>
public interface IQuadMeshGenerator : IGenerator
{
    QuadMesh3D Eval(EvalContext context);
}


/// <summary>
/// A modifier converts objects into other objects. 
/// </summary>
public interface IModifier : IScriptedComponent
{
    
}

/// <summary>
/// A modifier that converts from models into models. 
/// </summary>
public interface IModelModifier : IModifier
{
    IModel3D Eval(IModel3D model3D, EvalContext context);
}

/// <summary>
/// 
/// </summary>
public interface ILineMeshModifier : IModifier
{
    object Eval(LineMesh3D mesh, EvalContext context);
}

/// <summary>
/// An executable command 
/// </summary>
public interface IScriptedCommand : IScriptedComponent
{
    string Name { get; }
    void Execute(IHostApplication app);
    bool CanExecute(IHostApplication app);
}
