using Ara3D.BimOpenSchema;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using static Ara3D.BimOpenSchema.CommonRevitParameters;
using Document = Autodesk.Revit.DB.Document;

namespace Ara3D.Bowerbird.RevitSamples;

// These are deprecated functions that used to be used, but are now skipped. 
// We may reintroduce them in the future, so I'm keeping them around to make sure that they compile. 
public partial class BosDocumentBuilder
{
    public void DEPRECATED_ProcessConnectors()
    {
        var cms = DEPRECATED_GetConnectorManagers(Document);
        foreach (ConnectorManager cm in cms)
        {
            foreach (Connector conn in cm.Connectors)
            {
                if (!ProcessedConnectors.ContainsKey(conn.Id))
                {
                    ProcessedConnectors.Add(conn.Id, DEPRECATED_ProcessConnector(conn));
                }
            }
        }
    }


    public EntityIndex DEPRECATED_ProcessConnector(Connector conn)
    {
        var entityIndex = DataBuilder.AddEntity(conn.Id, "", DocumentIndex, "", InvalidEntity, InvalidEntity);
        AddDotNetTypeAsParameter(entityIndex, conn);

        try
        {
            // Cache some things we’ll use a lot
            var domain = conn.Domain;
            var shape = conn.Shape;
            bool ownerIsFamilyInstance = conn.Owner is FamilyInstance;

            // --- Basic identity / classification ---
            AddParameter(entityIndex, ConnectorId, conn.Id.ToString());
            AddParameter(entityIndex, ConnectorTypeStr, conn.ConnectorType.ToString());
            AddParameter(entityIndex, ConnectorShape, shape.ToString());
            AddParameter(entityIndex, ConnectorDomain, domain.ToString());
            AddParameter(entityIndex, ConnectorDescription, conn.Description);
            AddParameter(entityIndex, ConnectorOwner, conn.Owner, RelationType.MemberOf);

            // --- Geometry / location ---
            if (conn.ConnectorType == ConnectorType.Physical)
            {
                AddParameter(entityIndex, ConnectorOrigin, conn.Origin);
                AddParameter(entityIndex, ConnectorCoordinateSystem, conn.CoordinateSystem?.ToString());
                AddParameter(entityIndex, ConnectorIsConnected, conn.IsConnected);
                AddParameter(entityIndex, ConnectorIsMovable, conn.IsMovable);
            }

            // Size / geometry-specific (shape guards)
            switch (shape)
            {
                case ConnectorProfileType.Rectangular:
                case ConnectorProfileType.Oval:
                    // Height / Width only valid for rectangular/oval profiles
                    AddParameter(entityIndex, ConnectorHeight, conn.Height);
                    AddParameter(entityIndex, ConnectorWidth, conn.Width);
                    break;

                case ConnectorProfileType.Round:
                    // Radius only valid for round profiles
                    AddParameter(entityIndex, ConnectorRadius, conn.Radius);
                    break;

                default:
                    // Other shapes exist but have no extra scalar size here
                    break;
            }

            AddParameter(entityIndex, ConnectorEngagementLength, conn.EngagementLength);
            AddParameter(entityIndex, ConnectorGasketLength, conn.GasketLength);

            // --- Flow / performance data (assigned vs actual) ---

            // Assigned (design) values – all of these can throw if:
            //  - connector is not in a family instance, or
            //  - connector is in the wrong domain.
            if (ownerIsFamilyInstance)
            {
                // HVAC-only assigned properties
                if (domain == Domain.DomainHvac)
                {
                    AddParameter(entityIndex, ConnectorAssignedDuctFlowConfiguration,
                        conn.AssignedDuctFlowConfiguration.ToString());
                    AddParameter(entityIndex, ConnectorAssignedDuctLossMethod, conn.AssignedDuctLossMethod.ToString());
                    AddParameter(entityIndex, ConnectorAssignedLossCoefficient, conn.AssignedLossCoefficient);
                }

                // Piping-only assigned properties
                if (domain == Domain.DomainPiping)
                {
                    AddParameter(entityIndex, ConnectorAssignedFixtureUnits, conn.AssignedFixtureUnits);
                    AddParameter(entityIndex, ConnectorAssignedKCoefficient, conn.AssignedKCoefficient);
                    AddParameter(entityIndex, ConnectorAssignedPipeFlowConfiguration,
                        conn.AssignedPipeFlowConfiguration.ToString());
                    AddParameter(entityIndex, ConnectorAssignedPipeLossMethod, conn.AssignedPipeLossMethod.ToString());
                    AddParameter(entityIndex, ConnectorDemand, conn.Demand);
                }

                // Shared between HVAC and Piping
                if (domain == Domain.DomainHvac || domain == Domain.DomainPiping)
                {
                    AddParameter(entityIndex, ConnectorAssignedFlowDirection, conn.AssignedFlowDirection.ToString());
                    AddParameter(entityIndex, ConnectorAssignedFlowFactor, conn.AssignedFlowFactor);
                    AddParameter(entityIndex, ConnectorAssignedPressureDrop, conn.AssignedPressureDrop);
                    AddParameter(entityIndex, ConnectorAssignedFlow, conn.AssignedFlow);
                    AddParameter(entityIndex, ConnectorFlow, conn.Flow);
                    AddParameter(entityIndex, ConnectorPressureDrop, conn.PressureDrop);
                    AddParameter(entityIndex, ConnectorVelocityPressure, conn.VelocityPressure);
                    AddParameter(entityIndex, ConnectorCoefficient, conn.Coefficient);
                    AddParameter(entityIndex, ConnectorDirection, conn.Direction.ToString());
                }
            }

            // --- System-type classification (domain-dependent) ---
            if (domain == Domain.DomainHvac)
            {
                AddParameter(entityIndex, ConnectorDuctSystemType, conn.DuctSystemType.ToString());
            }

            if (domain == Domain.DomainPiping)
            {
                AddParameter(entityIndex, ConnectorPipeSystemType, conn.PipeSystemType.ToString());
            }

            if (domain == Domain.DomainElectrical)
            {
                AddParameter(entityIndex, ConnectorElectricalSystemType, conn.ElectricalSystemType.ToString());
            }

            // Utility (int enum in practice, safe)
            AddParameter(entityIndex, ConnectorUtility, conn.Utility);

            // --- Direction / angle / behavior ---
            if (domain == Domain.DomainHvac || domain == Domain.DomainCableTrayConduit || domain == Domain.DomainPiping)
            {
                AddParameter(entityIndex, ConnectorAngle, conn.Angle);
                AddParameter(entityIndex, ConnectorAllowsSlopeAdjustments, conn.AllowsSlopeAdjustments);
            }
        }
        catch (Exception ex)
        {
            AddError(entityIndex, ex);
        }

        return entityIndex;
    }
    public static ConnectorManager DEPRECATED_TryGetConnectorManager(Element e)
    {
        switch (e)
        {
            case MEPCurve mepCurve:
                return mepCurve.ConnectorManager;

            case FabricationPart fab:
                return fab.ConnectorManager;

            case FamilyInstance fi:
                return fi.MEPModel?.ConnectorManager;

            default:
                return null;
        }
    }

    public static IEnumerable<ConnectorManager> DEPRECATED_GetConnectorManagers(Document doc)
    {
        // FamilyInstances with MEPModel
        foreach (var fi in doc.GetElements<FamilyInstance>())
        {
            var cm = fi.MEPModel?.ConnectorManager;
            if (cm != null)
                yield return cm;
        }

        // All MEPCurves (Pipes, Ducts, etc.)
        foreach (var curve in doc.GetElements<MEPCurve>())
        {
            var cm = curve.ConnectorManager;
            if (cm != null)
                yield return cm;
        }

        // Fabrication parts, if you're using them
        foreach (var fab in doc.GetElements<FabricationPart>())
        {
            var cm = fab.ConnectorManager;
            if (cm != null)
                yield return cm;
        }
    }

    public void DEPRECATED_ProcessSpace(EntityIndex ei, Space space)
    {
        // Actual loads / airflows
        AddParameter(ei, SpaceActualExhaustAirflow, space.ActualExhaustAirflow);
        AddParameter(ei, SpaceActualHVACLoad, space.ActualHVACLoad);
        AddParameter(ei, SpaceActualLightingLoad, space.ActualLightingLoad);
        AddParameter(ei, SpaceActualOtherLoad, space.ActualOtherLoad);
        AddParameter(ei, SpaceActualPowerLoad, space.ActualPowerLoad);
        AddParameter(ei, SpaceActualReturnAirflow, space.ActualReturnAirflow);
        AddParameter(ei, SpaceActualSupplyAirflow, space.ActualSupplyAirflow);

        // Air changes / people / illumination
        AddParameter(ei, SpaceAirChangesPerHour, space.AirChangesPerHour);
        AddParameter(ei, SpaceAreaPerPerson, space.AreaperPerson);
        AddParameter(ei, SpaceAverageEstimatedIllumination, space.AverageEstimatedIllumination);

        // Base / limit / height
        AddParameter(ei, SpaceBaseHeatLoadOn, space.BaseHeatLoadOn.ToString());
        AddParameter(ei, SpaceBaseOffset, space.BaseOffset);
        AddParameter(ei, SpaceLimitOffset, space.LimitOffset);
        AddParameter(ei, SpaceUnboundedHeight, space.UnboundedHeight);

        // Calculated loads / flows
        // NOTE: may or may not be computed. 
        AddParameter(ei, SpaceCalculatedCoolingLoad, () => space.CalculatedCoolingLoad);
        AddParameter(ei, SpaceCalculatedHeatingLoad, () => space.CalculatedHeatingLoad);
        AddParameter(ei, SpaceCalculatedSupplyAirflow, () => space.CalculatedSupplyAirflow);

        // Reflectances
        AddParameter(ei, SpaceCeilingReflectance, space.CeilingReflectance);
        AddParameter(ei, SpaceFloorReflectance, space.FloorReflectance);
        AddParameter(ei, SpaceWallReflectance, space.WallReflectance);

        // Condition / units / type (enums → string)
        AddParameter(ei, SpaceConditionType, space.ConditionType.ToString());
        AddParameter(ei, SpaceLightingLoadUnit, space.LightingLoadUnit.ToString());
        AddParameter(ei, SpacePowerLoadUnit, space.PowerLoadUnit.ToString());
        AddParameter(ei, SpaceOccupancyUnit, space.OccupancyUnit.ToString());
        AddParameter(ei, SpaceOutdoorAirFlowStandard, space.OutdoorAirFlowStandard.ToString());
        AddParameter(ei, SpaceSpaceType, space.SpaceType.ToString());

        // Design loads / flows
        AddParameter(ei, SpaceDesignCoolingLoad, space.DesignCoolingLoad);
        AddParameter(ei, SpaceDesignExhaustAirflow, space.DesignExhaustAirflow);
        AddParameter(ei, SpaceDesignHeatingLoad, space.DesignHeatingLoad);
        AddParameter(ei, SpaceDesignHVACLoadPerArea, space.DesignHVACLoadperArea);
        AddParameter(ei, SpaceDesignLightingLoad, space.DesignLightingLoad);
        AddParameter(ei, SpaceDesignOtherLoadPerArea, space.DesignOtherLoadperArea);
        AddParameter(ei, SpaceDesignPowerLoad, space.DesignPowerLoad);
        AddParameter(ei, SpaceDesignReturnAirflow, space.DesignReturnAirflow);
        AddParameter(ei, SpaceDesignSupplyAirflow, space.DesignSupplyAirflow);

        // People / gains
        AddParameter(ei, SpaceNumberOfPeople, space.NumberofPeople);
        AddParameter(ei, SpaceLatentHeatGainPerPerson, space.LatentHeatGainperPerson);
        AddParameter(ei, SpaceSensibleHeatGainPerPerson, space.SensibleHeatGainperPerson);

        // Outdoor air / ventilation
        AddParameter(ei, SpaceOutdoorAirflow, space.OutdoorAirflow);
        AddParameter(ei, SpaceOutdoorAirPerArea, space.OutdoorAirPerArea);
        AddParameter(ei, SpaceOutdoorAirPerPerson, space.OutdoorAirPerPerson);

        // Misc numeric
        AddParameter(ei, SpaceLightingCalculationWorkplane, space.LightingCalculationWorkplane);
        AddParameter(ei, SpaceReturnAirflow, space.ReturnAirflow.ToString());
        AddParameter(ei, SpaceSpaceCavityRatio, space.SpaceCavityRatio);
        AddParameter(ei, SpaceVolume, space.Volume);

        // Booleans
        AddParameter(ei, SpaceOccupiable, space.Occupiable);
        AddParameter(ei, SpacePlenum, space.Plenum);

        AddParameter(ei, SpaceUpperLimit, space.UpperLimit);
        AddParameter(ei, SpaceRoom, space.Room);
        AddParameter(ei, SpaceSpaceTypeElement, space.SpaceTypeId);
        AddParameter(ei, SpaceZone, space.Zone);
    }


    public void DEPRECATED_ProcessArea(EntityIndex ei, Area area)
    {
        AddParameter(ei, AreaSchemeRevitParameterDesc, area.AreaScheme);
        AddParameter(ei, AreaIsGrossInterior, area.IsGrossInterior);
    }



    public void DEPRECATED_ProcessMepSystem(EntityIndex ei, MEPSystem sys)
    {
        // TODO: this should be an option.

        AddParameter(ei, MepSystemBaseEquipment, sys.BaseEquipment);
        AddParameter(ei, MepSystemBaseEquipmentConnector, sys.BaseEquipmentConnector);
        AddParameter(ei, MepSystemHasDesignParts, sys.HasDesignParts);
        AddParameter(ei, MepSystemHasFabricationParts, sys.HasFabricationParts);
        AddParameter(ei, MepSystemHasPlaceholders, sys.HasPlaceholders);
        AddParameter(ei, MepSystemIsEmpty, sys.IsEmpty);
        AddParameter(ei, MepSystemIsMultipleNetwork, sys.IsMultipleNetwork);
        AddParameter(ei, MepSystemIsValid, sys.IsValid);
        AddParameter(ei, MepSystemSectionsCount, sys.SectionsCount);

        foreach (Element terminal in sys.Elements)
        {
            if (terminal == null) continue;
            var terminalEntityIndex = ProcessElement(terminal.Id);
            DataBuilder.AddRelation(terminalEntityIndex, ei, RelationType.MemberOf);
        }

        TryProcessAs<ElectricalSystem>(sys, ei, DEPRECATED_ProcessElectricalSystem);
        TryProcessAs<MechanicalSystem>(sys, ei, DEPRECATED_ProcessMechanicalSystem);
        TryProcessAs<PipingSystem>(sys, ei, DEPRECATED_ProcessPipingSystem);
    }

    public void DEPRECATED_ProcessMechanicalSystem(EntityIndex ei, MechanicalSystem ms)
    {
        AddParameter(ei, MechSystemType, ms.SystemType.ToString());
        AddParameter(ei, MechSystemIsWellConnected, ms.IsWellConnected);

        foreach (Element duct in ms.DuctNetwork)
        {
            if (duct == null) continue;
            var ductEntityIndex = ProcessElement(duct.Id);
            DataBuilder.AddRelation(ductEntityIndex, ei, RelationType.MemberOf);
        }
    }

    public void DEPRECATED_ProcessElectricalSystem(EntityIndex ei, ElectricalSystem es)
    {
        AddParameter(ei, ElecSystemType, es.SystemType.ToString());
        AddParameter(ei, ElecSystemBalancedLoad, es.BalancedLoad);
        AddParameter(ei, ElecSystemCircuitConnectionType, es.CircuitConnectionType.ToString());
        AddParameter(ei, ElecSystemCircuitType, es.CircuitType.ToString());
        AddParameter(ei, ElecSystemCircuitNumber, es.CircuitNumber);
        AddParameter(ei, ElecSystemFrame, es.Frame);
        AddParameter(ei, ElecSystemHasCustomCircuitPath, es.HasCustomCircuitPath);
        AddParameter(ei, ElecSystemHotConductorsNumber, es.HotConductorsNumber);
        AddParameter(ei, ElecSystemIsBasePanelFeedThroughLugsOccupied, es.IsBasePanelFeedThroughLugsOccupied);
        AddParameter(ei, ElecSystemLoadClassificationAbbreviations, es.LoadClassificationAbbreviations);
        AddParameter(ei, ElecSystemLoadClassifications, es.LoadClassifications);
        AddParameter(ei, ElecSystemLoadName, es.LoadName);
        AddParameter(ei, ElecSystemNeutralConductorsNumber, es.NeutralConductorsNumber);
        AddParameter(ei, ElecSystemPanelName, es.PanelName);
        AddParameter(ei, ElecSystemPhaseLabel, es.PhaseLabel);
        AddParameter(ei, ElecSystemPolesNumber, es.PolesNumber);
        AddParameter(ei, ElecSystemPowerFactor, es.PowerFactor);
        AddParameter(ei, ElecSystemPowerFactorState, es.PowerFactorState.ToString());
        AddParameter(ei, ElecSystemRating, es.Rating);
        AddParameter(ei, ElecSystemRunsNumber, es.RunsNumber);
        AddParameter(ei, ElecSystemSlotIndex, es.SlotIndex);
        AddParameter(ei, ElecSystemStartSlot, es.StartSlot);
        AddParameter(ei, ElecSystemVoltage, es.Voltage);
        AddParameter(ei, ElecSystemWays, es.Ways);
        AddParameter(ei, ElecSystemWireType, es.WireType.ToString());

        AddParameter(ei, ElecSystemLength, () => es.Length);

        // Power-only metrics
        if (es.SystemType == ElectricalSystemType.PowerCircuit)
        {
            AddParameter(ei, ElecSystemApparentCurrent, es.ApparentCurrent);
            AddParameter(ei, ElecSystemApparentLoad, es.ApparentLoad);
            AddParameter(ei, ElecSystemTrueCurrent, es.TrueCurrent);
            AddParameter(ei, ElecSystemTrueLoad, es.TrueLoad);
        }
    }

    public void DEPRECATED_ProcessPipingSystem(EntityIndex ei, PipingSystem ps)
    {
        var pipesAndFittings = ps.PipingNetwork;

        foreach (Element pipeOrFitting in pipesAndFittings)
        {
            if (pipeOrFitting == null) continue;
            var pipeElementIndex = ProcessElement(pipeOrFitting.Id);
            DataBuilder.AddRelation(pipeElementIndex, ei, RelationType.MemberOf);
        }

        AddParameter(ei, PipingSystemTypeStr, ps.SystemType.ToString());
    }

    public void DEPRECATED_ProcessZone(EntityIndex entityIndex, Zone z)
    {
        AddParameter(entityIndex, ZoneArea, z.Area);
        AddParameter(entityIndex, ZoneCoolingAirTemperature, z.CoolingAirTemperature);
        AddParameter(entityIndex, ZoneCoolingSetPoint, z.CoolingSetPoint);
        AddParameter(entityIndex, ZoneDehumidificationSetPoint, z.DehumidificationSetPoint);
        AddParameter(entityIndex, ZoneGrossArea, z.GrossArea);
        AddParameter(entityIndex, ZoneGrossVolume, z.GrossVolume);
        AddParameter(entityIndex, ZoneHeatingAirTemperature, z.HeatingAirTemperature);
        AddParameter(entityIndex, ZoneHeatingSetPoint, z.HeatingSetPoint);
        AddParameter(entityIndex, ZonePerimeter, z.Perimeter);
        AddParameter(entityIndex, ZoneServiceType, () => z.ServiceType.ToString());

        var spaces = z.Spaces;
        foreach (Space space in spaces)
        {
            if (space == null) continue;
            var spaceIndex = ProcessElement(space.Id);
            DataBuilder.AddRelation(entityIndex, spaceIndex, RelationType.ContainedIn);
        }
    }
}