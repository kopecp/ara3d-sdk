
using Ara3D.Utils;
using Newtonsoft.Json;

namespace Ara3D.Bowerbird.RevitSamples;

public class BimOpenSchemaExportSettings
{
    public enum DetailLevelEnum
    {
        Coarse = 1,
        Medium = 2,
        Fine = 3,
    }

    [JsonIgnore]
    public DirectoryPath Folder { get; set; } = DefaultFolder;

    public bool IncludeLinks { get; set; } = true;
    public bool IncludeGeometry { get; set; } = true;
    public bool UseCurrentView { get; set; } = false;
    public DetailLevelEnum DetailLevel { get; set; } = DetailLevelEnum.Fine;

    public static DirectoryPath DefaultFolder 
        => SpecialFolders.MyDocuments.RelativeFolder("BIM Open Schema");
}