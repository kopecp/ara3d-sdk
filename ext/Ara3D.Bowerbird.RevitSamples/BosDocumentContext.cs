using Ara3D.BimOpenSchema;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;
using ElementId = Autodesk.Revit.DB.ElementId;

namespace Ara3D.Bowerbird.RevitSamples;

public class BosDocumentContext
{
    public readonly Document Document;
    public readonly BosDocumentContext Parent;
    public readonly RevitLinkInstance LinkInstance;
    public readonly List<long> ElementIds = [];
    public readonly HashSet<long> TypeIds = [];
    public readonly Dictionary<long, EntityIndex> ElementToEntityIndex = new();
    public readonly bool IsLink;
    public readonly string LinkName = "";
    public readonly string Path = "";
    public readonly string ExternalPath = "";
    public readonly string Title = "";
    public readonly bool IsDetached;
    public readonly Transform Transform;

    public BosDocumentContext(Document document, BosDocumentContext parent = null, RevitLinkInstance rli = null)
    {
        if (rli == null || parent == null)
            if (rli != null || parent != null)
                throw new Exception("If either the RevitLinkInstance is null or the parent is null, both must be null");

        Document = document;
        Parent = parent;

        Transform = rli != null 
            ? parent.Transform.Multiply(rli.GetTransform()) 
            : Transform.Identity;

        LinkInstance = rli;
        IsDetached = document.IsDetached;
        Path = document.PathName;
        Title = document.Title;

        if (LinkInstance == null) 
            return;
        
        IsLink = true;
        LinkName = LinkInstance.Name;
        var typeId = LinkInstance.GetTypeId();
        var extRef = ExternalFileUtils.GetExternalFileReference(Parent.Document, typeId);
        var modelPath = extRef.GetPath();
        ExternalPath = modelPath == null 
            ? "" : ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath) ?? "";
    }

    public static BosDocumentContext Create(BosDocumentContext parent, RevitLinkInstance rli)
    {
        var linkDocument = rli.GetLinkDocument();
        return linkDocument != null ? new BosDocumentContext(linkDocument, parent, rli) : null;
    }

    public List<BosDocumentContext> GatherLinkedDocuments()
    {
        var r = new HashSet<BosDocumentContext>();
        GatherLinkedDocuments(r);
        return r.ToList();
    }

    public void GatherLinkedDocuments(HashSet<BosDocumentContext> set)
    {
        if (!set.Add(this))
            return;
        foreach (var link in Document.GetLinks())
        {
            var tmp = Create(this, link);
            tmp?.GatherLinkedDocuments(set);
        }
    }

    public void RetrieveElementIds()
    {
        if (ElementIds.Count > 0)
            throw new Exception("Elements already retrieved");
        var ids = new FilteredElementCollector(Document).WhereElementIsNotElementType().ToElementIds();
        foreach (var id in ids)
        {
            var val = id.Value;
            ElementToEntityIndex.Add(val, (EntityIndex)ElementIds.Count);
            ElementIds.Add(val);
        }
    
        foreach (var id in ElementIds)
        {
            var e = Document.GetElement(new ElementId(id));
            var typeId = e.GetTypeId();
            if (typeId != ElementId.InvalidElementId)
            {
                var val = typeId.Value;
                var index = ElementIds.Count + TypeIds.Count;
                 if (!TypeIds.Add(val))
                    continue;
                ElementToEntityIndex.TryAdd(val, (EntityIndex)index);
            }
        }
    }

    public string Key
        => $"{Path}-{Title}-{ExternalPath}";

    public override int GetHashCode()
        => Key.GetHashCode();

    public override bool Equals(object obj)
        => obj is BosDocumentContext de && Key == de.Key;

    public IEnumerable<Element> GetElements()
    {
        foreach (var eid in ElementIds)
        {
            var e = Document.GetElement(new ElementId(eid));
            if (e != null)
                yield return e;
        }
    }
}