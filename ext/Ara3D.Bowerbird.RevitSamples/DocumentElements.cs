using Ara3D.BimOpenSchema;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;
using ElementId = Autodesk.Revit.DB.ElementId;

namespace Ara3D.Bowerbird.RevitSamples;

public class DocumentElements
{

    public readonly Document Document;
    public readonly Document HostDocument;
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

    public DocumentElements(Document document, Document hostDoc = null, RevitLinkInstance rli = null)
    {
        Document = document;
        HostDocument = hostDoc;
        LinkInstance = rli;
        IsDetached = document.IsDetached;
        Path = document.PathName;
        Title = document.Title;
        if (LinkInstance != null)
        {
            if (hostDoc == null)
                throw new ArgumentNullException("If the RevitLinkInstance is present, the host document must not be null");
            IsLink = true;
            LinkName = rli.Name;
            var typeId = rli.GetTypeId();
            var extRef = ExternalFileUtils.GetExternalFileReference(HostDocument, typeId);
            var modelPath = extRef.GetPath();
            ExternalPath = modelPath == null ? "" : ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath) ?? "";
        }
    }

    public static DocumentElements Create(Document hostDocument, RevitLinkInstance rli)
    {
        var linkDocument = rli.GetLinkDocument();
        return linkDocument != null ? new DocumentElements(linkDocument, hostDocument, rli) : null;
    }

    public List<DocumentElements> GatherLinkedDocuments()
    {
        var r = new HashSet<DocumentElements>();
        GatherLinkedDocuments(r);
        return r.ToList();
    }

    public void GatherLinkedDocuments(HashSet<DocumentElements> set)
    {
        if (!set.Add(this))
            return;
        foreach (var link in Document.GetLinks())
        {
            var tmp = Create(Document, link);
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
    }

    public void RetrieveUsedTypeIds()
    {
        if (TypeIds.Count > 0)
            throw new Exception("Element type ids already retrieved");
        foreach (var id in ElementIds)
        {
            var e = Document.GetElement(new ElementId(id));
            var typeId = e.GetTypeId();
            if (typeId != ElementId.InvalidElementId)
                TypeIds.Add(typeId.Value);
        }
    }

    public string Key
        => $"{Path}-{Title}-{ExternalPath}";

    public override int GetHashCode()
        => Key.GetHashCode();

    public override bool Equals(object obj)
        => obj is DocumentElements de && Key == de.Key;
}