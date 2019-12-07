using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Solution;
using AgBMPTool.DBModel.Model.Type;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgBMPTool.BLL.Services.Utilities
{
    public class DBToMemory : IDBToMemory
    {
        private readonly IUnitOfWork _uow;

        public DBToMemory(IUnitOfWork _iUnitOfWork, int userId)
        {
            this._uow = _iUnitOfWork;

            Projects = _uow.GetRepository<Project>().Get().ToList();
            ModelComponents = _uow.GetRepository<ModelComponent>().Get().ToList();
            Scenarios = _uow.GetRepository<Scenario>().Get().ToList();
            UnitScenarios = _uow.GetRepository<UnitScenario>().Get().ToList();
            UnitScenarioEffectivenesses = _uow.GetRepository<UnitScenarioEffectiveness>().Get().ToList();
            BMPEffectivenessTypes = _uow.GetRepository<BMPEffectivenessType>().Get().ToList();
            BMPCombinationBMPTypes = _uow.GetRepository<BMPCombinationBMPTypes>().Get().ToList();
            ScenarioModelResultTypes = _uow.GetRepository<ScenarioModelResultType>().Get().ToList();
            ScenarioModelResults = _uow.GetRepository<ScenarioModelResult>().Get().ToList();
            SubAreas = _uow.GetRepository<SubArea>().Get().ToList();
            SolutionLegalSubDivisions = _uow.GetRepository<SolutionLegalSubDivisions>().Get().ToList();
            SolutionParcels = _uow.GetRepository<SolutionParcels>().Get().ToList();
            SolutionModelComponents = _uow.GetRepository<SolutionModelComponents>().Get().ToList();
            UnitTypes = _uow.GetRepository<UnitType>().Get().ToList();
            BMPTypes = _uow.GetRepository<BMPType>().Get().ToList();
            ModelComponentTypes = _uow.GetRepository<ModelComponentType>().Get().ToList();
            ProjectWatersheds = _uow.GetRepository<ProjectWatersheds>().Get().ToList();
            ProjectMunicipalities = _uow.GetRepository<ProjectMunicipalities>().Get().ToList();
            Watersheds = _uow.GetRepository<Watershed>().Get().ToList();
            IsolatedWetlands = _uow.GetRepository<IsolatedWetland>().Get().ToList();
            RiparianWetlands = _uow.GetRepository<RiparianWetland>().Get().ToList();
            Lakes = _uow.GetRepository<Lake>().Get().ToList();
            VegetativeFilterStrips = _uow.GetRepository<VegetativeFilterStrip>().Get().ToList();
            RiparianBuffers = _uow.GetRepository<RiparianBuffer>().Get().ToList();
            GrassedWaterways = _uow.GetRepository<GrassedWaterway>().Get().ToList();
            Reservoirs = _uow.GetRepository<Reservoir>().Get().ToList();
            SmallDams = _uow.GetRepository<SmallDam>().Get().ToList();
            Wascobs = _uow.GetRepository<Wascob>().Get().ToList();
            Dugouts = _uow.GetRepository<Dugout>().Get().ToList();
            Feedlots = _uow.GetRepository<Feedlot>().Get().ToList();
            CatchBasins = _uow.GetRepository<CatchBasin>().Get().ToList();
            ManureStorages = _uow.GetRepository<ManureStorage>().Get().ToList();
            FlowDiversions = _uow.GetRepository<FlowDiversion>().Get().ToList();
            ClosedDrains = _uow.GetRepository<ClosedDrain>().Get().ToList();
            RockChutes = _uow.GetRepository<RockChute>().Get().ToList();
            PointSources = _uow.GetRepository<PointSource>().Get().ToList();
            Reaches = _uow.GetRepository<Reach>().Get().ToList();

            Investors = _uow.GetRepository<Investor>().Get().ToList();
            WatershedExistingBMPs = _uow.GetRepository<WatershedExistingBMPType>().Get().ToList();
        }
    }
}
