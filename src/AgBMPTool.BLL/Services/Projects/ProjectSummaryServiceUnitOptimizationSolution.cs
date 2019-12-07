using AgBMPTool.BLL.DLLException;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Solution;
using AgBMPTool.DBModel.Model.Type;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.DBL.Interface;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Services.Projects
{
    public class ProjectSummaryServiceUnitOptimizationSolution : IProjectSummaryService
    {

        public readonly IUnitOfWork _uow;
        public readonly IUtilitiesSummaryService _uss;

        public ProjectSummaryServiceUnitOptimizationSolution(IUnitOfWork _iUnitOfWork, IUtilitiesSummaryService _iUtilitiesSummaryService)
        {
            _uow = _iUnitOfWork;
            _uss = _iUtilitiesSummaryService;
        }

        #region Project summary
        public List<ProjectSummaryDTO> GetProjectSummaryDTOs(List<ProjectDTO> projectDTOs)
        {
            // Define results
            List<ProjectSummaryDTO> res = new List<ProjectSummaryDTO>();

            //Object lockMe = new object();

            // Foreach projectDTO
            foreach (var p in projectDTOs)
            {
                var BMPCostEffectiveness = this.GetSelectedBMPCostEffectivenessDTOForSelectionOverview(p.Id);

                ProjectSummaryDTO r = new ProjectSummaryDTO
                {
                    EffectivenessSummaryDTOs = this.GetEffectivenessSummaryDTOs(BMPCostEffectiveness),
                    BMPSummaryDTOs = this.GetBMPSummaryDTOs(BMPCostEffectiveness.Where(x => x.IsSelectable)?.ToList<BMPCostAllEffectivenessDTO>()
                                        ?? new List<BMPCostAllEffectivenessDTO>()),
                    Cost = BMPCostEffectiveness.Where(x => x.IsSelectable)?.Select(o => o.Cost).Sum()
                                        ?? 0,
                    Id = p.Id,
                    Name = p.Name,
                    CreatedDate = p.CreatedDate,
                    Description = p.Description,
                    Scope = p.Scope,
                    ScenarioTypeId = p.ScenarioTypeId,
                    SpatialUnitId = p.SpatialUnitId,
                    StartYear = p.StartYear,
                    EndYear = p.EndYear
                };

                res.Add(r);
            };


            // Return results
            return res;
        }

        public List<BMPSummaryDTO> GetProjectBMPSummaryDTOs(int projectId)
        {
            var BMPCostEffectiveness = this.GetSelectedBMPCostEffectivenessDTOForSelectionOverview(projectId);

            return this.GetBMPSummaryDTOs(BMPCostEffectiveness.Where(x => x.IsSelectable)?.ToList<BMPCostAllEffectivenessDTO>()
                                    ?? new List<BMPCostAllEffectivenessDTO>());
        }

        public List<EffectivenessSummaryDTO> GetProjectEffectivenessSummaryDTOs(int projectId)
        {
            var BMPCostEffectiveness = this.GetSelectedBMPCostEffectivenessDTOForSelectionOverview(projectId);

            return this.GetEffectivenessSummaryDTOs(BMPCostEffectiveness);
        }

        public List<BMPSummaryDTO> GetProjectBaselineBMPSummaryDTOs(int projectId)
        {
            int scenTypeId = _uow.GetRepository<Project>().Get(p => p.Id == projectId).FirstOrDefault()?.ScenarioTypeId ?? 0;

            if (scenTypeId != 2)
            {
                return new List<BMPSummaryDTO>();
            }

            var BMPCostEffectiveness = this.GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId);

            return this.GetBaselineBMPSummaryDTOs(BMPCostEffectiveness.Where(x => !x.IsSelectable)?.ToList<BMPCostAllEffectivenessDTO>()
                                            ?? new List<BMPCostAllEffectivenessDTO>());
        }

        public List<EffectivenessSummaryDTO> GetProjectBaselineEffectivenessSummaryDTOs(int projectId)
        {
            int scenTypeId = _uow.GetRepository<Project>().Get(p => p.Id == projectId).FirstOrDefault()?.ScenarioTypeId ?? 0;

            if (scenTypeId != 2)
            {
                return new List<EffectivenessSummaryDTO>();
            }

            var BMPCostEffectiveness = this.GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId);

            return this.GetEffectivenessSummaryDTOs(BMPCostEffectiveness.Where(x => !x.IsSelectable)?.ToList<BMPCostAllEffectivenessDTO>()
                                            ?? new List<BMPCostAllEffectivenessDTO>());
        }

        public decimal GetProjectCost(int projectId)
        {
            var BMPCostEffectiveness = this.GetSelectedBMPCostEffectivenessDTOForSelectionOverview(projectId);

            return BMPCostEffectiveness.Where(x => x.IsSelectable)?.Select(o => o.Cost).Sum()
                                        ?? 0;
        }
        #endregion

        #region BMP Scope and Intelligent Recommendation
        public List<BMPGeometryCostEffectivenessDTO> GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId)
        {
            var pInfo = (from p in _uow.GetRepository<Project>().Query()
                         where p.Id == projectId
                         select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            //var allBMPLocations = _uss.GetAllOptimizationBmpLocationsByProjectId(projectId);

            var selectedBMPLocations = _uss.GetSelectedOptimizationBmpLocationsByProjectId(projectId);

            // Include all and combined bmps
            int largestSingleBMPId = Int16.MaxValue;

            var locationType = pInfo.ProjectSpatialUnitTypeId == 1 ? OptimizationSolutionLocationTypeEnum.LegalSubDivision : OptimizationSolutionLocationTypeEnum.Parcel;
            //var locationIds = pInfo.ProjectSpatialUnitTypeId == 1 ? allBMPLocations.OptimizationLegalSubDivisions.Select(o => o.LocationId).ToHashSet() : allBMPLocations.OptimizationParcels.Select(o => o.LocationId).ToHashSet();
            //var reachBMPMcIds = allBMPLocations.OptimizationModelComponents.Select(o => o.LocationId).ToHashSet();
            var scenIds = _uss.GetAllScenarioIdsByProjectId(projectId, pInfo.ScenarioTypeId);

            var uosFields = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)locationType
                        //&& locationIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId <= largestSingleBMPId);
            var uosReachBMPs = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                        //&& reachBMPMcIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId <= largestSingleBMPId);

            var uosIdsField = uosFields.Select(o => o.Id).ToHashSet();
            var uosIdsReachBMP = uosReachBMPs.Select(o => o.Id).ToHashSet();

            var uosEffFields = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsField.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);
            var uosEffReachBMPs = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsReachBMP.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);

            return GetBMPGeometryCostEffectivenessDTOsCore(locationType, uosFields, uosReachBMPs, uosEffFields, uosEffReachBMPs, selectedBMPLocations);
        }

        public List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            return this.GetSingleBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId, bmpTypeId).ToList<BMPCostAllEffectivenessDTO>();
        }

        public List<BMPCostSingleEffectivenessDTO> GetSingleBMPCostSingleEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId, int smrvTypeId)
        {
            return (from b in GetSingleBMPCostAllEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId, bmpTypeId)
                    where b.EffectivenessDTOs.Select(x => x.ScenarioModelResultVariableTypeId).Contains(smrvTypeId)
                    select new BMPCostSingleEffectivenessDTO
                    {
                        LocationId = b.LocationId,
                        FarmId = b.FarmId,
                        BMPArea = b.BMPArea,
                        Cost = b.Cost,
                        BMPCombinationTypeId = b.BMPCombinationTypeId,
                        BMPCombinationTypeName = b.BMPCombinationTypeName,
                        IsSelectable = b.IsSelectable,
                        ScenarioModelResultVariableId = smrvTypeId,
                        OnsiteEffectiveness = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).EffectivenessValue,
                        OffsiteEffectiveness = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).EffectivenessValue,
                        OnsiteCostEffectivenessValue = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).CostEffectivenessValue,
                        OffsiteCostEffectivenessValue = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).CostEffectivenessValue,
                    }).ToList();
        }

        public List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            var pInfo = (from p in _uow.GetRepository<Project>().Query()
                         where p.Id == projectId
                         select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            //var allBMPLocations = _uss.GetAllOptimizationBmpLocationsByProjectId(projectId);

            var selectedBMPLocations = _uss.GetSelectedOptimizationBmpLocationsByProjectId(projectId);

            var locationType = pInfo.ProjectSpatialUnitTypeId == 1 ? OptimizationSolutionLocationTypeEnum.LegalSubDivision : OptimizationSolutionLocationTypeEnum.Parcel;
            //var locationIds = pInfo.ProjectSpatialUnitTypeId == 1 ? allBMPLocations.OptimizationLegalSubDivisions.Select(o => o.LocationId).ToHashSet() : allBMPLocations.OptimizationParcels.Select(o => o.LocationId).ToHashSet();
            //var reachBMPMcIds = allBMPLocations.OptimizationModelComponents.Select(o => o.LocationId).ToHashSet();
            var scenIds = _uss.GetAllScenarioIdsByProjectId(projectId, pInfo.ScenarioTypeId);

            var uosFields = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)locationType
                        //&& locationIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId == bmpTypeId);
            var uosReachBMPs = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                        //&& reachBMPMcIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId == bmpTypeId);

            var uosIdsField = uosFields.Select(o => o.Id).ToHashSet();
            var uosIdsReachBMP = uosReachBMPs.Select(o => o.Id).ToHashSet();

            var uosEffFields = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsField.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);
            var uosEffReachBMPs = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsReachBMP.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);

            return GetBMPGeometryCostEffectivenessDTOsCore(locationType, uosFields, uosReachBMPs, uosEffFields, uosEffReachBMPs, selectedBMPLocations);
        }

        public List<BMPTypeLocationsDTO> GetBMPTypeLocationsForBMPScopeAndIntelligentRecommendation(int projectId)
        {
            var _BMPGeometryCostEffectivenessDTOs = this.GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId);


            var res = new List<BMPTypeLocationsDTO>();

            foreach (var bmpTypeId in _BMPGeometryCostEffectivenessDTOs.Select(o => o.BMPCombinationTypeId).Distinct())
            {
                res.Add(new BMPTypeLocationsDTO
                {
                    BMPTypeId = bmpTypeId,
                    LocationIds = _BMPGeometryCostEffectivenessDTOs.Where(x => x.BMPCombinationTypeId == bmpTypeId).Select(o => o.LocationId)
                });
            }

            return res;
        }

        public List<Geometry> GetBMPGeometriesForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            return this.GetSingleBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId, bmpTypeId)?.Select(o => o.Geometry).ToList()
                   ?? new List<Geometry>();
        }
        #endregion

        #region BMP Selection and Overview
        public List<BMPGeometryCostEffectivenessDTO> GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(int projectId)
        {
            var pInfo = (from p in _uow.GetRepository<Project>().Get(p => p.Id == projectId)
                         select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            //var allBMPLocations = _uss.GetAllSolutionBmpLocationsByProjectId(projectId);

            var selectedBMPLocations = _uss.GetSelectedSolutionBmpLocationsByProjectId(projectId);

            // Remove all combined BMPs
            int largestSingleBMPId = _uow.GetRepository<BMPType>().Get().Select(o => o.Id).Max();

            var locationType = pInfo.ProjectSpatialUnitTypeId == 1 ? OptimizationSolutionLocationTypeEnum.LegalSubDivision : OptimizationSolutionLocationTypeEnum.Parcel;
            //var locationIds = pInfo.ProjectSpatialUnitTypeId == 1 ? allBMPLocations.SolutionLegalSubDivisions.Select(o => o.LocationId).ToHashSet() : allBMPLocations.SolutionParcels.Select(o => o.LocationId).ToHashSet();
            //var reachBMPMcIds = allBMPLocations.SolutionModelComponents.Select(o => o.LocationId).ToHashSet();
            var scenIds = _uss.GetAllScenarioIdsByProjectId(projectId, pInfo.ScenarioTypeId);

            var uosFields = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)locationType
                        //&& locationIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId <= largestSingleBMPId);
            var uosReachBMPs = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                        //&& reachBMPMcIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId <= largestSingleBMPId);

            var uosIdsField = uosFields.Select(o => o.Id).ToHashSet();
            var uosIdsReachBMP = uosReachBMPs.Select(o => o.Id).ToHashSet();

            var uosEffFields = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsField.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);
            var uosEffReachBMPs = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsReachBMP.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);

            return GetBMPGeometryCostEffectivenessDTOsCore(locationType, uosFields, uosReachBMPs, uosEffFields, uosEffReachBMPs, selectedBMPLocations);
        }

        public List<BMPTypeLocationsDTO> GetBMPTypeLocationsForBMPSelectionAndOverview(int projectId)
        {
            var _BMPGeometryCostEffectivenessDTOs = this.GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId);


            var res = new List<BMPTypeLocationsDTO>();

            foreach (var bmpTypeId in _BMPGeometryCostEffectivenessDTOs.Select(o => o.BMPCombinationTypeId).Distinct())
            {
                res.Add(new BMPTypeLocationsDTO
                {
                    BMPTypeId = bmpTypeId,
                    LocationIds = _BMPGeometryCostEffectivenessDTOs.Where(x => x.BMPCombinationTypeId == bmpTypeId).Select(o => o.LocationId)
                });
            }

            return res;
        }

        public List<Geometry> GetBMPGeometriesForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            return this.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId, bmpTypeId)?.Select(o => o.Geometry).ToList()
                ?? new List<Geometry>();
        }

        public List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            return this.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId, bmpTypeId).ToList<BMPCostAllEffectivenessDTO>();
        }

        public List<BMPCostSingleEffectivenessDTO> GetSingleBMPCostSingleEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId, int smrvTypeId)
        {
            return (from b in GetSingleBMPCostAllEffectivenessDTOForBMPSelectionAndOverview(projectId, bmpTypeId)
                    where b.EffectivenessDTOs.Select(x => x.ScenarioModelResultVariableTypeId).Contains(smrvTypeId)
                    select new BMPCostSingleEffectivenessDTO
                    {
                        LocationId = b.LocationId,
                        FarmId = b.FarmId,
                        BMPArea = b.BMPArea,
                        Cost = b.Cost,
                        BMPCombinationTypeId = b.BMPCombinationTypeId,
                        BMPCombinationTypeName = b.BMPCombinationTypeName,
                        IsSelectable = b.IsSelectable,
                        ScenarioModelResultVariableId = smrvTypeId,
                        OnsiteEffectiveness = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).EffectivenessValue,
                        OffsiteEffectiveness = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).EffectivenessValue,
                        OnsiteCostEffectivenessValue = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).CostEffectivenessValue,
                        OffsiteCostEffectivenessValue = b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) == null ? 0 : b.EffectivenessDTOs.Find(x => x.ScenarioModelResultVariableTypeId == smrvTypeId && x.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).CostEffectivenessValue,
                    }).ToList();
        }

        public List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            var pInfo = (from p in _uow.GetRepository<Project>().Query()
                         where p.Id == projectId
                         select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            //var allBMPLocations = _uss.GetAllSolutionBmpLocationsByProjectId(projectId);

            var selectedBMPLocations = _uss.GetSelectedSolutionBmpLocationsByProjectId(projectId);

            var locationType = pInfo.ProjectSpatialUnitTypeId == 1 ? OptimizationSolutionLocationTypeEnum.LegalSubDivision : OptimizationSolutionLocationTypeEnum.Parcel;
            //var locationIds = pInfo.ProjectSpatialUnitTypeId == 1 ? allBMPLocations.SolutionLegalSubDivisions.Select(o => o.LocationId).ToHashSet() : allBMPLocations.SolutionParcels.Select(o => o.LocationId).ToHashSet();
            //var reachBMPMcIds = allBMPLocations.SolutionModelComponents.Select(o => o.LocationId).ToHashSet();
            var scenIds = _uss.GetAllScenarioIdsByProjectId(projectId, pInfo.ScenarioTypeId);

            var uosFields = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)locationType
                        //&& locationIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId == bmpTypeId);
            var uosReachBMPs = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                        //&& reachBMPMcIds.Contains(uos.LocationId)
                        && scenIds.Contains(uos.ScenarioId)
                        && uos.BMPCombinationId == bmpTypeId);

            var uosIdsField = uosFields.Select(o => o.Id).ToHashSet();
            var uosIdsReachBMP = uosReachBMPs.Select(o => o.Id).ToHashSet();

            var uosEffFields = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsField.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);
            var uosEffReachBMPs = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsReachBMP.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);

            return GetBMPGeometryCostEffectivenessDTOsCore(locationType, uosFields, uosReachBMPs, uosEffFields, uosEffReachBMPs, selectedBMPLocations);
        }

        public List<LsdParcelBMPSummaryDTO> GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(int projectId)
        {
            // Check if projectId > 0
            if (projectId > 0)
            {
                // Get project spatial unit type id
                var project = _uow.GetRepository<Project>().Get(x => x.Id == projectId).FirstOrDefault();
                int projectSpatilUnitId = project.ProjectSpatialUnitTypeId;
                int scenarioTypeId = project.ScenarioTypeId;

                // Get selected BMPs information
                var selectedBMPs = GetSelectedBMPCostEffectivenessDTOForSelectionOverview(projectId);

                // Add existing BMPs for existing scenarios
                var baselineBMPs = new List<BMPGeometryCostEffectivenessDTO>();
                if (scenarioTypeId == (int)ScenarioTypeEnum.Existing)
                {
                    baselineBMPs = this.GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId).Where(x => !x.IsSelectable).ToList();
                }

                // Get selected and baseline LSD or Parcel id (locationId) BMP cost, area, farmid
                var locationBmpCostArea = selectedBMPs
                    .Where(x => x.OptimizationSolutionLocationTypeId != (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                    .Select(o => new { o.LocationId, o.FarmId, o.BMPCombinationTypeId, o.BMPCombinationTypeName, o.BMPArea, IsBaseline = !o.IsSelectable, o.Cost })
                    .ToList();

                var baselineLocationBmpCostArea = baselineBMPs
                    .Where(x => x.OptimizationSolutionLocationTypeId != (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                    .Select(o => new { o.LocationId, o.FarmId, o.BMPCombinationTypeId, o.BMPCombinationTypeName, o.BMPArea, IsBaseline = !o.IsSelectable, Cost = 0 })
                    .ToList();

                // Get selected and baseline reach BMP cost, area, isbaseline, farmid, and their located LSD or parcel id
                var reachBmpLsdParcelIds = _uss.GetReachBMPByMCIds(selectedBMPs
                    .Where(x => x.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                    .Select(o => o.LocationId).ToHashSet(), false).Select(o => new {
                        ReachBMPMcId = o.LocationId,
                        LocationId = projectSpatilUnitId == 1 ? o.LegalSubDivisionId : o.ParcelId
                    });

                var baselineReachBmpLsdParcelIds = _uss.GetReachBMPByMCIds(baselineBMPs
                    .Where(x => x.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                    .Select(o => o.LocationId).ToHashSet(), false).Select(o => new {
                        ReachBMPMcId = o.LocationId,
                        LocationId = projectSpatilUnitId == 1 ? o.LegalSubDivisionId : o.ParcelId
                    });

                var reachBmpCostArea =
                    (from rBMP in selectedBMPs.Where(x => x.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                     join rLocation in reachBmpLsdParcelIds on rBMP.LocationId equals rLocation.ReachBMPMcId
                     select new { ReachBMPMcId = rBMP.LocationId, rLocation.LocationId, rBMP.FarmId, rBMP.BMPCombinationTypeId, rBMP.BMPCombinationTypeName, rBMP.BMPArea, IsBaseline = !rBMP.IsSelectable, rBMP.Cost }
                    ).ToList();

                var baselineReachBmpCostArea =
                    (from rBMP in baselineBMPs.Where(x => x.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP)
                     join rLocation in baselineReachBmpLsdParcelIds on rBMP.LocationId equals rLocation.ReachBMPMcId
                     select new { ReachBMPMcId = rBMP.LocationId, rLocation.LocationId, rBMP.FarmId, rBMP.BMPCombinationTypeId, rBMP.BMPCombinationTypeName, rBMP.BMPArea, IsBaseline = !rBMP.IsSelectable, Cost = 0 }
                    ).ToList();

                // if both LSD/Parcel and reach BMPs are empty return empty list
                if (locationBmpCostArea.Count() + baselineReachBmpLsdParcelIds.Count()
                    + reachBmpCostArea.Count() + baselineReachBmpCostArea.Count() == 0)
                {
                    return new List<LsdParcelBMPSummaryDTO>();
                }

                var res = new List<LsdParcelBMPSummaryDTO>();

                var locationIds = locationBmpCostArea.Select(o => o.LocationId)
                    .Union(reachBmpCostArea.Select(o => o.LocationId))
                    .Union(baselineLocationBmpCostArea.Select(o => o.LocationId))
                    .Union(baselineReachBmpCostArea.Select(o => o.LocationId))
                    .Distinct().ToList();

                // Foreach LSD or parcel
                foreach (var locationId in locationIds)
                {
                    // Find reach BMPs in current LSD or parcel
                    var locationBmp = locationBmpCostArea.Find(x => x.LocationId == locationId);
                    var reachBmps = reachBmpCostArea.FindAll(x => x.LocationId == locationId).ToList();
                    var baselineLocationBmp = baselineLocationBmpCostArea.Find(x => x.LocationId == locationId);
                    var baselineReachBmps = baselineReachBmpCostArea.FindAll(x => x.LocationId == locationBmp.LocationId).ToList();

                    bool hasLsdParcel = locationBmp != null;
                    bool hasReachBmps = reachBmps != null && reachBmps.Count > 0;
                    bool hasBaseLsdParcel = baselineLocationBmp != null;
                    bool hasBaseReachBmps = baselineReachBmps != null && baselineReachBmps.Count > 0;

                    // Check if any BMP is found in this lsd/parcel id
                    if (!hasLsdParcel && !hasReachBmps && !hasBaseLsdParcel && !hasBaseReachBmps)
                    {
                        throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Can't find any BMP in this LSD/Parcel - {locationId}");
                    }

                    bool findFarm = false;

                    // Find farm Id
                    int farmId = 0;
                    if (!findFarm && hasLsdParcel)
                    {
                        farmId = locationBmp.FarmId;
                        findFarm = true;
                    }
                    if (!findFarm && hasReachBmps)
                    {
                        farmId = reachBmps.FirstOrDefault().FarmId;
                        findFarm = true;
                    }
                    if (!findFarm && hasBaseLsdParcel)
                    {
                        farmId = baselineLocationBmp.FarmId;
                        findFarm = true;
                    }
                    if (!findFarm && hasBaseReachBmps)
                    {
                        farmId = baselineReachBmps.FirstOrDefault().FarmId;
                        findFarm = true;
                    }

                    // Add to result list
                    res.Add(new LsdParcelBMPSummaryDTO
                    {
                        Cost = (hasLsdParcel ? locationBmp.Cost : 0) +
                                (hasReachBmps ? reachBmps.Select(o => o.Cost).Sum() : 0),
                        FarmId = farmId,
                        LsdOrParcelId = locationId,
                        LsdOrParcelBmp = (!hasLsdParcel && !hasBaseLsdParcel) ? "None" :
                                        (hasLsdParcel ? string.Join(", ", locationBmp.BMPCombinationTypeName.Split("_")) + $" ({locationBmp.BMPArea.ToString("N3")} ha)" + (hasBaseLsdParcel ? ", " : "") : "") +
                                        (hasBaseLsdParcel ? string.Join(", ", baselineLocationBmp.BMPCombinationTypeName.Split("_")) + $" ({baselineLocationBmp.BMPArea.ToString("N3")} ha, baseline)" : ""),
                        ProjectSpatialUnitTypeId = projectSpatilUnitId,
                        StructuralBmp = (!hasReachBmps && !hasBaseReachBmps) ? "None" :
                                        (hasReachBmps ? string.Join(", ", reachBmps.Select(o => $"{o.BMPCombinationTypeName} ({o.BMPArea.ToString("N3")} ha)").ToArray()) + (hasBaseReachBmps ? ", " : "") : "") +
                                        (hasBaseReachBmps ? string.Join(", ", baselineReachBmps.Select(o => $"{o.BMPCombinationTypeName} ({o.BMPArea.ToString("N3")} ha, baseline)").ToArray()) : ""),
                    });
                }

                // return list
                return res;
            }
            else // else return empty list
            {
                return new List<LsdParcelBMPSummaryDTO>();
            }
        }
        #endregion

        #region Local functions
        private List<BMPGeometryCostEffectivenessDTO> GetBMPGeometryCostEffectivenessDTOsCore(
            OptimizationSolutionLocationTypeEnum locationType,
            IEnumerable<UnitOptimizationSolution> uosFields,
            IEnumerable<UnitOptimizationSolution> uosReachBMPs,
            IEnumerable<UnitOptimizationSolutionEffectiveness> uosEffFields,
            IEnumerable<UnitOptimizationSolutionEffectiveness> uosEffReachBMPs,
            SolutionOptimizationBase solution)
        {

            var effs =
                (
                from uose in uosEffFields
                join uos in uosFields on uose.UnitOptimizationSolutionId equals uos.Id
                join bc in _uow.GetRepository<BMPCombinationType>().Get() on uos.BMPCombinationId equals bc.Id
                join bet in _uow.GetRepository<BMPEffectivenessType>().Get() on uose.BMPEffectivenessTypeId equals bet.Id
                join s in _uow.GetRepository<ScenarioModelResultType>().Get() on bet.ScenarioModelResultTypeId equals s.Id
                where uose.BMPEffectivenessTypeId != 22
                group uose.Value by new { uose.BMPEffectivenessTypeId, uose.UnitOptimizationSolutionId, s.UnitTypeId, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId } into g
                select new
                {
                    g.Key.BMPEffectivenessTypeId,
                    g.Key.UnitOptimizationSolutionId,
                    EffectivenessValue = g.Average(o => o),
                    CostEffectivenessConverter = g.Key.UnitTypeId == 13 ? 10 : 1,
                    g.Key.BMPEffectivenessLocationTypeId,
                    g.Key.ScenarioModelResultVariableTypeId
                }
                ).Union(
                from uose in uosEffReachBMPs
                join uos in uosReachBMPs on uose.UnitOptimizationSolutionId equals uos.Id
                join bc in _uow.GetRepository<BMPCombinationType>().Get() on uos.BMPCombinationId equals bc.Id
                join bet in _uow.GetRepository<BMPEffectivenessType>().Get() on uose.BMPEffectivenessTypeId equals bet.Id
                join s in _uow.GetRepository<ScenarioModelResultType>().Get() on bet.ScenarioModelResultTypeId equals s.Id
                where uose.BMPEffectivenessTypeId != 22
                group uose.Value by new { uose.BMPEffectivenessTypeId, uose.UnitOptimizationSolutionId, s.UnitTypeId, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId } into g
                select new
                {
                    g.Key.BMPEffectivenessTypeId,
                    g.Key.UnitOptimizationSolutionId,
                    EffectivenessValue = g.Average(o => o),
                    CostEffectivenessConverter = g.Key.UnitTypeId == 13 ? 10 : 1,
                    g.Key.BMPEffectivenessLocationTypeId,
                    g.Key.ScenarioModelResultVariableTypeId
                }
                ).ToList();

            var geometryCosts =
                (
                    from uose in uosEffFields
                    join uos in uosFields on uose.UnitOptimizationSolutionId equals uos.Id
                    join bc in _uow.GetRepository<BMPCombinationType>().Get() on uos.BMPCombinationId equals bc.Id
                    where uose.BMPEffectivenessTypeId == 22
                    group uose.Value by new { uos.BMPCombinationId, uos.BMPArea, bc.Name, uos.FarmId, uos.Geometry, uos.LocationId, uos.IsExisting, uos.OptimizationSolutionLocationTypeId, UnitOptimizationSolutionId = uos.Id, locationType } into g
                    select new
                    {
                        g.Key.UnitOptimizationSolutionId,
                        BMPCombinationTypeId = g.Key.BMPCombinationId,
                        BMPArea = g.Key.BMPArea,
                        Cost = g.Average(x => x),
                        BMPCombinationTypeName = g.Key.Name,
                        FarmId = g.Key.FarmId,
                        Geometry = g.Key.Geometry,
                        IsSelectable = !g.Key.IsExisting,
                        LocationId = g.Key.LocationId,
                        OptimizationSolutionLocationTypeId = g.Key.OptimizationSolutionLocationTypeId,
                        g.Key.locationType
                    }
                ).Union(
                    from uose in uosEffReachBMPs
                    join uos in uosReachBMPs on uose.UnitOptimizationSolutionId equals uos.Id
                    join bc in _uow.GetRepository<BMPCombinationType>().Get() on uos.BMPCombinationId equals bc.Id
                    where uose.BMPEffectivenessTypeId == 22
                    group uose.Value by new { uos.BMPCombinationId, uos.BMPArea, bc.Name, uos.FarmId, uos.Geometry, uos.LocationId, uos.IsExisting, uos.OptimizationSolutionLocationTypeId, UnitOptimizationSolutionId = uos.Id, locationType = OptimizationSolutionLocationTypeEnum.ReachBMP } into g
                    select new
                    {
                        g.Key.UnitOptimizationSolutionId,
                        BMPCombinationTypeId = g.Key.BMPCombinationId,
                        BMPArea = g.Key.BMPArea,
                        Cost = g.Average(x => x),
                        BMPCombinationTypeName = g.Key.Name,
                        FarmId = g.Key.FarmId,
                        Geometry = g.Key.Geometry,
                        IsSelectable = !g.Key.IsExisting,
                        LocationId = g.Key.LocationId,
                        OptimizationSolutionLocationTypeId = g.Key.OptimizationSolutionLocationTypeId,
                        g.Key.locationType
                    }
                ).ToList();

            var res = new List<BMPGeometryCostEffectivenessDTO>();

            foreach (var gc in geometryCosts)
            {
                res.Add(new BMPGeometryCostEffectivenessDTO
                {
                    BMPCombinationTypeId = gc.BMPCombinationTypeId,
                    BMPArea = gc.BMPArea,
                    Cost = gc.Cost,
                    BMPCombinationTypeName = gc.BMPCombinationTypeName,
                    FarmId = gc.FarmId,
                    Geometry = gc.Geometry,
                    IsSelectable = gc.IsSelectable,
                    LocationId = gc.LocationId,
                    OptimizationSolutionLocationTypeId = gc.OptimizationSolutionLocationTypeId,
                    IsSelected = (solution.UnitSolutions.Where(x => x.BMPTypeId == gc.BMPCombinationTypeId && x.LocationId == gc.LocationId && x.LocationType == gc.locationType).Count() > 0),
                    EffectivenessDTOs = effs.Where(x => x.UnitOptimizationSolutionId == gc.UnitOptimizationSolutionId).Select(o => new EffectivenessDTO
                    {
                        BMPEffectivenessTypeId = o.BMPEffectivenessTypeId,
                        EffectivenessValue = o.EffectivenessValue,
                        CostEffectivenessValue = o.EffectivenessValue == 0 ? 0 : gc.Cost / o.EffectivenessValue / o.CostEffectivenessConverter,
                        BMPEffectivenessLocationTypeId = o.BMPEffectivenessLocationTypeId,
                        ScenarioModelResultVariableTypeId = (int)o.ScenarioModelResultVariableTypeId
                    }).ToList()
                });
            }

            return res;
        }

        private List<BMPCostAllEffectivenessDTO> GetSelectedBMPCostEffectivenessDTOForSelectionOverview(int projectId)
        {
            var pInfo = (from p in _uow.GetRepository<Project>().Get(p => p.Id == projectId)
                         select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            var selectedBMPLocations = _uss.GetSelectedSolutionBmpLocationsByProjectId(projectId);

            var locationType = pInfo.ProjectSpatialUnitTypeId == 1 ? OptimizationSolutionLocationTypeEnum.LegalSubDivision : OptimizationSolutionLocationTypeEnum.Parcel;
            var locations = pInfo.ProjectSpatialUnitTypeId == 1 ? selectedBMPLocations.SolutionLegalSubDivisions : selectedBMPLocations.SolutionParcels;
            var reachBMPMcIds = selectedBMPLocations.SolutionModelComponents.Select(o => o.LocationId).ToHashSet();
            var scenIds = _uss.GetAllScenarioIdsByProjectId(projectId, pInfo.ScenarioTypeId);

            HashSet<string> locationBMPCombinationId = new HashSet<string>();
            HashSet<string> reachBMPMcIdBMPCombinationId = selectedBMPLocations.SolutionModelComponents.Select(o => $"{o.LocationId}_{o.BMPTypeId}").ToHashSet();

            foreach (var locationId in locations.Select(o => o.LocationId).Distinct())
            {
                locationBMPCombinationId.Add($"{locationId}_{this.GetBMPCombinationIdByBmpTypes(locations.Where(x => x.LocationId == locationId).Select(o => o.BMPTypeId).Distinct().ToList())}");
            }

            var uosFields = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)locationType
                        && locationBMPCombinationId.Contains($"{uos.LocationId}_{uos.BMPCombinationId}")
                        && scenIds.Contains(uos.ScenarioId)
                        && !uos.IsExisting).AsParallel().ToList();
            var uosReachBMPs = _uow.GetRepository<UnitOptimizationSolution>().Get(uos =>
                        uos.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP
                        && reachBMPMcIdBMPCombinationId.Contains($"{uos.LocationId}_{uos.BMPCombinationId}")
                        && scenIds.Contains(uos.ScenarioId)
                        && !uos.IsExisting).AsParallel().ToList();

            var uosIdsField = uosFields.Select(o => o.Id).Distinct().ToHashSet();
            var uosIdsReachBMP = uosReachBMPs.Select(o => o.Id).Distinct().ToHashSet();

            var uosEffFields = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsField.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);
            var uosEffReachBMPs = _uow.GetRepository<UnitOptimizationSolutionEffectiveness>().Get(uose =>
                        uosIdsReachBMP.Contains(uose.UnitOptimizationSolutionId) && uose.Year >= pInfo.StartYear && uose.Year <= pInfo.EndYear);

            return GetBMPGeometryCostEffectivenessDTOsCore(locationType, uosFields, uosReachBMPs, uosEffFields, uosEffReachBMPs, selectedBMPLocations).ToList<BMPCostAllEffectivenessDTO>();
        }

        private List<BMPCombinationBMPTypes> _BMPCombinationBMPTypes;

        private List<BMPCombinationBMPTypes> BMPCombinationBMPTypes
        {
            get
            {
                if (_BMPCombinationBMPTypes == null)
                {
                    _BMPCombinationBMPTypes = _uow.GetRepository<BMPCombinationBMPTypes>().Get().ToList();
                }
                return _BMPCombinationBMPTypes;
            }
        }

        private int GetBMPCombinationIdByBmpTypes(List<int> bmpTypes)
        {
            var bmpComobinatonBMPTypes = BMPCombinationBMPTypes.Where(x => bmpTypes.Contains(x.BMPTypeId)).ToHashSet();

            return bmpComobinatonBMPTypes
                            .Where(b => bmpTypes.Contains(b.BMPTypeId))
                            .GroupBy(b => b.BMPCombinationTypeId)
                            .Where(grp => grp.Count() == bmpTypes.Count())
                            .Select(grp => grp.Key)
                            .Intersect(bmpComobinatonBMPTypes
                            .GroupBy(b => b.BMPCombinationTypeId)
                            .Where(grp => grp.Count() == bmpTypes.Count())
                            .Select(grp => grp.Key)).FirstOrDefault();
        }

        private readonly HashSet<int> m3ToMMBMPEffeTypeIds = new HashSet<int> { 1, 2, 3, 4 };
        private readonly HashSet<int> areaWeightedBMPEffeTypeIds = new HashSet<int> { 13 };
        private readonly HashSet<int> m3ToM3perSecBMPEffeTypeIds = new HashSet<int> { 14 };
        private readonly int SECONDS_PER_DAY = 86400;
        private readonly int SECONDS_PER_YEAR = 31557600; // 86400 * 365.25

        private List<EffectivenessSummaryDTO> GetEffectivenessSummaryDTOs(List<BMPCostAllEffectivenessDTO> BMPCostEffectiveness)
        {
            return BMPCostEffectiveness == null ? new List<EffectivenessSummaryDTO>() :
                   (from bce in BMPCostEffectiveness.SelectMany(o => o.EffectivenessDTOs.Select(oo => new { oo.BMPEffectivenessTypeId, oo.EffectivenessValue, o.BMPArea })) // not including biodiversity
                    join bet in _uow.GetRepository<BMPEffectivenessType>().Get() on bce.BMPEffectivenessTypeId equals bet.Id
                    join smrvt in _uow.GetRepository<ScenarioModelResultType>().Get() on bet.ScenarioModelResultTypeId equals smrvt.Id
                    join u in _uow.GetRepository<UnitType>().Get() on smrvt.UnitTypeId equals u.Id
                    orderby bet.Id
                    group new { bce.BMPArea, bce.EffectivenessValue } by new
                    {
                        u.UnitSymbol,
                        bet
                    } into g
                    select new EffectivenessSummaryDTO
                    {
                        UnitSymbol = g.Key.UnitSymbol,
                        BMPEffectivenessType = g.Key.bet,
                        ValueChange = Math.Round(
                            m3ToMMBMPEffeTypeIds.Contains(g.Key.bet.Id) ? g.Sum(o => o.EffectivenessValue) / g.Sum(o => o.BMPArea) * 10
                            :
                            areaWeightedBMPEffeTypeIds.Contains(g.Key.bet.Id) ? g.Sum(o => o.EffectivenessValue * o.BMPArea) / g.Sum(o => o.BMPArea)
                            :
                            m3ToM3perSecBMPEffeTypeIds.Contains(g.Key.bet.Id) ? g.Sum(o => o.EffectivenessValue) / SECONDS_PER_YEAR
                            :
                            g.Sum(o => o.EffectivenessValue), 4)
                    }).ToList();
        }

        private List<BMPSummaryDTO> GetBMPSummaryDTOs(List<BMPCostAllEffectivenessDTO> BMPCostEffectiveness)
        {
            return BMPCostEffectiveness == null ? new List<BMPSummaryDTO>() :
                (from bce in BMPCostEffectiveness
                 join b in _uow.GetRepository<BMPCombinationType>().Get() on bce.BMPCombinationTypeId equals b.Id
                 group new { bce.BMPArea, bce.Cost } by b into g
                 select new BMPSummaryDTO { BMPTypeId = g.Key.Id, BMPTypeName = g.Key.Name, ModelComponentCount = g.Count(), TotalArea = Math.Round(g.Sum(o => o.BMPArea), 3), TotalCost = Math.Round(g.Sum(o => o.Cost), 2) }).ToList();
        }

        private List<BMPSummaryDTO> GetBaselineBMPSummaryDTOs(List<BMPCostAllEffectivenessDTO> BMPCostEffectiveness)
        {
            return BMPCostEffectiveness == null ? new List<BMPSummaryDTO>() :
                (from bce in BMPCostEffectiveness
                 join b in _uow.GetRepository<BMPCombinationType>().Get() on bce.BMPCombinationTypeId equals b.Id
                 group new { bce.BMPArea, bce.Cost } by b into g
                 select new BMPSummaryDTO { BMPTypeId = g.Key.Id, BMPTypeName = g.Key.Name, ModelComponentCount = g.Count(), TotalArea = Math.Round(g.Sum(o => o.BMPArea), 3), TotalCost = 0 }).ToList();
        }
        #endregion

    }
}
