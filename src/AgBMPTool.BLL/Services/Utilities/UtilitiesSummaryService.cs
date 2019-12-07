using AgBMPTool.BLL.DLLException;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.DBModel.Model.Boundary;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Solution;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL.Interface;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Services.Utilities
{
    public class UtilitiesSummaryService : IUtilitiesSummaryService
    {
        public readonly IUnitOfWork _uow;

        public UtilitiesSummaryService(IUnitOfWork _iUnitOfWork)
        {
            this._uow = _iUnitOfWork;
        }

        public IntelligentRecommendationOptimizationDTO GetAllOptimizationBmpLocationsByProjectId(int projectId)
        {
            return new IntelligentRecommendationOptimizationDTO
            {
                ProjectId = projectId,
                UnitSolutions = (from l in _uow.GetRepository<OptimizationLegalSubDivisions>().Get()
                                 join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                 select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.LegalSubDivisionId, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<OptimizationParcels>().Get()
                                    join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ParcelId, LocationType = OptimizationSolutionLocationTypeEnum.Parcel }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<OptimizationModelComponents>().Get()
                                    join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ModelComponentId, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP }).Distinct()
                                  .ToList()
            };
        }

        public IntelligentRecommendationOptimizationDTO GetSelectedOptimizationBmpLocationsByProjectId(int projectId)
        {
            return new IntelligentRecommendationOptimizationDTO
            {
                ProjectId = projectId,
                UnitSolutions = (from l in _uow.GetRepository<OptimizationLegalSubDivisions>().Get(x => x.IsSelected)
                                 join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                 select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.LegalSubDivisionId, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<OptimizationParcels>().Get(x => x.IsSelected)
                                    join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ParcelId, LocationType = OptimizationSolutionLocationTypeEnum.Parcel }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<OptimizationModelComponents>().Get(x => x.IsSelected)
                                    join o in _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId) on l.OptimizationId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ModelComponentId, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP }).Distinct()
                                  .ToList()
            };
        }

        public SolutionDTO GetSelectedSolutionBmpLocationsByProjectId(int projectId)
        {
            return new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = (from l in _uow.GetRepository<SolutionLegalSubDivisions>().Get(x => x.IsSelected)
                                 join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                 select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.LegalSubDivisionId, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<SolutionParcels>().Get(x => x.IsSelected)
                                    join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ParcelId, LocationType = OptimizationSolutionLocationTypeEnum.Parcel }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<SolutionModelComponents>().Get(x => x.IsSelected)
                                    join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ModelComponentId, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP }).Distinct()
                                  .ToList()
            };
        }

        public SolutionDTO GetAllSolutionBmpLocationsByProjectId(int projectId)
        {
            return new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = (from l in _uow.GetRepository<SolutionLegalSubDivisions>().Get()
                                 join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                 select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.LegalSubDivisionId, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<SolutionParcels>().Get()
                                    join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ParcelId, LocationType = OptimizationSolutionLocationTypeEnum.Parcel }).Distinct()
                                 .Union(
                                    from l in _uow.GetRepository<SolutionModelComponents>().Get()
                                    join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                                    select new UnitSolution { BMPTypeId = l.BMPTypeId, LocationId = l.ModelComponentId, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP }).Distinct()
                                  .ToList()
            };
        }

        public HashSet<int> GetSubAreaMCIds(IntelligentRecommendationOptimizationDTO bmpLocations, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)ProjectSpatialUnitTypeEnum.LSD)
            {
                return (
                    from lsd in bmpLocations.OptimizationLegalSubDivisions
                    join sa in _uow.GetRepository<SubArea>().Get() on lsd.LocationId equals sa.LegalSubDivisionId
                    select sa.ModelComponentId
                    ).Distinct().ToHashSet();
            }
            else
            {
                return (
                    from p in bmpLocations.OptimizationParcels
                    join sa in _uow.GetRepository<SubArea>().Get() on p.LocationId equals sa.ParcelId
                    select sa.ModelComponentId
                    ).Distinct().ToHashSet();
            }
        }

        public Dictionary<int, List<int>> BmpCombinationTypeToBMPTypes { get; set; }

        public List<int> GetBMPTypeIdsByBMPCombinationTypeId(int bmpCombinationTypeId)
        {
            if (BmpCombinationTypeToBMPTypes == null)
            {
                var bmps = _uow.GetRepository<BMPCombinationBMPTypes>().Get();

                BmpCombinationTypeToBMPTypes = new Dictionary<int, List<int>>();

                foreach (var b in bmps)
                {
                    if (!BmpCombinationTypeToBMPTypes.ContainsKey(b.BMPCombinationTypeId))
                    {
                        BmpCombinationTypeToBMPTypes[b.BMPCombinationTypeId] = new List<int>();
                    }
                    BmpCombinationTypeToBMPTypes[b.BMPCombinationTypeId].Add(b.BMPTypeId);
                }
            }

            return BmpCombinationTypeToBMPTypes.ContainsKey(bmpCombinationTypeId) ? BmpCombinationTypeToBMPTypes[bmpCombinationTypeId] : new List<int>();
        }

        public int SaveSolution(SolutionOptimizationBase solutionDTO, bool isFromOptimization)
        {
            if (solutionDTO.UnitSolutions.Count > 0 && solutionDTO.GetType() == typeof(SolutionDTO))
            {
                var _solutionDTO = (SolutionDTO) solutionDTO;
                /* Get solution Id*/
                int sId = _uow.GetRepository<Solution>().Get(x => x.ProjectId == _solutionDTO.ProjectId).FirstOrDefault()?.Id ?? 0;

                /* Return false if no solution found */
                if (sId == 0) return -999;

                /* Update IsSelected based on recommended solution */
                var res =
                    _uow.GetRepository<SolutionLegalSubDivisions>().Get(x => x.SolutionId == sId).Select(o =>
                    {
                        o.IsSelected = (_solutionDTO.SolutionLegalSubDivisions.Find(x => x.LocationId == o.LegalSubDivisionId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count +
                    _uow.GetRepository<SolutionParcels>().Get(x => x.SolutionId == sId).Select(o =>
                    {
                        o.IsSelected = (_solutionDTO.SolutionParcels.Find(x => x.LocationId == o.ParcelId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count +
                    _uow.GetRepository<SolutionModelComponents>().Get(x => x.SolutionId == sId).Select(o =>
                    {
                        o.IsSelected = (_solutionDTO.SolutionModelComponents.Find(x => x.LocationId == o.ModelComponentId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count;

                /* if some records are affected */
                if (res > 0)
                {
                    /* Update solution table as FromOptimization = true */
                    _uow.GetRepository<Solution>().Get(x => x.Id == sId)
                        .Select(o => { o.FromOptimization = isFromOptimization; return o; }).Count();

                    /* Update project modified time */
                    _uow.GetRepository<Project>().Get(x => x.Id == _solutionDTO.ProjectId)
                        .Select(o => { o.Modified = DateTime.Now; return o; }).Count();

                    /* Save to database */
                    _uow.Commit();

                    return sId;
                }
                else
                {
                    return -999;
                }
            }
            else /* Otherwise return false */
            {
                return -999;
            }
        }

        public int SaveOptimization(SolutionOptimizationBase optimizationDTO)
        {
            if (optimizationDTO.UnitSolutions.Count > 0 && optimizationDTO.GetType() == typeof(IntelligentRecommendationOptimizationDTO))
            {
                var _optimizationDTO = (IntelligentRecommendationOptimizationDTO)optimizationDTO;

                /* Get solution Id*/
                int oId = _uow.GetRepository<Optimization>().Get(x => x.ProjectId == _optimizationDTO.ProjectId).FirstOrDefault()?.Id ?? 0;

                /* Return false if no solution found */
                if (oId == 0) return -999;

                /* Update IsSelected based on recommended solution */
                var res =
                    _uow.GetRepository<OptimizationLegalSubDivisions>().Get(x => x.OptimizationId == oId).Select(o =>
                    {
                        o.IsSelected = (_optimizationDTO.OptimizationLegalSubDivisions.Find(x => x.LocationId == o.LegalSubDivisionId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count +
                    _uow.GetRepository<OptimizationParcels>().Get(x => x.OptimizationId == oId).Select(o =>
                    {
                        o.IsSelected = (_optimizationDTO.OptimizationParcels.Find(x => x.LocationId == o.ParcelId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count +
                    _uow.GetRepository<OptimizationModelComponents>().Get(x => x.OptimizationId == oId).Select(o =>
                    {
                        o.IsSelected = (_optimizationDTO.OptimizationModelComponents.Find(x => x.LocationId == o.ModelComponentId && x.BMPTypeId == o.BMPTypeId) != null);
                        return o;
                    }).ToList().Count;

                /* if some records are affected */
                if (res > 0)
                {
                    /* Update project modified time */
                    _uow.GetRepository<Project>().Get(x => x.Id == _optimizationDTO.ProjectId)
                        .Select(o => { o.Modified = DateTime.Now; return o; }).Count();

                    /* Save to database */
                    _uow.Commit();

                    return oId;
                }
                else
                {
                    return -999;
                }
            }
            else /* Otherwise return false */
            {
                return -999;
            }
        }

        public HashSet<int> GetSubAreaIds(int projectId, int userId, int userTypeId)
        {
            if (userTypeId == (int)Enumerators.Enumerators.UserType.MunicipalityManager)
            {
                return ((from pm in _uow.GetRepository<ProjectMunicipalities>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherM in _uow.GetRepository<Municipality>().Query() on pm.MunicipalityId equals otherM.Id
                         where pm.ProjectId == projectId && otherM.Geometry.Intersects(sa.Geometry)
                         select sa.Id)
                        .Union
                        (from pw in _uow.GetRepository<ProjectWatersheds>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherW in _uow.GetRepository<Watershed>().Query() on pw.WatershedId equals otherW.Id
                         where pw.ProjectId == projectId && otherW.Geometry.Intersects(sa.Geometry)
                         select sa.Id)
                        .Intersect
                        (from um in _uow.GetRepository<UserMunicipalities>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherM in _uow.GetRepository<Municipality>().Query() on um.MunicipalityId equals otherM.Id
                         where um.UserId == userId && otherM.Geometry.Intersects(sa.Geometry)
                         select sa.Id))
                        .Distinct().ToHashSet();
            }
            else if (userTypeId == (int)Enumerators.Enumerators.UserType.WatershedManager)
            {
                return ((from pm in _uow.GetRepository<ProjectMunicipalities>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherM in _uow.GetRepository<Municipality>().Query() on pm.MunicipalityId equals otherM.Id
                         where pm.ProjectId == projectId && otherM.Geometry.Intersects(sa.Geometry)
                         select sa.Id)
                        .Union
                        (from pw in _uow.GetRepository<ProjectWatersheds>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherW in _uow.GetRepository<Watershed>().Query() on pw.WatershedId equals otherW.Id
                         where pw.ProjectId == projectId && otherW.Geometry.Intersects(sa.Geometry)
                         select sa.Id)
                         .Intersect
                        (from uw in _uow.GetRepository<UserWatersheds>().Query()
                         from sa in _uow.GetRepository<SubArea>().Query()
                         join otherW in _uow.GetRepository<Watershed>().Query() on uw.WatershedId equals otherW.Id
                         where uw.UserId == userId && otherW.Geometry.Intersects(sa.Geometry)
                         select sa.Id))
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

        public HashSet<int> GetAllModelComponentIdsBySubAreaIds(HashSet<int> subAreaIds)
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

        public HashSet<int> GetScenarioIdsBySubAreaMcIds(HashSet<int> subAreaMCIds, int scenarioTypeId)
        {
            return (from mc in _uow.GetRepository<ModelComponent>().Query()
                    join s in _uow.GetRepository<Scenario>().Query() on mc.WatershedId equals s.WatershedId
                    where subAreaMCIds.Contains(mc.Id) && s.ScenarioTypeId == scenarioTypeId
                    select s.Id).Distinct().ToHashSet();
        }        

        public HashSet<int> GetAllScenarioIdsByProjectId(int projectId, int scenTypeId)
        {
            return (
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get(uos => uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision)
                join l in _uow.GetRepository<SolutionLegalSubDivisions>().Get() on uos.LocationId equals l.LegalSubDivisionId
                join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Get(s => s.ScenarioTypeId == scenTypeId) on uos.ScenarioId equals s.Id
                select uos.ScenarioId)
                .Union(
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get(uos => uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.Parcel)
                join l in _uow.GetRepository<SolutionParcels>().Get() on uos.LocationId equals l.ParcelId
                join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Get(s => s.ScenarioTypeId == scenTypeId) on uos.ScenarioId equals s.Id
                select uos.ScenarioId)
                .Union(
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get(uos => uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                join l in _uow.GetRepository<SolutionModelComponents>().Get() on uos.LocationId equals l.ModelComponentId
                join o in _uow.GetRepository<Solution>().Get(x => x.ProjectId == projectId) on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Get(s => s.ScenarioTypeId == scenTypeId) on uos.ScenarioId equals s.Id
                select uos.ScenarioId)
                .Distinct()
                .ToHashSet();
        }

        public HashSet<int> GetSelectedScenarioIdsByProjectId(int projectId, int scenTypeId)
        {
            return (
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get()
                join l in _uow.GetRepository<SolutionLegalSubDivisions>().Get() on uos.LocationId equals l.LegalSubDivisionId
                join o in _uow.GetRepository<Solution>().Get() on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Query() on uos.ScenarioId equals s.Id
                where o.ProjectId == projectId && l.IsSelected && uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision && s.ScenarioTypeId == scenTypeId
                select uos.ScenarioId)
                .Union(
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get()
                join l in _uow.GetRepository<SolutionParcels>().Get() on uos.LocationId equals l.ParcelId
                join o in _uow.GetRepository<Solution>().Get() on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Query() on uos.ScenarioId equals s.Id
                where o.ProjectId == projectId && l.IsSelected && uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.Parcel && s.ScenarioTypeId == scenTypeId
                select uos.ScenarioId)
                .Union(
                from uos in _uow.GetRepository<UnitOptimizationSolution>().Get()
                join l in _uow.GetRepository<SolutionModelComponents>().Get() on uos.LocationId equals l.ModelComponentId
                join o in _uow.GetRepository<Solution>().Get() on l.SolutionId equals o.Id
                join s in _uow.GetRepository<Scenario>().Query() on uos.ScenarioId equals s.Id
                where o.ProjectId == projectId && l.IsSelected && uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP && s.ScenarioTypeId == scenTypeId
                select uos.ScenarioId)
                .Distinct()
                .ToHashSet();
        }

        public List<BMPCostAreaDTO> GetBMPCostAreaDTOs(
            OptimizationSolutionLocationTypeEnum locationType,
            HashSet<int> locationIds,
            HashSet<int> reachBMPMcIds,
            HashSet<int> scenIds,
            SolutionDTO solution)
        {
            return
                (
                    from uos in _uow.GetRepository<UnitOptimizationSolution>().Query()
                    join bc in _uow.GetRepository<BMPCombinationType>().Query() on uos.BMPCombinationId equals bc.Id
                    where uos.OptimizationSolutionLocationTypeId == (int)locationType
                            && locationIds.Contains(uos.LocationId) && scenIds.Contains(uos.ScenarioId)
                    select new BMPCostAreaDTO
                    {
                        BMPCombinationTypeId = uos.BMPCombinationId,
                        BMPArea = uos.BMPArea,
                        BMPCombinationTypeName = bc.Name,
                        FarmId = uos.FarmId,
                        IsSelected = solution.UnitSolutions.Find(x => x.LocationId == uos.LocationId && x.LocationType == locationType) == null,
                        IsSelectable = !uos.IsExisting,
                        LocationId = uos.LocationId,
                        OptimizationSolutionLocationTypeId = uos.OptimizationSolutionLocationTypeId
                    }
                ).Union(
                    from uos in _uow.GetRepository<UnitOptimizationSolution>().Query()
                    join bc in _uow.GetRepository<BMPCombinationType>().Query() on uos.BMPCombinationId equals bc.Id
                    where uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                            && reachBMPMcIds.Contains(uos.LocationId) && scenIds.Contains(uos.ScenarioId)
                    select new BMPCostAreaDTO
                    {
                        BMPCombinationTypeId = uos.BMPCombinationId,
                        BMPArea = uos.BMPArea,
                        BMPCombinationTypeName = bc.Name,
                        FarmId = uos.FarmId,
                        IsSelected = solution.UnitSolutions.Find(x => x.LocationId == uos.LocationId && x.LocationType == OptimizationSolutionLocationTypeEnum.ReachBMP) == null,
                        IsSelectable = !uos.IsExisting,
                        LocationId = uos.LocationId,
                        OptimizationSolutionLocationTypeId = uos.OptimizationSolutionLocationTypeId
                    }
                ).ToList();
        }

        public List<ReachBMPGeometry> GetReachBMPByMCIds(HashSet<int> reachBMPMCIds, bool hasGeometry)
        {
            var modelComponentTypeIds = (from mcId in reachBMPMCIds
                                         join mc in _uow.GetRepository<ModelComponent>().Query() on mcId equals mc.Id
                                         select mc.ModelComponentTypeId).Distinct().ToHashSet();

            return new List<ReachBMPGeometry>().AsEnumerable()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.IsolatedWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<IsolatedWetland>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RiparianWetland>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Lake) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Lake>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.VegetativeFilterStrip) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<VegetativeFilterStrip>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianBuffer) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RiparianBuffer>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.GrassedWaterway) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<GrassedWaterway>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Length * bmp.Width / 10000 }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.FlowDiversion) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<FlowDiversion>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Reservoir) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Reservoir>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.SmallDam) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<SmallDam>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Wascob) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Wascob>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ClosedDrain) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<ClosedDrain>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Dugout) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Dugout>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.CatchBasin) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<CatchBasin>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Feedlot) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Feedlot>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ManureStorage) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<ManureStorage>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RockChute) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RockChute>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.PointSource) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<PointSource>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, ParcelId = sa.ParcelId, LegalSubDivisionId = sa.LegalSubDivisionId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .ToList();
        }
    }
}
