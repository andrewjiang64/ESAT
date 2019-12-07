using AgBMPTool.BLL.DLLException;
using AgBMPTool.DBModel.Model.Boundary;
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
using System.Reflection;
using System.Text;

namespace AgBMPTool.BLL.Services.Utilities
{
    public class DBToMemoryUser : IDBToMemory
    {
        private readonly IUnitOfWork _uow;

        public DBToMemoryUser(IUnitOfWork _iUnitOfWork, int userId)
        {
            this._uow = _iUnitOfWork;

            ModelComponentTypes = _uow.GetRepository<ModelComponentType>().Get().ToList();
            BMPEffectivenessTypes = _uow.GetRepository<BMPEffectivenessType>().Get().ToList();
            BMPCombinationBMPTypes = _uow.GetRepository<BMPCombinationBMPTypes>().Get().ToList();
            BMPCombinationTypes = _uow.GetRepository<BMPCombinationType>().Get().ToList();
            ScenarioModelResultTypes = _uow.GetRepository<ScenarioModelResultType>().Get().ToList();
            UnitTypes = _uow.GetRepository<UnitType>().Get().ToList();
            BMPTypes = _uow.GetRepository<BMPType>().Get().ToList();

            User = _uow.GetRepository<User>().Get(x => x.Id == userId).FirstOrDefault();

            HashSet<int> mcIds = this.GetAllModelComponentIdsBySubAreaIds(User.Id, User.UserTypeId);

            Projects = _uow.GetRepository<Project>().Get(x => x.UserId == userId).ToList();

            HashSet<int> projectIds = Projects.Select(x => x.Id).ToHashSet();

            ProjectWatersheds = _uow.GetRepository<ProjectWatersheds>().Get(x => projectIds.Contains(x.ProjectId)).ToList();

            ProjectMunicipalities = _uow.GetRepository<ProjectMunicipalities>().Get(x => projectIds.Contains(x.ProjectId)).ToList();

            SubAreas = _uow.GetRepository<SubArea>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();

            ModelComponents = _uow.GetRepository<ModelComponent>().Get(x => mcIds.Contains(x.Id)).ToList();

            Watersheds = _uow.GetRepository<Watershed>().Get(x => ModelComponents.Select(xx => xx.WatershedId).Contains(x.Id)).ToList();
            HashSet<int> watershedIds = ModelComponents.Select(xx => xx.WatershedId).Distinct().ToHashSet();

            Scenarios = _uow.GetRepository<Scenario>().Get(x => watershedIds.Contains(x.WatershedId)).ToList();
            HashSet<int> scenarioIds = Scenarios.Select(x => x.Id).ToHashSet();

            UnitScenarios = _uow.GetRepository<UnitScenario>().Get(x => scenarioIds.Contains(x.ScenarioId)).ToList();
            HashSet<int> unitScenarioIds = UnitScenarios.Select(x => x.Id).ToHashSet();

            UnitScenarioEffectivenesses = _uow.GetRepository<UnitScenarioEffectiveness>().Get(x => unitScenarioIds.Contains(x.UnitScenarioId)).ToList();

            ScenarioModelResults = _uow.GetRepository<ScenarioModelResult>().Get(x => scenarioIds.Contains(x.ScenarioId)).ToList();

            HashSet<int> solutionIds = _uow.GetRepository<Solution>().Get(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToHashSet();

            SolutionLegalSubDivisions = _uow.GetRepository<SolutionLegalSubDivisions>().Get(x => solutionIds.Contains(x.SolutionId)).ToList();
            SolutionParcels = _uow.GetRepository<SolutionParcels>().Get(x => solutionIds.Contains(x.SolutionId)).ToList();
            SolutionModelComponents = _uow.GetRepository<SolutionModelComponents>().Get(x => solutionIds.Contains(x.SolutionId)).ToList();

            IsolatedWetlands = _uow.GetRepository<IsolatedWetland>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            RiparianWetlands = _uow.GetRepository<RiparianWetland>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Lakes = _uow.GetRepository<Lake>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            VegetativeFilterStrips = _uow.GetRepository<VegetativeFilterStrip>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            RiparianBuffers = _uow.GetRepository<RiparianBuffer>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            GrassedWaterways = _uow.GetRepository<GrassedWaterway>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Reservoirs = _uow.GetRepository<Reservoir>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            SmallDams = _uow.GetRepository<SmallDam>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Wascobs = _uow.GetRepository<Wascob>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Dugouts = _uow.GetRepository<Dugout>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Feedlots = _uow.GetRepository<Feedlot>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            CatchBasins = _uow.GetRepository<CatchBasin>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            ManureStorages = _uow.GetRepository<ManureStorage>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            FlowDiversions = _uow.GetRepository<FlowDiversion>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            ClosedDrains = _uow.GetRepository<ClosedDrain>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            RockChutes = _uow.GetRepository<RockChute>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            PointSources = _uow.GetRepository<PointSource>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
            Reaches = _uow.GetRepository<Reach>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();

            Investors = _uow.GetRepository<Investor>().Get().ToList();
            WatershedExistingBMPs = _uow.GetRepository<WatershedExistingBMPType>().Get(x => mcIds.Contains(x.ModelComponentId)).ToList();
        }

        public readonly User User;

        private HashSet<int> GetSubAreaIds(int projectId, int userId, int userTypeId)
        {
            if (userTypeId == (int)Enumerators.Enumerators.UserType.MunicipalityManager)
            {
                return (from um in _uow.GetRepository<UserMunicipalities>().Query()
                        from sa in _uow.GetRepository<SubArea>().Query()
                        join otherM in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals otherM.Id
                        where um.UserId == userId && otherM.Geometry.Intersects(sa.Geometry)
                        select sa.Id)
                        .Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.WatershedManager)
            {
                return (from uw in _uow.GetRepository<UserWatersheds>().Query()
                        from sa in _uow.GetRepository<SubArea>().Query()
                        join otherW in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals otherW.Id
                        where uw.UserId == userId && otherW.Geometry.Intersects(sa.Geometry)
                        select sa.Id)
                        .Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.Farmer)
            {
                return (from up in _uow.GetRepository<UserParcels>().Query()
                        from sa in _uow.GetRepository<SubArea>().Query()
                        join otherP in _uow.GetRepository<Parcel>().Query() on up.ParcelId equals otherP.Id
                        where up.UserId == userId && otherP.Geometry.Intersects(sa.Geometry)
                        select sa.Id).Distinct().ToHashSet();
            }

            throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"User type not supported!");
        }

        private HashSet<int> GetAllModelComponentIdsBySubAreaIds(int userId, int userTypeId)
        {
            HashSet<int> subAreaIds = new HashSet<int>();

            if (userTypeId == (int)Enumerators.Enumerators.UserType.MunicipalityManager)
            {
                subAreaIds = (from um in _uow.GetRepository<UserMunicipalities>().Query()
                              from sa in _uow.GetRepository<SubArea>().Query()
                              join otherM in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals otherM.Id
                              where um.UserId == userId && otherM.Geometry.Intersects(sa.Geometry)
                              select sa.Id)
                        .Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.WatershedManager)
            {
                subAreaIds = (from uw in _uow.GetRepository<UserWatersheds>().Query()
                              from sa in _uow.GetRepository<SubArea>().Query()
                              join otherW in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals otherW.Id
                              where uw.UserId == userId && otherW.Geometry.Intersects(sa.Geometry)
                              select sa.Id)
                        .Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.Farmer)
            {
                subAreaIds = (from up in _uow.GetRepository<UserParcels>().Query()
                              from sa in _uow.GetRepository<SubArea>().Query()
                              join otherP in _uow.GetRepository<Parcel>().Query() on up.ParcelId equals otherP.Id
                              where up.UserId == userId && otherP.Geometry.Intersects(sa.Geometry)
                              select sa.Id).Distinct().ToHashSet();
            }
            else
            {
                throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                            // following is the message send to console
                            $"User type not supported!");
            }

            return (from bmp in _uow.GetRepository<SubArea>().Query()
                    where subAreaIds.Contains(bmp.Id)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<IsolatedWetland>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<RiparianWetland>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<VegetativeFilterStrip>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<RiparianBuffer>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<GrassedWaterway>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<FlowDiversion>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<Reservoir>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<Wascob>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<Dugout>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<CatchBasin>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<Feedlot>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<ManureStorage>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<RockChute>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<PointSource>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<ClosedDrain>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<SmallDam>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId)
             .Union(from bmp in _uow.GetRepository<Lake>().Query()
                    where subAreaIds.Contains(bmp.SubAreaId)
                    select bmp.ModelComponentId).Distinct().ToHashSet();
        }
    }
}
