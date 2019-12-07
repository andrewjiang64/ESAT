using AgBMPTool.BLL.Enumerators;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL.Interface;
using ESAT.Import.ESATException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESAT.Import.Model
{
    public class WatershedExistingBMPTypeFactory : IWatershedExistingBMPTypeFactory
    {
        private readonly IUnitOfWork _uow;

        public WatershedExistingBMPTypeFactory(IUnitOfWork _iUnitOfWork)
        {
            this._uow = _iUnitOfWork;
        }

        public List<WatershedExistingBMPTypeDTO> BuildWatershedExistingBMPTypeDTOs(int watershedId, int scenarioTypeId, int investorId)
        {
            int largestSingleBMPId = _uow.GetRepository<BMPType>().Get().Select(o => o.Id).Max();

            // Get conventional Except existing modelcomponentId-BMPTypeId
            var mcIdBMPTypeId = (from u in _uow.GetRepository<UnitScenario>().Query()
                                 join s in _uow.GetRepository<Scenario>().Query() on u.ScenarioId equals s.Id
                                 join mc in _uow.GetRepository<ModelComponent>().Query() on u.ModelComponentId equals mc.Id
                                 where mc.WatershedId == watershedId && u.BMPCombinationId <= largestSingleBMPId && s.ScenarioTypeId == 1
                                 select $"{u.ModelComponentId}_{u.BMPCombinationId}")
                                .Except
                                (from u in _uow.GetRepository<UnitScenario>().Query()
                                 join s in _uow.GetRepository<Scenario>().Query() on u.ScenarioId equals s.Id
                                 join mc in _uow.GetRepository<ModelComponent>().Query() on u.ModelComponentId equals mc.Id
                                 where mc.WatershedId == watershedId && u.BMPCombinationId <= largestSingleBMPId && s.ScenarioTypeId == scenarioTypeId
                                 select $"{u.ModelComponentId}_{u.BMPCombinationId}").ToList();

            // Build List<WatershedExistingBMPTypeDTO>
            List<WatershedExistingBMPTypeDTO> res = new List<WatershedExistingBMPTypeDTO>();

            foreach (string s in mcIdBMPTypeId)
            {
                string[] tokens = s.Split("_");

                res.Add(new WatershedExistingBMPTypeDTO
                {
                    ModelComponentId = Convert.ToInt16(tokens[0]),
                    BMPTypeId = Convert.ToInt16(tokens[1]),
                    ScenarioTypeId = scenarioTypeId,
                    InvestorId = investorId
                });
            }

            return res;
        }

        private HashSet<int> GetSubAreaIds(int projectId, int userId, int userTypeId)
        {
            if (userTypeId == (int)Enumerators.UserType.MunicipalityManager)
            {
                return (from pm in _uow.GetRepository<ProjectMunicipalities>().Query()
                        from sa in _uow.GetRepository<SubArea>().Query()
                        join otherM in _uow.GetRepository<Municipality>().Query() on pm.MunicipalityId equals otherM.Id
                        where pm.ProjectId == projectId && otherM.Geometry.Intersects(sa.Geometry)
                        select sa.Id).Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.UserType.WatershedManager)
            {
                return (from pw in _uow.GetRepository<ProjectWatersheds>().Query()
                        from sa in _uow.GetRepository<SubArea>().Query()
                        join otherW in _uow.GetRepository<Watershed>().Query() on pw.WatershedId equals otherW.Id
                        where pw.ProjectId == projectId && otherW.Geometry.Intersects(sa.Geometry)
                        select sa.Id).Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.UserType.Farmer)
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

        private HashSet<int> GetAllModelComponentIdsBySubAreaIds(HashSet<int> subAreaIds)
        {
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


        public HashSet<int> GetScenarioIdsBySubAreaIds(HashSet<int> subAreaIds, int scenarioTypeId)
        {
            return (from sa in _uow.GetRepository<SubArea>().Query()
                    join sb in _uow.GetRepository<Subbasin>().Query() on sa.SubbasinId equals sb.Id
                    join sw in _uow.GetRepository<SubWatershed>().Query() on sb.SubWatershedId equals sw.Id
                    join s in _uow.GetRepository<Scenario>().Query() on sw.WatershedId equals s.WatershedId
                    where subAreaIds.Contains(sa.Id) && s.ScenarioTypeId == scenarioTypeId
                    select s.Id).Distinct().ToHashSet();
        }
    }
}
