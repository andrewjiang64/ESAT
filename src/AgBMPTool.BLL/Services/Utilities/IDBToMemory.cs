using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Solution;
using AgBMPTool.DBModel.Model.Type;
using AgBMPTool.DBModel.Model.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Services.Utilities
{
    public abstract class IDBToMemory
    {
        public IEnumerable<Project> Projects;
        public IEnumerable<ModelComponent> ModelComponents;
        public IEnumerable<Scenario> Scenarios;
        public IEnumerable<UnitScenario> UnitScenarios;
        public IEnumerable<UnitScenarioEffectiveness> UnitScenarioEffectivenesses;
        public IEnumerable<BMPEffectivenessType> BMPEffectivenessTypes;
        public IEnumerable<BMPCombinationType> BMPCombinationTypes;
        public IEnumerable<BMPCombinationBMPTypes> BMPCombinationBMPTypes;
        public IEnumerable<ScenarioModelResultType> ScenarioModelResultTypes;
        public IEnumerable<ScenarioModelResult> ScenarioModelResults;
        public IEnumerable<SubArea> SubAreas;
        public IEnumerable<SolutionLegalSubDivisions> SolutionLegalSubDivisions;
        public IEnumerable<SolutionParcels> SolutionParcels;
        public IEnumerable<SolutionModelComponents> SolutionModelComponents;
        public IEnumerable<UnitType> UnitTypes;
        public IEnumerable<BMPType> BMPTypes;
        public IEnumerable<ModelComponentType> ModelComponentTypes;
        public IEnumerable<ProjectWatersheds> ProjectWatersheds;
        public IEnumerable<ProjectMunicipalities> ProjectMunicipalities;
        public IEnumerable<Watershed> Watersheds;
        public IEnumerable<IsolatedWetland> IsolatedWetlands;
        public IEnumerable<RiparianWetland> RiparianWetlands;
        public IEnumerable<Lake> Lakes;
        public IEnumerable<VegetativeFilterStrip> VegetativeFilterStrips;
        public IEnumerable<GrassedWaterway> GrassedWaterways;
        public IEnumerable<RiparianBuffer> RiparianBuffers;
        public IEnumerable<Reservoir> Reservoirs;
        public IEnumerable<SmallDam> SmallDams;
        public IEnumerable<Wascob> Wascobs;
        public IEnumerable<Dugout> Dugouts;
        public IEnumerable<CatchBasin> CatchBasins;
        public IEnumerable<Feedlot> Feedlots;
        public IEnumerable<ManureStorage> ManureStorages;
        public IEnumerable<FlowDiversion> FlowDiversions;
        public IEnumerable<ClosedDrain> ClosedDrains;
        public IEnumerable<RockChute> RockChutes;
        public IEnumerable<PointSource> PointSources;
        public IEnumerable<Reach> Reaches;
        public IEnumerable<Investor> Investors;
        public IEnumerable<WatershedExistingBMPType> WatershedExistingBMPs;
    }
}
