namespace Ara3D.IfcGeometry;

public class IfcClassifierHelpers
{
    public static HashSet<string> InstanceEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- Spatial structure ---
        "IfcProject",
        "IfcSite",
        "IfcBuilding",
        "IfcBuildingStorey",
        "IfcSpace",

        // --- Architectural elements ---
        "IfcWall",
        "IfcWallStandardCase",
        "IfcSlab",
        "IfcRoof",
        "IfcColumn",
        "IfcBeam",
        "IfcFooting",
        "IfcPile",
        "IfcCurtainWall",
        "IfcCovering",
        "IfcStair",
        "IfcStairFlight",
        "IfcRamp",
        "IfcRampFlight",
        "IfcChimney",

        // --- Openings & components ---
        "IfcDoor",
        "IfcWindow",
        "IfcOpeningElement",
        "IfcShadingDevice",

        // --- Furnishings & equipment ---
        "IfcFurnishingElement",
        "IfcFurniture",
        "IfcSystemFurnitureElement",
        "IfcSanitaryTerminal",

        // --- Structural ---
        "IfcStructuralCurveMember",
        "IfcStructuralSurfaceMember",

        // --- MEP / distribution ---
        "IfcDistributionElement",
        "IfcDistributionFlowElement",
        "IfcFlowSegment",
        "IfcFlowFitting",
        "IfcFlowController",
        "IfcFlowTerminal",
        "IfcFlowMovingDevice",
        "IfcFlowStorageDevice",
        "IfcFlowTreatmentDevice",

        // --- MEP concrete examples ---
        "IfcPipeSegment",
        "IfcPipeFitting",
        "IfcPipeAccessory",
        "IfcDuctSegment",
        "IfcDuctFitting",
        "IfcCableCarrierSegment",
        "IfcCableCarrierFitting",
        "IfcElectricalFixture",
        "IfcLightFixture",
        "IfcAirTerminal",
        "IfcValve",
        "IfcPump",
        "IfcFan",

        // --- Annotation / proxy ---
        "IfcProxy",
        "IfcAnnotation",

        // --- Grouping & zones (still instances) ---
        "IfcZone",
        "IfcGroup",
        "IfcSystem"
    };

    public static HashSet<string> TypeEntities =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Core ---
            "IfcTypeObject",
            "IfcObjectType",
            "IfcElementType",

            // --- Architectural types ---
            "IfcWallType",
            "IfcSlabType",
            "IfcRoofType",
            "IfcBeamType",
            "IfcColumnType",
            "IfcFootingType",
            "IfcCurtainWallType",
            "IfcCoveringType",
            "IfcStairType",
            "IfcRampType",
            "IfcChimneyType",

            // --- Openings & components ---
            "IfcDoorType",
            "IfcWindowType",
            "IfcShadingDeviceType",

            // --- Furnishings ---
            "IfcFurnitureType",
            "IfcSystemFurnitureElementType",
            "IfcSanitaryTerminalType",

            // --- MEP / distribution ---
            "IfcDistributionElementType",
            "IfcDistributionFlowElementType",
            "IfcFlowSegmentType",
            "IfcFlowFittingType",
            "IfcFlowControllerType",
            "IfcFlowTerminalType",
            "IfcFlowMovingDeviceType",
            "IfcFlowStorageDeviceType",
            "IfcFlowTreatmentDeviceType",

            // --- MEP concrete examples ---
            "IfcPipeSegmentType",
            "IfcPipeFittingType",
            "IfcPipeAccessoryType",
            "IfcDuctSegmentType",
            "IfcDuctFittingType",
            "IfcCableCarrierSegmentType",
            "IfcCableCarrierFittingType",
            "IfcElectricalFixtureType",
            "IfcLightFixtureType",
            "IfcAirTerminalType",
            "IfcValveType",
            "IfcPumpType",
            "IfcFanType"
        };
}