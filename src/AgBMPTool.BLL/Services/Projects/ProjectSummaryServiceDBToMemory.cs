using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AgBMPTool.BLL.DLLException;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Services.Utilities;
using AgBMPTool.DBModel;
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
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Services.Projects
{
    public class ProjectSummaryServiceDBToMemory : IProjectSummaryService
    {
        public IDBToMemory _dtm { get; set; }

        public readonly IUnitOfWork _uow;
        public readonly IUtilitiesSummaryService _uss;

        public ProjectSummaryServiceDBToMemory() { }

        public ProjectSummaryServiceDBToMemory(IUnitOfWork _iUnitOfWork, IUtilitiesSummaryService _iUtilitiesSummaryService)
        {
            _uow = _iUnitOfWork;
            _uss = _iUtilitiesSummaryService;
        }

        private EffectivenessSummaryDTO GetProjectEffectivenessSummaryByEffectivenessType(int projectId, int bmpEffectivenessTypeId)
        {
            // Get GetModelComponentBMPCombinationTypes By ProjectId
            var m = GetModelComponentBMPCombinationTypesByProjectId(projectId);

            // If no GetModelComponentBMPCombinationTypes found
            if (m.Count == 0)
            {
                // Return empty list
                return null;
            }
            else // Else 
            {
                // Get project
                var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();

                return this.GetEffectivenessSummaryByScenTypeIdModelComponentBmpCombination(project.ScenarioTypeId, m, bmpEffectivenessTypeId, project.StartYear, project.EndYear).FirstOrDefault();
            }
        }

        private Dictionary<int, EffectivenessSummaryDTO> GetEffectivenessSummaryByProjectList(List<int> projectIds, int bmpEffectivenessTypeId)
        {
            var res = new Dictionary<int, EffectivenessSummaryDTO>();

            foreach (int projectId in projectIds)
            {
                res[projectId] = this.GetProjectEffectivenessSummaryByEffectivenessType(projectId, bmpEffectivenessTypeId);
            }

            return res;
        }

        private List<EffectivenessSummaryDTO> GetProjectEffectivenessSummaries(int projectId)
        {
            // Get GetModelComponentBMPCombinationTypes By ProjectId
            var m = GetModelComponentBMPCombinationTypesByProjectId(projectId);

            // If no GetModelComponentBMPCombinationTypes found
            if (m.Count == 0)
            {
                // Return empty list
                return new List<EffectivenessSummaryDTO>();
            }
            else // Else 
            {
                // Get project
                var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();

                return this.GetEffectivenessSummaryByScenTypeIdModelComponentBmpCombination(project.ScenarioTypeId, m, -999, project.StartYear, project.EndYear);
            }
        }

        private List<BMPSummaryDTO> GetProjectBMPSummaries(int projectId)
        {
            // Get GetModelComponentBMPCombinationTypes By ProjectId
            var m = GetModelComponentBMPCombinationTypesByProjectId(projectId);

            // If no GetModelComponentBMPCombinationTypes found
            if (m.Count == 0)
            {
                // Return empty list
                return new List<BMPSummaryDTO>();
            }
            else // Else 
            {
                // Get project
                var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();

                return this.GetBMPSummaryByModelComponentBmpCombination(project.ScenarioTypeId, project.ProjectSpatialUnitTypeId, m, project.StartYear, project.EndYear);
            }
        }

        private decimal GetProjectCost(int projectId)
        {
            // Get GetModelComponentBMPCombinationTypes By ProjectId
            var m = GetModelComponentBMPCombinationTypesByProjectId(projectId);

            // If no GetModelComponentBMPCombinationTypes found
            if (m.Count == 0)
            {
                // Return empty list
                return (decimal)0.0;
            }
            else // Else 
            {
                // Get project
                var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();

                return this.GetCostByModelComponentBmpCombination(project.ScenarioTypeId, m, project.StartYear, project.EndYear);
            }
        }

        private List<decimal> GetCostByProjectList(List<int> projectIds)
        {
            var res = new List<decimal>();

            foreach (int projectId in projectIds)
            {
                res.Add(this.GetProjectCost(projectId));
            }

            return res;
        }

        private List<EffectivenessSummaryDTO> GetProjectBaselineEffectiveness(int projectId)
        {
            // Get project
            var project = _dtm.Projects.Where(x => x.Id == projectId).Select(x => x).FirstOrDefault();

            // If is conventional baseline
            if (project.ScenarioTypeId == 1)
            {
                // Return empty list
                return new List<EffectivenessSummaryDTO>();
            }
            else // Else
            {
                // Get GetBaselineModelComponentBMPCombinationTypes By ProjectId
                var m = GetBaselineModelComponentBMPCombinationTypesByProjectId(project.Id);

                // If no GetModelComponentBMPCombinationTypes found
                if (m.Count == 0)
                {
                    // Return empty list
                    return new List<EffectivenessSummaryDTO>();
                }

                // Use conventional ProjectScenarioType = 1 to get baseline summary
                return this.GetEffectivenessSummaryByScenTypeIdModelComponentBmpCombination(1, m, -999, project.StartYear, project.EndYear);
            } // Endif
        }

        private List<BMPSummaryDTO> GetProjectBaselineBMPSummaries(int projectId)
        {
            // Get project
            var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();

            // If is conventional baseline
            if (project.ScenarioTypeId == 1)
            {
                // Return empty list
                return new List<BMPSummaryDTO>();
            }
            else // Else
            {
                // Get GetBaselineModelComponentBMPCombinationTypes By ProjectId
                var m = GetBaselineModelComponentBMPCombinationTypesByProjectId(project.Id);

                // If no GetModelComponentBMPCombinationTypes found
                if (m.Count == 0)
                {
                    // Return empty list
                    return new List<BMPSummaryDTO>();
                }

                // Use conventional ProjectScenarioType = 1 to get baseline summary
                return this.GetBMPSummaryByModelComponentBmpCombination(1, project.ProjectSpatialUnitTypeId, m, project.StartYear, project.EndYear);
            } // Endif
        }

        private List<EffectivenessSummaryDTO> GetEffectivenessSummaryByScenTypeIdModelComponentBmpCombination(int scenarioTypeId, Dictionary<int, int> modelComponentBmpCombinationType, int bmpEffectivenessTypeId, int startYear, int endYear)
        {
            // check year
            if (startYear >= endYear)
            {
                throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"startYear CAN'T be larger than endYear!");
            }

            // Define local aggregate summary
            var aggregateLocalSummary = new Dictionary<BMPEffectivenessType, LocalEffectSummary>();

            foreach (var mcBmpComEntry in modelComponentBmpCombinationType)
            {
                // Get MC
                var mc = _dtm.ModelComponents.Where(x => x.Id == mcBmpComEntry.Key).FirstOrDefault();

                // Get watershedId
                int watershedId = mc.WatershedId;

                // Get scenarioId
                int scenarioId = _dtm.Scenarios.Where(x => x.WatershedId == watershedId && x.ScenarioTypeId == scenarioTypeId).FirstOrDefault()?.Id ?? 0;

                // Get UnitScenarioId
                var unitScenarioId = _dtm.UnitScenarios.Where(x => x.ModelComponentId == mcBmpComEntry.Key && x.ScenarioId == scenarioId && x.BMPCombinationId == mcBmpComEntry.Value).FirstOrDefault()?.Id;

                // Get list of UnitScenarioEffectiveness
                var unitScenEffects = _dtm.UnitScenarioEffectivenesses.Where(x => x.UnitScenarioId == unitScenarioId && x.Year >= startYear && x.Year <= endYear).ToList();


                // Foreach UnitScenarioEffectiveness
                foreach (var unitScenEffect in unitScenEffects)
                {
                    // Get BMPEffectivenessType
                    var bmpEffectivenessType = _dtm.BMPEffectivenessTypes.Where(x => x.Id == unitScenEffect.BMPEffectivenessTypeId).FirstOrDefault();

                    // If bmpEffectivenessTypeId provided, only summarize this type
                    if (bmpEffectivenessTypeId > 0 && bmpEffectivenessTypeId != bmpEffectivenessType.Id)
                    {
                        continue;
                    }

                    // Not summarizing BMP cost, it is summarized in GetBmpCostByBmpTypeModelComponentScenarioType method
                    if (bmpEffectivenessType.Id == 22)
                    {
                        continue;
                    }

                    // Get percentage change
                    decimal percChange = unitScenEffect.Value;

                    // Get ScenarioModelResult value
                    decimal smrValue = this.GetScenarioModelResultValue(scenarioId, mcBmpComEntry.Value, mc, bmpEffectivenessType, unitScenEffect.Year);

                    // Get value change = baseline value * percentage chage
                    decimal valueChange = smrValue * percChange / 100;

                    // Add to res
                    if (aggregateLocalSummary.ContainsKey(bmpEffectivenessType))
                    {
                        aggregateLocalSummary[bmpEffectivenessType].Value += smrValue;
                        aggregateLocalSummary[bmpEffectivenessType].ValueChange += valueChange;
                        aggregateLocalSummary[bmpEffectivenessType].PercentChange += percChange;
                    }
                    else
                    {
                        aggregateLocalSummary.Add(bmpEffectivenessType,
                            new LocalEffectSummary
                            {
                                Value = smrValue,
                                ValueChange = valueChange,
                                PercentChange = percChange
                            });
                    }
                }
            }

            if (aggregateLocalSummary.Count == 0)
            {
                return new List<EffectivenessSummaryDTO>
                {
                    new EffectivenessSummaryDTO
                    {
                        BMPEffectivenessType = _dtm.BMPEffectivenessTypes.Where(x => x.Id == bmpEffectivenessTypeId).FirstOrDefault()
                    }
                };
            }

            var res = new List<EffectivenessSummaryDTO>();

            // Calculate yearly average results for each BMPEffectivenessType
            foreach (var a in aggregateLocalSummary)
            {
                // Get unit symbol
                string unitSymbol = _dtm.UnitTypes.Where(x => x.Id == a.Key.UnitTypeId).Select(o => o.UnitSymbol).FirstOrDefault();

                // Onsite
                if (a.Key.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite)
                {
                    res.Add(
                        new EffectivenessSummaryDTO
                        {
                            BMPEffectivenessType = a.Key,
                            UnitSymbol = unitSymbol,
                            PercentChange = a.Value.Value == 0 ? 0 : a.Value.ValueChange / a.Value.Value * 100,
                            ValueChange = a.Value.ValueChange / (endYear - startYear + 1)
                        });
                }
                else // Offsite
                {
                    res.Add(
                        new EffectivenessSummaryDTO
                        {
                            BMPEffectivenessType = a.Key,
                            UnitSymbol = unitSymbol,
                            PercentChange = a.Value.PercentChange / (endYear - startYear + 1),
                            ValueChange = a.Value.ValueChange / (endYear - startYear + 1)
                        });
                }
            }

            return res;
        }

        private List<BMPSummaryDTO> GetBMPSummaryByModelComponentBmpCombination(int scenarioTypeId, int projectSpatialUnitTypeId, Dictionary<int, int> modelComponentBMPCombinationType, int startYear, int endYear)
        {
            var res = new List<BMPSummaryDTO>();

            // Foreach through BMPType-List<MCId>
            foreach (var bmpTypeMcIdEntry in this.GetBMPTypeModelComponentListByModelComponentBMPCombination(modelComponentBMPCombinationType))
            {
                // Get BMPType
                var bmpType = _dtm.BMPTypes.Where(x => x.Id == bmpTypeMcIdEntry.Key).FirstOrDefault();

                // Get BMP area based on BMPType, MCIds, and scenarioTypeId
                var area = this.GetBmpAreaByBmpTypeModelComponentScenarioType(bmpType.Id, bmpTypeMcIdEntry.Value);

                // Get BMP cost based on BMPType, MCIds, and scenarioTypeId
                var cost = this.GetBmpCostByBmpTypeModelComponentScenarioType(bmpType.Id, bmpTypeMcIdEntry.Value, scenarioTypeId, startYear, endYear);

                // Define count
                var cnt = 0;

                // Get 

                // If BMP type model component type is structure
                if (_dtm.ModelComponentTypes.Where(x => x.Id == bmpType.ModelComponentTypeId).FirstOrDefault()?.IsStructure ?? false)
                {
                    // BMP count = count of MCId
                    cnt = bmpTypeMcIdEntry.Value.Count;
                }
                else // Else If BMP type model component type is subarea
                {
                    if (projectSpatialUnitTypeId == 1) // If projectSpatialUnitTypeId = 1 LSD
                    {
                        // BMP count = all LSD count in List<MCId>
                        cnt = _dtm.SubAreas.Where(x => bmpTypeMcIdEntry.Value.Contains(x.ModelComponentId)).Select(l => new { Id = l.LegalSubDivisionId }).Distinct().Count();
                    }
                    else if (projectSpatialUnitTypeId == 2) // Else if projectSpatialUnitTypeId = 2 Parcel
                    {
                        // BMP count = all parcel count in List<MCId>
                        cnt = _dtm.SubAreas.Where(x => bmpTypeMcIdEntry.Value.Contains(x.ModelComponentId)).Select(p => new { Id = p.ParcelId }).Distinct().Count();
                    }
                    // End if
                }// End if

                // Add to res new BMPSummaryDTO
                res.Add(new BMPSummaryDTO { BMPType = bmpType, ModelComponentCount = cnt, TotalArea = area, TotalCost = cost });
            }
            // End Foreach
            return res;
        }

        private decimal GetCostByModelComponentBmpCombination(int scenarioTypeId, Dictionary<int, int> modelComponentBMPCombinationType, int startYear, int endYear)
        {
            decimal res = (decimal)0.0;

            // Foreach through BMPType-List<MCId>
            foreach (var bmpTypeMcIdEntry in this.GetBMPTypeModelComponentListByModelComponentBMPCombination(modelComponentBMPCombinationType))
            {
                // Get BMPType
                var bmpTypeId = _dtm.BMPTypes.Where(x => x.Id == bmpTypeMcIdEntry.Key).FirstOrDefault()?.Id ?? 0;

                // Get BMP cost based on BMPType, MCIds, and scenarioTypeId
                res += this.GetBmpCostByBmpTypeModelComponentScenarioType(bmpTypeId, bmpTypeMcIdEntry.Value, scenarioTypeId, startYear, endYear);
            }
            // End Foreach

            return res;
        }

        private Dictionary<int, List<int>> GetBMPTypeModelComponentListByModelComponentBMPCombination(Dictionary<int, int> modelComponentBMPCombinationType)
        {
            // Define BMPType-List<MCId>
            var res = new Dictionary<int, List<int>>();

            // Foreach MC-BMPCombionationTypes
            foreach (var mcBmpComEntry in modelComponentBMPCombinationType)
            {
                // Get all BMPTyps by BMPCombinationId
                var bmpTypes = _dtm.BMPCombinationBMPTypes.Where(x => x.BMPCombinationTypeId == mcBmpComEntry.Value).ToList();

                // Foreach BMPTypes
                foreach (var bmpCombBmpType in bmpTypes)
                {
                    // Add BMPType, mcId to BMPType-List<MCId>
                    if (!res.ContainsKey(bmpCombBmpType.BMPTypeId))
                    {
                        res[bmpCombBmpType.BMPTypeId] = new List<int>();
                    }
                    res[bmpCombBmpType.BMPTypeId].Add(mcBmpComEntry.Key);
                }
                // End foreach
            }
            // End foreach

            return res;
        }

        private Dictionary<int, int> GetModelComponentBMPCombinationTypesByProjectId(int projectId)
        {
            var res = new Dictionary<int, int>();

            // Get project
            var project = _dtm.Projects.Where(x => x.Id == projectId).FirstOrDefault();


            // Define a MC-<list>bmpType
            var mcBmpTypes = new Dictionary<int, List<int>>();

            // if project is using LSD
            if (project.ProjectSpatialUnitTypeId == 1) // LSD = 1
            {

                // Loop through all solutionLSDs
                foreach (var solutionLsd in _dtm.SolutionLegalSubDivisions.Where(x => x.SolutionId == projectId).ToList())
                {
                    // Get all SubAreas in one LSD
                    var subAreas = _dtm.SubAreas.Where(x => x.LegalSubDivisionId == solutionLsd.LegalSubDivisionId).ToList();

                    // Foreach subarea
                    foreach (var subArea in subAreas)
                    {
                        // Add subarea MCId and BMPTypeId to MC-<list>bmpType
                        if (!mcBmpTypes.ContainsKey(subArea.ModelComponentId))
                        {
                            mcBmpTypes[subArea.ModelComponentId] = new List<int>();
                        }
                        mcBmpTypes[subArea.ModelComponentId].Add(solutionLsd.BMPTypeId);
                    }
                }
            }
            else if (project.ProjectSpatialUnitTypeId == 2) // Parcel = 2
            {

                // Loop through all solutionParcels
                foreach (var solutionParcel in _dtm.SolutionParcels.Where(x => x.SolutionId == projectId).ToList())
                {
                    // Get all SubAreas in one parcel
                    var subAreas = _dtm.SubAreas.Where(x => x.ParcelId == solutionParcel.ParcelId).ToList();

                    // Foreach subarea
                    foreach (var subArea in subAreas)
                    {
                        // Add subarea MCId and BMPTypeId to MC-<list>bmpType
                        if (!mcBmpTypes.ContainsKey(subArea.ModelComponentId))
                        {
                            mcBmpTypes[subArea.ModelComponentId] = new List<int>();
                        }
                        mcBmpTypes[subArea.ModelComponentId].Add(solutionParcel.BMPTypeId);
                    }
                }

            }

            //Loop through all solutionComponentStructures
            foreach (var solutionMC in _dtm.SolutionModelComponents.Where(x => x.SolutionId == projectId).ToList())
            {
                // Add reach MCId and BMPTypeId to MC-<list>bmpType
                if (!mcBmpTypes.ContainsKey(solutionMC.ModelComponentId))
                {
                    mcBmpTypes[solutionMC.ModelComponentId] = new List<int>();
                }
                mcBmpTypes[solutionMC.ModelComponentId].Add(solutionMC.BMPTypeId);
            }

            // return ModelComponentBMPCombinationTypes
            return this.GetModelComponentBMPCombinationTypesByMCBMPList(project.ScenarioTypeId, mcBmpTypes);
        }

        private List<LocationGeometry> GetSpatialUnitBySubAreaMCIds(HashSet<int> subAreaMCIds, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD)
            {
                return (from sa in _uow.GetRepository<SubArea>().Get(sa => subAreaMCIds.Contains(sa.ModelComponentId))
                        join lsd in _uow.GetRepository<LegalSubDivision>().Query() on sa.LegalSubDivisionId equals lsd.Id
                        join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                        join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                        group sa.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                        select new LocationGeometry { LocationId = g.Key.lsdId, FarmId = g.Key.farmID, Geometry = g.Key.Geometry, Area = g.Sum(o => o) }).Distinct().ToList();

            }
            else
            {
                return (from sa in _uow.GetRepository<SubArea>().Get(sa => subAreaMCIds.Contains(sa.ModelComponentId))
                        join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                        join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                        group sa.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                        select new LocationGeometry { LocationId = g.Key.parcelId, FarmId = g.Key.farmID, Geometry = g.Key.Geometry, Area = g.Sum(o => o) }).Distinct().ToList();
            }
        }

        private List<LocationGeometry> GetSpatialUnitBySubAreaMCIdsDbContext(AgBMPToolContext db, HashSet<int> subAreaMCIds, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD)
            {
                return (from sa in db.SubAreas.Where(sa => subAreaMCIds.Contains(sa.ModelComponentId))
                        join lsd in db.LegalSubDivisions on sa.LegalSubDivisionId equals lsd.Id
                        join p in db.Parcels on sa.ParcelId equals p.Id
                        join f in db.Farms on p.OwnerId equals f.OwnerId
                        group sa.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                        select new LocationGeometry { LocationId = g.Key.lsdId, FarmId = g.Key.farmID, Geometry = g.Key.Geometry, Area = g.Sum(o => o) }).Distinct().ToList();

            }
            else
            {
                return (from sa in db.SubAreas.Where(sa => subAreaMCIds.Contains(sa.ModelComponentId))
                        join p in db.Parcels on sa.ParcelId equals p.Id
                        join f in db.Farms on p.OwnerId equals f.OwnerId
                        group sa.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                        select new LocationGeometry { LocationId = g.Key.parcelId, FarmId = g.Key.farmID, Geometry = g.Key.Geometry, Area = g.Sum(o => o) }).Distinct().ToList();
            }
        }

        private Dictionary<int, decimal> GetSpatialUnitBySubAreaMCIdsVfstRibufGww(int bmpTypeId, HashSet<int> subAreaMCIds, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD)
            {
                switch (bmpTypeId)
                {
                    case 4:
                        return (
                    from bmp in _uow.GetRepository<VegetativeFilterStrip>().Query()
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join lsd in _uow.GetRepository<LegalSubDivision>().Query() on sa.LegalSubDivisionId equals lsd.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                    where subAreaMCIds.Contains(sa.ModelComponentId)
                    group bmp.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                    select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);

                    case 6:
                        return (from bmp in _uow.GetRepository<GrassedWaterway>().Query()
                                join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                                join lsd in _uow.GetRepository<LegalSubDivision>().Query() on sa.LegalSubDivisionId equals lsd.Id
                                join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                                join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group new { bmp.Length, bmp.Width } by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                                select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o.Length * o.Width / 10000) }).ToDictionary(t => t.LocationId, t => t.Area);
                    case 5:
                        return (
                                from bmp in _uow.GetRepository<RiparianBuffer>().Query()
                                join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                                join lsd in _uow.GetRepository<LegalSubDivision>().Query() on sa.LegalSubDivisionId equals lsd.Id
                                join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                                join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group bmp.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                                select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);
                }
            }
            else
            {
                switch (bmpTypeId)
                {
                    case 4:
                        return (
                    from bmp in _uow.GetRepository<VegetativeFilterStrip>().Query()
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                    where subAreaMCIds.Contains(sa.ModelComponentId)
                    group bmp.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                    select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);

                    case 6:
                        return (from bmp in _uow.GetRepository<GrassedWaterway>().Query()
                                join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                                join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                                join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group new { bmp.Length, bmp.Width } by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                                select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o.Length * o.Width / 10000) }).ToDictionary(t => t.LocationId, t => t.Area);
                    case 5:
                        return (
                            from bmp in _uow.GetRepository<RiparianBuffer>().Query()
                            join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                            join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                            join f in _uow.GetRepository<Farm>().Query() on p.OwnerId equals f.OwnerId
                            where subAreaMCIds.Contains(sa.ModelComponentId)
                            group bmp.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                            select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);
                }
            }
            return new Dictionary<int, decimal>();
        }

        private Dictionary<int, decimal> GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(AgBMPToolContext db, int bmpTypeId, HashSet<int> subAreaMCIds, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD)
            {
                switch (bmpTypeId)
                {
                    case 4:
                        return (
                    from bmp in db.VegetativeFilterStrips
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join lsd in db.LegalSubDivisions on sa.LegalSubDivisionId equals lsd.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join f in db.Farms on p.OwnerId equals f.OwnerId
                    where subAreaMCIds.Contains(sa.ModelComponentId)
                    group bmp.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                    select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);

                    case 6:
                        return (from bmp in db.GrassedWaterways
                                join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                                join lsd in db.LegalSubDivisions on sa.LegalSubDivisionId equals lsd.Id
                                join p in db.Parcels on sa.ParcelId equals p.Id
                                join f in db.Farms on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group new { bmp.Length, bmp.Width } by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                                select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o.Length * o.Width / 10000) }).ToDictionary(t => t.LocationId, t => t.Area);
                    case 5:
                        return (
                                from bmp in db.RiparianBuffers
                                join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                                join lsd in db.LegalSubDivisions on sa.LegalSubDivisionId equals lsd.Id
                                join p in db.Parcels on sa.ParcelId equals p.Id
                                join f in db.Farms on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group bmp.Area by new { lsdId = lsd.Id, farmID = f.Id, lsd.Geometry } into g
                                select new { LocationId = g.Key.lsdId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);
                }
            }
            else
            {
                switch (bmpTypeId)
                {
                    case 4:
                        return (
                    from bmp in db.VegetativeFilterStrips
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join f in db.Farms on p.OwnerId equals f.OwnerId
                    where subAreaMCIds.Contains(sa.ModelComponentId)
                    group bmp.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                    select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);

                    case 6:
                        return (from bmp in db.GrassedWaterways
                                join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                                join p in db.Parcels on sa.ParcelId equals p.Id
                                join f in db.Farms on p.OwnerId equals f.OwnerId
                                where subAreaMCIds.Contains(sa.ModelComponentId)
                                group new { bmp.Length, bmp.Width } by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                                select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o.Length * o.Width / 10000) }).ToDictionary(t => t.LocationId, t => t.Area);
                    case 5:
                        return (
                            from bmp in db.RiparianBuffers
                            join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                            join p in db.Parcels on sa.ParcelId equals p.Id
                            join f in db.Farms on p.OwnerId equals f.OwnerId
                            where subAreaMCIds.Contains(sa.ModelComponentId)
                            group bmp.Area by new { parcelId = p.Id, farmID = f.Id, p.Geometry } into g
                            select new { LocationId = g.Key.parcelId, Area = g.Sum(o => o) }).ToDictionary(t => t.LocationId, t => t.Area);
                }
            }
            return new Dictionary<int, decimal>();
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
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RiparianWetland>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Lake) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Lake>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.VegetativeFilterStrip) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<VegetativeFilterStrip>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianBuffer) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RiparianBuffer>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.GrassedWaterway) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<GrassedWaterway>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Length * bmp.Width / 10000 }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.FlowDiversion) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<FlowDiversion>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Reservoir) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Reservoir>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.SmallDam) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<SmallDam>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Wascob) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Wascob>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ClosedDrain) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<ClosedDrain>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Dugout) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Dugout>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.CatchBasin) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<CatchBasin>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Feedlot) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<Feedlot>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ManureStorage) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<ManureStorage>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RockChute) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<RockChute>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.PointSource) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in _uow.GetRepository<PointSource>().Query()
                    join r in _uow.GetRepository<Reach>().Query() on bmp.ReachId equals r.Id
                    join sa in _uow.GetRepository<SubArea>().Query() on bmp.SubAreaId equals sa.Id
                    join p in _uow.GetRepository<Parcel>().Query() on sa.ParcelId equals p.Id
                    join farm in _uow.GetRepository<Farm>().Query() on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .ToList();
        }

        public List<ReachBMPGeometry> GetReachBMPByMCIdsDbContext(AgBMPToolContext db, HashSet<int> reachBMPMCIds, bool hasGeometry)
        {
            var modelComponentTypeIds = (from mcId in reachBMPMCIds
                                         join mc in db.ModelComponents on mcId equals mc.Id
                                         select mc.ModelComponentTypeId).Distinct().ToHashSet();

            return new List<ReachBMPGeometry>().AsEnumerable()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.IsolatedWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.IsolatedWetlands
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RiparianWetlands
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Lake) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Lakes
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.VegetativeFilterStrip) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.VegetativeFilterStrips
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianBuffer) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RiparianBuffers
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.GrassedWaterway) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.GrassedWaterways
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Length * bmp.Width / 10000 }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.FlowDiversion) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.FlowDiversions
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Reservoir) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Reservoirs
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.SmallDam) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.SmallDams
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Wascob) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Wascobs
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ClosedDrain) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.ClosedDrains
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Dugout) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Dugouts
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.CatchBasin) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.CatchBasins
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Feedlot) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Feedlots
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ManureStorage) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.ManureStorages
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RockChute) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RockChutes
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.PointSource) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.PointSources
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, Area = 0.01m }).Distinct()
             .ToList();
        }

        public class LocationGeometry
        {
            public int LocationId { get; set; }
            public decimal Area { get; set; }
            public int FarmId { get; set; }
            public Geometry Geometry { get; set; }
        }

        public class ReachBMPGeometry : LocationGeometry
        {
            public int ReachMCId { get; set; }
        }

        private int GetBMPCombinationIdByBmpTypes(List<int> bmpTypes)
        {
            var bmpComobinatonBMPTypes = _dtm.BMPCombinationBMPTypes.Where(x => bmpTypes.Contains(x.BMPTypeId)).ToHashSet();

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

        private Dictionary<int, int> GetModelComponentBMPCombinationTypesByMCBMPList(int scenarioTypeId, Dictionary<int, List<int>> mcBmpTypes)
        {
            var res = new Dictionary<int, int>();

            //var r = new ESAT.Import.Utils.EsatDbRetriever();

            // Loop through all MC-<list>bmpType
            foreach (var mcBmpType in mcBmpTypes)
            {
                // Get watershedId
                int watershedId = _dtm.ModelComponents.Where(x => x.Id == mcBmpType.Key).FirstOrDefault()?.WatershedId ?? 0;

                // Get scenarioId
                int scenarioId = _dtm.Scenarios.Where(x => x.WatershedId == watershedId && x.ScenarioTypeId == scenarioTypeId).FirstOrDefault()?.Id ?? 0;

                // Get BMPCombinationId from <list>BMPType
                int bmpCombionationId = this.GetBMPCombinationIdByBmpTypes(mcBmpType.Value); // r.GetBmpCombinationId(mcBmpType.Value);

                // Get UnitScenario using MCId, ScenarioId, and BMPCombinationId
                int unitScenarioCnt = _dtm.UnitScenarios.Where(x => x.ModelComponentId == mcBmpType.Key && x.ScenarioId == scenarioId && x.BMPCombinationId == bmpCombionationId).Count();

                // If UnitScenario != null
                if (unitScenarioCnt > 0)
                {
                    // Add MCId-BMPCombinationId to res
                    res[mcBmpType.Key] = bmpCombionationId;
                }
            }

            return res;
        }

        private Dictionary<int, int> GetBaselineModelComponentBMPCombinationTypesByProjectId(int projectId)
        {

            // Get projectWatershedIds
            var projectWatershedIds = _dtm.ProjectWatersheds.Where(x => x.ProjectId == projectId).Select(o => o.WatershedId).ToList();

            // Define a MC-<list>bmpType
            var mcBmpTypes = new Dictionary<int, List<int>>();

            int largestBMPTypeId = _dtm.BMPTypes.Select(o => o.Id).Max();

            // Foreach projectWatershedId
            foreach (int wId in projectWatershedIds)
            {
                // Get convScenarioId
                int convScenarioId = _dtm.Scenarios.Where(x => x.WatershedId == wId && x.ScenarioTypeId == 1).Select(o => o.Id).FirstOrDefault();

                // Get exisScenarioId
                int exisScenarioId = _dtm.Scenarios.Where(x => x.WatershedId == wId && x.ScenarioTypeId == 2).Select(o => o.Id).FirstOrDefault();

                // Get exisCompositeKey (ModelComponentId_BMPCombinationId) list of exisScenarioId
                var exisCompositeKey = _dtm.UnitScenarios.Where(x => x.ScenarioId == exisScenarioId).Select(o => $"{o.ModelComponentId}_{o.BMPCombinationId}").ToHashSet();

                // Get ModelComponentId, BMPCombinationId of convScenarioId by exisCompositeKey NOT containing convCompositeKey (ModelComponentId_BMPCombinationId)
                var mcIdBmpComIds = _dtm.UnitScenarios.Where(x => !exisCompositeKey.Contains($"{x.ModelComponentId}_{x.BMPCombinationId}")).Select(o => new { o.ModelComponentId, o.BMPCombinationId }).ToList();

                // Add ModelComponentId, BMPCombinationId to res
                foreach (var mb in mcIdBmpComIds)
                {
                    // Only get single BMP combination Id
                    if (mb.BMPCombinationId > 0 && mb.BMPCombinationId <= largestBMPTypeId)
                    {
                        // Add subarea MCId and BMPTypeId to MC-<list>bmpType
                        if (!mcBmpTypes.ContainsKey(mb.ModelComponentId))
                        {
                            mcBmpTypes[mb.ModelComponentId] = new List<int>();
                        }

                        mcBmpTypes[mb.ModelComponentId].Add(mb.BMPCombinationId);
                    }
                }
            }// End foreach

            // return ModelComponentBMPCombinationTypes use conventional scenario type id = 1
            return this.GetModelComponentBMPCombinationTypesByMCBMPList(1, mcBmpTypes);
        }

        private decimal GetBmpAreaByBmpTypeModelComponentScenarioType(int bmpTypeId, List<int> modelComponentIds)
        {
            decimal res = (decimal)0.01; // Assume 100m2 for point or line BMP

            List<int> subAreaIds = new List<int>();
            if (bmpTypeId == 4 || bmpTypeId == 5 || bmpTypeId == 6)
            {
                subAreaIds = _dtm.SubAreas.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Id).ToList();
            }


            switch (bmpTypeId)
            {
                case 1:
                    res = _dtm.IsolatedWetlands.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 2:
                    res = _dtm.RiparianWetlands.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 3:
                    res = _dtm.Lakes.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 4:
                    res = _dtm.VegetativeFilterStrips.Where(x => subAreaIds.Contains(x.SubAreaId)).Select(o => o.Area).Sum();
                    break;
                case 5:
                    res = _dtm.RiparianBuffers.Where(x => subAreaIds.Contains(x.SubAreaId)).Select(o => o.Area).Sum();
                    break;
                case 6:
                    res = _dtm.GrassedWaterways.Where(x => subAreaIds.Contains(x.SubAreaId)).Select(o => (o.Width * o.Length)).Sum() / 10000;
                    break;
                case 8:
                    res = _dtm.Reservoirs.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 9:
                    res = _dtm.SmallDams.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 10:
                    res = _dtm.Wascobs.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 12:
                    res = _dtm.Dugouts.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 13:
                    res = _dtm.CatchBasins.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 14:
                    res = _dtm.Feedlots.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 15:
                    res = _dtm.ManureStorages.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
                case 7: // FlowDiversion no area
                case 11: // ClosedDrain no area
                case 16:// RockChute no area
                case 17:// PointSource no area
                    break;
                default: // Others use the full subarea area
                    res = _dtm.SubAreas.Where(x => modelComponentIds.Contains(x.ModelComponentId)).Select(o => o.Area).Sum();
                    break;
            }

            return res; // to hectare
        }

        private int GetReachIdByBmpTypeModelComponentScenarioType(int bmpCombinationTypeId, int modelComponentId)
        {
            switch (bmpCombinationTypeId)
            {
                case 1:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.IsolatedWetlands.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 2:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.RiparianWetlands.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 3:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.Lakes.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 4:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.VegetativeFilterStrips.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 5:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.RiparianBuffers.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 6:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.GrassedWaterways.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 8:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.Reservoirs.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 9:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.SmallDams.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 10:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.Wascobs.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 12:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.Dugouts.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 13:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.CatchBasins.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 14:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.Feedlots.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 15:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.ManureStorages.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 7:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.FlowDiversions.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 11:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.ClosedDrains.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 16:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.RockChutes.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                case 17:
                    return _dtm.Reaches.Where(x => x.Id ==
                                (_dtm.PointSources.Where(xx => xx.ModelComponentId == modelComponentId)
                                .FirstOrDefault()?.ReachId ?? 0)).FirstOrDefault()?.ModelComponentId ?? 0;
                default: // Others use the subarea id
                    return modelComponentId;
            }
        }

        private decimal GetBmpCostByBmpTypeModelComponentScenarioType(int bmpTypeId, List<int> modelComponentIds, int scenarioTypeId, int startYear, int endYear)
        {
            decimal res = (decimal)0.0;

            // Foreach mcId
            foreach (var mcId in modelComponentIds)
            {
                // Get watershedId
                int watershedId = _dtm.ModelComponents.Where(x => x.Id == mcId).FirstOrDefault()?.WatershedId ?? 0;

                // Get scenarioId
                int scenarioId = _dtm.Scenarios.Where(x => x.WatershedId == watershedId && x.ScenarioTypeId == scenarioTypeId).FirstOrDefault()?.Id ?? 0;

                // Get unitScenarioId
                var unitScenarioId = _dtm.UnitScenarios.Where(x => x.BMPCombinationId == bmpTypeId && x.ScenarioId == scenarioId && x.ModelComponentId == mcId).FirstOrDefault()?.Id;

                // Get cost from UnitScenarioEffectiveness by bmpTypeId (single BMP type), mcId, scenarioId and BMPEffectivenessTypeId = 22
                var uses = _dtm.UnitScenarioEffectivenesses.Where(x => x.UnitScenarioId == unitScenarioId && x.Year >= startYear && x.Year <= endYear && x.BMPEffectivenessTypeId == 22).ToList();


                // Add cost to res
                foreach (var use in uses)
                {
                    res += use.Value;
                }

            } // End foreach

            return res / (endYear - startYear + 1); // return yearly average result
        }

        private decimal GetScenarioModelResultValue(int scenarioId, int bmpCombinationTypeId, ModelComponent modelComponent, BMPEffectivenessType bmpEffectivenessType, int year)
        {
            int mcId;
            int mcTypeId = 2; // initial as reach type
            #region *** Get mcId and mcTypeId ***
            // If on-site BMPEffectivessLocationTypeId = 1
            if (bmpEffectivenessType.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) // on-site
            {
                if (modelComponent.ModelComponentTypeId == 1)// If field BMP : ModelComponentTypeId = 1
                {
                    // mcId = current modelCompent Id
                    mcId = modelComponent.Id;

                    // mcTypeId = current modelCompentTypeId
                    mcTypeId = modelComponent.ModelComponentTypeId;
                }
                else // Else : reach BMP
                {
                    // mcId = BMP reach Id
                    mcId = this.GetReachIdByBmpTypeModelComponentScenarioType(bmpCombinationTypeId, modelComponent.Id);
                }// Endif
            }
            else // Else off-site
            {
                // mcId = watershed outlet reach Id
                mcId = _dtm.Watersheds.Where(x => x.Id == modelComponent.WatershedId).FirstOrDefault()?.OutletReachId ?? 0;
            }// Endif
            #endregion

            // Get smrTypeId by mcTypeId and smrVariableTypeId, 
            var smrType = _dtm.ScenarioModelResultTypes.Where(x => x.ModelComponentTypeId == mcTypeId && x.ScenarioModelResultVariableTypeId == bmpEffectivenessType.ScenarioModelResultVariableTypeId).FirstOrDefault();

            // If not SMR type is found return 0
            if (smrType == null) return 0;

            // Return SMR value by scenarioId, mcId, year, and smrTypeId
            return _dtm.ScenarioModelResults.Where(x => x.ScenarioId == scenarioId && x.Year == year && x.ModelComponentId == mcId && x.ScenarioModelResultTypeId == smrType.Id).FirstOrDefault()?.Value ?? 0;

        }

        public List<BMPGeometryCostEffectivenessDTO> GetBMPGeometryCostEffectivenessDTOInternal(
            int ProjectSpatialUnitTypeId,
            int ScenarioTypeId,
            HashSet<int> currScenIds,
            HashSet<int> modelComponentIds,
            int StartYear,
            int EndYear,
            int largestSingleBMPId)
        {
            // Get available modelcomponents 
            var uss = _uow.GetRepository<UnitScenario>().Get(u => modelComponentIds.Contains(u.ModelComponentId) && currScenIds.Contains(u.ScenarioId) && u.BMPCombinationId <= largestSingleBMPId).ToList();
            var ussIds = uss.Select(o => o.Id).ToHashSet();
            var availables = (from u in uss.AsParallel()
                              join mc in _uow.GetRepository<ModelComponent>().Get(mc => modelComponentIds.Contains(mc.Id)).AsParallel() on u.ModelComponentId equals mc.Id
                              join w in _uow.GetRepository<Watershed>().Get().AsParallel() on mc.WatershedId equals w.Id
                              join r in _uow.GetRepository<Reach>().Get().AsParallel() on w.OutletReachId equals r.Id
                              join ue in _uow.GetRepository<UnitScenarioEffectiveness>().Get(ue => ussIds.Contains(ue.UnitScenarioId) && ue.Year >= StartYear && ue.Year <= EndYear).AsParallel() on u.Id equals ue.UnitScenarioId
                              join bet in _uow.GetRepository<BMPEffectivenessType>().Get().AsParallel() on ue.BMPEffectivenessTypeId equals bet.Id
                              join bc in _uow.GetRepository<BMPCombinationType>().Get().AsParallel() on u.BMPCombinationId equals bc.Id
                              join mct in _uow.GetRepository<ModelComponentType>().Get().AsParallel() on bc.ModelComponentTypeId equals mct.Id
                              select new { u.ScenarioId, BMPTypeId = bc.Id, BMPTypeName = bc.Name, mct.IsStructure, u.ModelComponentId, OutletReachMCId = r.ModelComponentId, ue.Year, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId, BMPEffectivenessTypeId = bet.Id, PercentChange = ue.Value }).ToList();

            // Get existing modelcomponents
            var webts = _uow.GetRepository<WatershedExistingBMPType>().Get(webt => modelComponentIds.Contains(webt.ModelComponentId) && webt.ScenarioTypeId == ScenarioTypeId).ToList();
            var webtIds = webts.Select(o => o.Id).ToHashSet();
            var existings = (from webt in webts.AsParallel()
                             join mc in _uow.GetRepository<ModelComponent>().Get(mc => modelComponentIds.Contains(mc.Id)).AsParallel() on webt.ModelComponentId equals mc.Id
                             join w in _uow.GetRepository<Watershed>().Get().AsParallel() on mc.WatershedId equals w.Id
                             join s in _uow.GetRepository<Scenario>().Get().AsParallel() on $"{w.Id}_{webt.ScenarioTypeId}" equals $"{s.WatershedId}_{s.ScenarioTypeId}"
                             join r in _uow.GetRepository<Reach>().Get().AsParallel() on w.OutletReachId equals r.Id
                             join ue in _uow.GetRepository<UnitScenarioEffectiveness>().Get(ue => webtIds.Contains(ue.UnitScenarioId) && ue.Year >= StartYear && ue.Year <= EndYear).AsParallel() on webt.Id equals ue.UnitScenarioId
                             join bet in _uow.GetRepository<BMPEffectivenessType>().Get().AsParallel() on ue.BMPEffectivenessTypeId equals bet.Id
                             join bc in _uow.GetRepository<BMPType>().Get().AsParallel() on webt.BMPTypeId equals bc.Id
                             join mct in _uow.GetRepository<ModelComponentType>().Get().AsParallel() on bc.ModelComponentTypeId equals mct.Id
                             select new { ScenarioId = s.Id, BMPTypeId = bc.Id, BMPTypeName = bc.Name, mct.IsStructure, webt.ModelComponentId, OutletReachMCId = r.ModelComponentId, ue.Year, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId, BMPEffectivenessTypeId = bet.Id, PercentChange = ue.Value }).ToList();

            var bmpIdNames = availables.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure }).Union(existings.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure })).Distinct().ToList();

            var allSubAreaMCIdsWithBMP = availables.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)
                                       .Union(existings.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)).Distinct().ToHashSet();

            var subAreaSpatialUnits = (from sa in _uow.GetRepository<SubArea>().Get(sa => allSubAreaMCIdsWithBMP.Contains(sa.ModelComponentId)).AsParallel()
                                       select new
                                       {
                                           SubAreaMcId = sa.ModelComponentId,
                                           SpatialUnitId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? sa.LegalSubDivisionId : sa.ParcelId
                                       }).ToList();

            List<BMPGeometryCostEffectivenessDTO> res = new List<BMPGeometryCostEffectivenessDTO>();

            // Get reach BMP ModelComponent id and geometry
            var reachBMPAvailable = this.GetReachBMPByMCIds(availables.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true);
            var reachBMPExisting = this.GetReachBMPByMCIds(existings.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true);

            // Get modelcomponent ids including outlet reach MC id
            var availableReachMcIds = reachBMPAvailable.Select(o => o.ReachMCId).Union(availables.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var existingReachMcIds = reachBMPExisting.Select(o => o.ReachMCId).Union(existings.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var availableSubAreaMcIds = availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(availables.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var existingSubAreaMcIds = existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(existings.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();

            // Append Scenario model result value and value change
            var availablesReachSMRValue = (from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).AsParallel() // onsite use bmp reach modelcomponent id
                                           join r in reachBMPAvailable.AsParallel() on a.ModelComponentId equals r.LocationId
                                           join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                           join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{r.ReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                           select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).AsParallel() // offsite use outlet reach modelcomponent id
                                      join r in reachBMPAvailable.AsParallel() on a.ModelComponentId equals r.LocationId
                                      join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{a.OutletReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).ToList();

            // Append Scenario model result value and value change
            var existingsReachSMRValue = (from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).AsParallel() // onsite use bmp reach modelcomponent id
                                          join r in reachBMPExisting.AsParallel() on e.ModelComponentId equals r.LocationId
                                          join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                          join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{r.ReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                          select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).AsParallel() // offsite use outlet reach modelcomponent id
                                      join r in reachBMPExisting.AsParallel() on e.ModelComponentId equals r.LocationId
                                      join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{e.OutletReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).ToList();

            // Get LSD/Parcel id and geometry
            var spatialUnitAvailable = this.GetSpatialUnitBySubAreaMCIds(availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            var spatialUnitExisting = this.GetSpatialUnitBySubAreaMCIds(existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            var suAvailableVfst =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(4,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 4).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suAvailableRibuf =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(5,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 5).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suAvailableGww =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(6,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 6).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );

            var suExistingVfst =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(4,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 4).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suExistingRibuf =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(5,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 5).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suExistingGww =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGww(6,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 6).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );

            // Append Scenario model result value and value change
            var availablesSubAreaSMRValue = (from a in availables.Where(a => !a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).AsParallel() // onsite use subarea modelcomponent id
                                             join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{a.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                             join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{a.ModelComponentId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                             select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (from a in availables.Where(a => !a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).AsParallel() // offsite use outlet reach modelcomponent id
                                      join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{a.OutletReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).ToList();

            // Append Scenario model result value and value change
            var existingsSubAreaSMRValue = (from e in existings.Where(e => !e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite).AsParallel() // onsite use subarea modelcomponent id
                                            join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{e.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                            join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{e.ModelComponentId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                            select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (from e in existings.Where(e => !e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite).AsParallel() // offsite use outlet reach modelcomponent id
                                      join smrt in _uow.GetRepository<ScenarioModelResultType>().Get().AsParallel() on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in _uow.GetRepository<ScenarioModelResult>().Get(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)).AsParallel() on $"{e.OutletReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).ToList();

            // Aggregate to LSD/parcel level results
            var spatialUnitAvailableResults = (
                from a in availablesSubAreaSMRValue.Select(o => o).AsParallel()
                join sa2su in subAreaSpatialUnits.AsParallel() on a.ModelComponentId equals sa2su.SubAreaMcId
                group a.ValueChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }).ToList();

            var spatialUnitExistingResults = (
                from a in existingsSubAreaSMRValue.AsParallel()
                join sa2su in subAreaSpatialUnits.AsParallel() on a.ModelComponentId equals sa2su.SubAreaMcId
                group a.ValueChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }).ToList();

            // Aggregate cost to LSD/parcel level results
            var spatialUnitAvailableCost = (
                from a in availables.Select(o => o).AsParallel()
                join sa2su in subAreaSpatialUnits.AsParallel() on a.ModelComponentId equals sa2su.SubAreaMcId
                where a.BMPEffectivenessTypeId == 22 // cost
                group a.PercentChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessTypeId, Cost = g.Sum(x => x) }).ToList();

            var spatialUnitExistingCost = (
                from e in existings.Select(o => o).AsParallel()
                join sa2su in subAreaSpatialUnits.AsParallel() on e.ModelComponentId equals sa2su.SubAreaMcId
                where e.BMPEffectivenessTypeId == 22 // cost
                group e.PercentChange by new { sa2su.SpatialUnitId, e.Year, e.BMPEffectivenessTypeId, e.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessTypeId, Cost = g.Sum(x => x) }).ToList();


            Object lockMe = new object();

            Parallel.ForEach(bmpIdNames, b =>
            {
                if (b.IsStructure)
                {
                    var results = availablesReachSMRValue.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    var costs = availables.Where(r => r.BMPTypeId == b.BMPTypeId && r.BMPEffectivenessTypeId == 22);
                    var rbs = reachBMPAvailable.Where(x => results.Select(o => o.ModelComponentId).ToHashSet().Contains(x.LocationId));

                    foreach (var rb in rbs)
                    {
                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = rb.LocationId,
                            FarmId = rb.FarmId,
                            Geometry = rb.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = rb.Area,
                            OptimizationSolutionLocationTypeId = (int)OptimizationSolutionLocationTypeEnum.ReachBMP,
                            EffectivenessDTOs =
                                (from r in results
                                 where r.ModelComponentId == rb.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from r in costs
                                 where r.ModelComponentId == rb.LocationId // cost
                                 group r.PercentChange by r.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = true
                        };

                        lock (lockMe)
                        {
                            res.Add(dto);
                        }
                    }

                    costs = existings.Where(r => r.BMPTypeId == b.BMPTypeId && r.BMPEffectivenessTypeId == 22);
                    results = existingsReachSMRValue.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    rbs = reachBMPAvailable.Where(x => results.Select(o => o.ModelComponentId).ToHashSet().Contains(x.LocationId));
                    foreach (var rb in rbs)
                    {
                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = rb.LocationId,
                            FarmId = rb.FarmId,
                            Geometry = rb.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = rb.Area,
                            OptimizationSolutionLocationTypeId = (int)OptimizationSolutionLocationTypeEnum.ReachBMP,
                            EffectivenessDTOs =
                                (from r in existingsReachSMRValue
                                 where r.ModelComponentId == rb.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from r in costs
                                 where r.ModelComponentId == rb.LocationId // cost
                                 group r.PercentChange by r.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = false
                        };

                        lock (lockMe)
                        {
                            res.Add(dto);
                        }
                    }

                }
                else
                {
                    var results = spatialUnitExistingResults.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    var costs = spatialUnitExistingCost.Where(c => c.BMPTypeId == b.BMPTypeId && c.BMPEffectivenessTypeId == 22);
                    var sus = spatialUnitExisting.Where(x => results.Select(o => o.LocationId).ToHashSet().Contains(x.LocationId));

                    HashSet<int> existingSU = new HashSet<int>();

                    foreach (var sue in sus)
                    {
                        decimal area;
                        switch (b.BMPTypeId)
                        {
                            case 4:
                                area = suExistingVfst[sue.LocationId] ;
                                break;
                            case 5:
                                area = suExistingRibuf[sue.LocationId];
                                break;
                            case 6:
                                area = suExistingGww[sue.LocationId];
                                break;
                            default:
                                area = sue.Area;
                                break;
                        }

                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = sue.LocationId,
                            FarmId = sue.FarmId,
                            Geometry = sue.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = area,
                            OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision : (int)OptimizationSolutionLocationTypeEnum.Parcel,
                            EffectivenessDTOs =
                                (from r in results
                                 where r.LocationId == sue.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from c in costs
                                 where c.LocationId == sue.LocationId // cost
                                 group c.Cost by c.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = false
                        };

                        lock (lockMe)
                        {
                            res.Add(dto);
                            existingSU.Add(sue.LocationId);
                        }
                    }

                    results = spatialUnitAvailableResults.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    costs = spatialUnitAvailableCost.Where(c => c.BMPTypeId == b.BMPTypeId && c.BMPEffectivenessTypeId == 22);
                    sus = spatialUnitAvailable.Where(x => results.Select(o => o.LocationId).ToHashSet().Contains(x.LocationId));

                    foreach (var sua in sus)
                    {
                        if (!existingSU.Contains(sua.LocationId))
                        {
                            decimal area;
                            switch (b.BMPTypeId)
                            {
                                case 4:
                                    area = suAvailableVfst[sua.LocationId];
                                    break;
                                case 5:
                                    area = suAvailableRibuf[sua.LocationId];
                                    break;
                                case 6:
                                    area = suAvailableGww[sua.LocationId];
                                    break;
                                default:
                                    area = sua.Area;
                                    break;
                            }

                            var dto = new BMPGeometryCostEffectivenessDTO
                            {
                                LocationId = sua.LocationId,
                                FarmId = sua.FarmId,
                                Geometry = sua.Geometry,
                                BMPCombinationTypeId = b.BMPTypeId,
                                BMPCombinationTypeName = b.BMPTypeName,
                                BMPArea = area,
                                OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision : (int)OptimizationSolutionLocationTypeEnum.Parcel,
                                EffectivenessDTOs =
                                    (from r in results
                                     where r.LocationId == sua.LocationId
                                     group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                     select new EffectivenessDTO
                                     {
                                         BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                         ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                         BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                         EffectivenessValue = g.Average(x => x)
                                     }).ToList(),
                                Cost =
                                    (from c in costs
                                     where c.LocationId == sua.LocationId // cost
                                     group c.Cost by c.BMPEffectivenessTypeId into g
                                     select g.Average(x => x)).FirstOrDefault(),
                                IsSelectable = true
                            };

                            lock (lockMe)
                            {
                                res.Add(dto);
                            }
                        }
                    }
                }
            });

            return res;
        }

        public List<BMPGeometryCostEffectivenessDTO> GetBMPGeometryCostEffectivenessDTOInternalDbContext(
            AgBMPToolContext db,
            int ProjectSpatialUnitTypeId,
            int ScenarioTypeId,
            HashSet<int> currScenIds,
            HashSet<int> modelComponentIds,
            int StartYear,
            int EndYear,
            int largestSingleBMPId)
        {
            // Get available modelcomponents 
            var uss = db.UnitScenarios.Where(u => modelComponentIds.Contains(u.ModelComponentId) && currScenIds.Contains(u.ScenarioId) && u.BMPCombinationId <= largestSingleBMPId).ToList();
            var ussIds = uss.Select(o => o.Id).ToHashSet();
            var availables = (from u in uss
                              join mc in db.ModelComponents.Where(mc => modelComponentIds.Contains(mc.Id)) on u.ModelComponentId equals mc.Id
                              join w in db.Watersheds on mc.WatershedId equals w.Id
                              join r in db.Reaches on w.OutletReachId equals r.Id
                              join ue in db.UnitScenarioEffectivenesses.Where(ue => ussIds.Contains(ue.UnitScenarioId) && ue.Year >= StartYear && ue.Year <= EndYear) on u.Id equals ue.UnitScenarioId
                              join bet in db.BMPEffectivenessTypes on ue.BMPEffectivenessTypeId equals bet.Id
                              join bc in db.BMPCombinationTypes on u.BMPCombinationId equals bc.Id
                              join mct in db.ModelComponentTypes on bc.ModelComponentTypeId equals mct.Id
                              select new { u.ScenarioId, BMPTypeId = bc.Id, BMPTypeName = bc.Name, mct.IsStructure, u.ModelComponentId, OutletReachMCId = r.ModelComponentId, ue.Year, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId, BMPEffectivenessTypeId = bet.Id, PercentChange = ue.Value }).ToList();

            // Get existing modelcomponents
            var webts = db.WatershedExistingBMPTypes.Where(webt => modelComponentIds.Contains(webt.ModelComponentId) && webt.ScenarioTypeId == ScenarioTypeId).ToList();
            var webtIds = webts.Select(o => o.Id).ToHashSet();
            var existings = (from webt in webts
                             join mc in db.ModelComponents.Where(mc => modelComponentIds.Contains(mc.Id)) on webt.ModelComponentId equals mc.Id
                             join w in db.Watersheds on mc.WatershedId equals w.Id
                             join s in db.Scenarios on $"{w.Id}_{webt.ScenarioTypeId}" equals $"{s.WatershedId}_{s.ScenarioTypeId}"
                             join r in db.Reaches on w.OutletReachId equals r.Id
                             join ue in db.UnitScenarioEffectivenesses.Where(ue => webtIds.Contains(ue.UnitScenarioId) && ue.Year >= StartYear && ue.Year <= EndYear) on webt.Id equals ue.UnitScenarioId
                             join bet in db.BMPEffectivenessTypes on ue.BMPEffectivenessTypeId equals bet.Id
                             join bc in db.BMPTypes on webt.BMPTypeId equals bc.Id
                             join mct in db.ModelComponentTypes on bc.ModelComponentTypeId equals mct.Id
                             select new { ScenarioId = s.Id, BMPTypeId = bc.Id, BMPTypeName = bc.Name, mct.IsStructure, webt.ModelComponentId, OutletReachMCId = r.ModelComponentId, ue.Year, bet.BMPEffectivenessLocationTypeId, bet.ScenarioModelResultVariableTypeId, BMPEffectivenessTypeId = bet.Id, PercentChange = ue.Value }).ToList();

            var bmpIdNames = availables.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure }).Union(existings.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure })).Distinct().ToList();

            var allSubAreaMCIdsWithBMP = availables.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)
                                       .Union(existings.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)).Distinct().ToHashSet();

            var subAreaSpatialUnits = (from sa in db.SubAreas.Where(sa => allSubAreaMCIdsWithBMP.Contains(sa.ModelComponentId))
                                       select new
                                       {
                                           SubAreaMcId = sa.ModelComponentId,
                                           SpatialUnitId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? sa.LegalSubDivisionId : sa.ParcelId
                                       }).ToList();

            List<BMPGeometryCostEffectivenessDTO> res = new List<BMPGeometryCostEffectivenessDTO>();

            // Get reach BMP ModelComponent id and geometry
            var reachBMPAvailable = this.GetReachBMPByMCIdsDbContext(db, availables.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true);
            var reachBMPExisting = this.GetReachBMPByMCIdsDbContext(db, existings.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true);

            // Get modelcomponent ids including outlet reach MC id
            var availableReachMcIds = reachBMPAvailable.Select(o => o.ReachMCId).Union(availables.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var existingReachMcIds = reachBMPExisting.Select(o => o.ReachMCId).Union(existings.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var availableSubAreaMcIds = availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(availables.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var existingSubAreaMcIds = existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(existings.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();

            // Append Scenario model result value and value change
            var availablesReachSMRValue = (from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) // onsite use bmp reach modelcomponent id
                                           join r in reachBMPAvailable on a.ModelComponentId equals r.LocationId
                                           join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                           join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.ReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                           select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                      join r in reachBMPAvailable on a.ModelComponentId equals r.LocationId
                                      join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{a.OutletReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).ToList();

            // Append Scenario model result value and value change
            var existingsReachSMRValue = (from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) // onsite use bmp reach modelcomponent id
                                          join r in reachBMPExisting on e.ModelComponentId equals r.LocationId
                                          join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                          join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.ReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                          select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                      join r in reachBMPExisting on e.ModelComponentId equals r.LocationId
                                      join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{e.OutletReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).ToList();

            // Get LSD/Parcel id and geometry
            var spatialUnitAvailable = this.GetSpatialUnitBySubAreaMCIdsDbContext(db, availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            var spatialUnitExisting = this.GetSpatialUnitBySubAreaMCIdsDbContext(db, existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            var suAvailableVfst =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 4,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 4).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suAvailableRibuf =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 5,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 5).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suAvailableGww =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 6,
                    availables.Where(o => !o.IsStructure && o.BMPTypeId == 6).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );

            var suExistingVfst =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 4,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 4).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suExistingRibuf =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 5,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 5).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );
            var suExistingGww =
                this.GetSpatialUnitBySubAreaMCIdsVfstRibufGwwDbContext(db, 6,
                    existings.Where(o => !o.IsStructure && o.BMPTypeId == 6).Select(o => o.ModelComponentId).ToHashSet(),
                    ProjectSpatialUnitTypeId
                    );

            // Append Scenario model result value and value change
            var availablesSubAreaSMRValue = (from a in availables.Where(a => !a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) // onsite use subarea modelcomponent id
                                             join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                             join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{a.ModelComponentId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                             select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (from a in availables.Where(a => !a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                      join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{a.OutletReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).ToList();

            // Append Scenario model result value and value change
            var existingsSubAreaSMRValue = (from e in existings.Where(e => !e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite) // onsite use subarea modelcomponent id
                                            join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                            join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{e.ModelComponentId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                            select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (from e in existings.Where(e => !e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                      join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                      join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{e.OutletReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                      select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).ToList();

            // Aggregate to LSD/parcel level results
            var spatialUnitAvailableResults = (
                from a in availablesSubAreaSMRValue.Select(o => o)
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                group a.ValueChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }).ToList();

            var spatialUnitExistingResults = (
                from a in existingsSubAreaSMRValue
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                group a.ValueChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }).ToList();

            // Aggregate cost to LSD/parcel level results
            var spatialUnitAvailableCost = (
                from a in availables.Select(o => o)
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                where a.BMPEffectivenessTypeId == 22 // cost
                group a.PercentChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessTypeId, Cost = g.Sum(x => x) }).ToList();

            var spatialUnitExistingCost = (
                from e in existings.Select(o => o)
                join sa2su in subAreaSpatialUnits on e.ModelComponentId equals sa2su.SubAreaMcId
                where e.BMPEffectivenessTypeId == 22 // cost
                group e.PercentChange by new { sa2su.SpatialUnitId, e.Year, e.BMPEffectivenessTypeId, e.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessTypeId, Cost = g.Sum(x => x) }).ToList();

            foreach (var b in bmpIdNames)
            {
                if (b.IsStructure)
                {
                    var results = availablesReachSMRValue.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    var costs = availables.Where(r => r.BMPTypeId == b.BMPTypeId && r.BMPEffectivenessTypeId == 22);
                    var rbs = reachBMPAvailable.Where(x => results.Select(o => o.ModelComponentId).ToHashSet().Contains(x.LocationId));

                    foreach (var rb in rbs)
                    {
                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = rb.LocationId,
                            FarmId = rb.FarmId,
                            Geometry = rb.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = rb.Area,
                            OptimizationSolutionLocationTypeId = (int)OptimizationSolutionLocationTypeEnum.ReachBMP,
                            EffectivenessDTOs =
                                (from r in results
                                 where r.ModelComponentId == rb.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from r in costs
                                 where r.ModelComponentId == rb.LocationId // cost
                                 group r.PercentChange by r.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = true
                        };

                        res.Add(dto);
                    }

                    costs = existings.Where(r => r.BMPTypeId == b.BMPTypeId && r.BMPEffectivenessTypeId == 22);
                    results = existingsReachSMRValue.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    rbs = reachBMPAvailable.Where(x => results.Select(o => o.ModelComponentId).ToHashSet().Contains(x.LocationId));
                    foreach (var rb in rbs)
                    {
                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = rb.LocationId,
                            FarmId = rb.FarmId,
                            Geometry = rb.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = rb.Area,
                            OptimizationSolutionLocationTypeId = (int)OptimizationSolutionLocationTypeEnum.ReachBMP,
                            EffectivenessDTOs =
                                (from r in results
                                 where r.ModelComponentId == rb.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from r in costs
                                 where r.ModelComponentId == rb.LocationId // cost
                                 group r.PercentChange by r.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = false
                        };

                        res.Add(dto);
                    }

                }
                else
                {
                    var results = spatialUnitExistingResults.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    var costs = spatialUnitExistingCost.Where(c => c.BMPTypeId == b.BMPTypeId && c.BMPEffectivenessTypeId == 22);
                    var sus = spatialUnitExisting.Where(x => results.Select(o => o.LocationId).ToHashSet().Contains(x.LocationId));

                    HashSet<int> existingSU = new HashSet<int>();

                    foreach (var sue in sus)
                    {
                        decimal area;
                        switch (b.BMPTypeId)
                        {
                            case 4:
                                area = suExistingVfst[sue.LocationId];
                                break;
                            case 5:
                                area = suExistingRibuf[sue.LocationId];
                                break;
                            case 6:
                                area = suExistingGww[sue.LocationId];
                                break;
                            default:
                                area = sue.Area;
                                break;
                        }

                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = sue.LocationId,
                            FarmId = sue.FarmId,
                            Geometry = sue.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = area,
                            OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision : (int)OptimizationSolutionLocationTypeEnum.Parcel,
                            EffectivenessDTOs =
                                (from r in results
                                 where r.LocationId == sue.LocationId
                                 group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                 select new EffectivenessDTO
                                 {
                                     BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                     ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                     BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                     EffectivenessValue = g.Average(x => x)
                                 }).ToList(),
                            Cost =
                                (from c in costs
                                 where c.LocationId == sue.LocationId // cost
                                 group c.Cost by c.BMPEffectivenessTypeId into g
                                 select g.Average(x => x)).FirstOrDefault(),
                            IsSelectable = false
                        };

                        res.Add(dto);
                        existingSU.Add(sue.LocationId);
                    }

                    results = spatialUnitAvailableResults.Where(xx => xx.BMPTypeId == b.BMPTypeId);
                    costs = spatialUnitAvailableCost.Where(c => c.BMPTypeId == b.BMPTypeId && c.BMPEffectivenessTypeId == 22);
                    sus = spatialUnitAvailable.Where(x => results.Select(o => o.LocationId).ToHashSet().Contains(x.LocationId));

                    foreach (var sua in sus)
                    {
                        if (!existingSU.Contains(sua.LocationId))
                        {
                            decimal area;
                            switch (b.BMPTypeId)
                            {
                                case 4:
                                    area = suAvailableVfst[sua.LocationId];
                                    break;
                                case 5:
                                    area = suAvailableRibuf[sua.LocationId];
                                    break;
                                case 6:
                                    area = suAvailableGww[sua.LocationId];
                                    break;
                                default:
                                    area = sua.Area;
                                    break;
                            }

                            var dto = new BMPGeometryCostEffectivenessDTO
                            {
                                LocationId = sua.LocationId,
                                FarmId = sua.FarmId,
                                Geometry = sua.Geometry,
                                BMPCombinationTypeId = b.BMPTypeId,
                                BMPCombinationTypeName = b.BMPTypeName,
                                BMPArea = area,
                                OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? (int)OptimizationSolutionLocationTypeEnum.LegalSubDivision : (int)OptimizationSolutionLocationTypeEnum.Parcel,
                                EffectivenessDTOs =
                                    (from r in results
                                     where r.LocationId == sua.LocationId
                                     group r.ValueChange by new { r.BMPEffectivenessLocationTypeId, r.ScenarioModelResultVariableTypeId, r.BMPEffectivenessTypeId } into g
                                     select new EffectivenessDTO
                                     {
                                         BMPEffectivenessLocationTypeId = g.Key.BMPEffectivenessLocationTypeId,
                                         ScenarioModelResultVariableTypeId = (int)g.Key.ScenarioModelResultVariableTypeId,
                                         BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                                         EffectivenessValue = g.Average(x => x)
                                     }).ToList(),
                                Cost =
                                    (from c in costs
                                     where c.LocationId == sua.LocationId // cost
                                     group c.Cost by c.BMPEffectivenessTypeId into g
                                     select g.Average(x => x)).FirstOrDefault(),
                                IsSelectable = true
                            };

                            res.Add(dto);
                        }
                    }
                }
            }

            return res;
        }

        public virtual List<BMPGeometryCostEffectivenessDTO> GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(int projectId)
        {
            var pu = (from p in _uow.GetRepository<Project>().Query()
                      join u in _uow.GetRepository<User>().Query() on p.UserId equals u.Id
                      where p.Id == projectId
                      select new { p.ProjectSpatialUnitTypeId, p.ScenarioTypeId, UserId = u.Id, u.UserTypeId, p.StartYear, p.EndYear }).FirstOrDefault();

            // Get all subareaIds
            HashSet<int> subAreaIds = _uss.GetSubAreaIds(projectId, pu.UserId, pu.UserTypeId);

            // Get all modelcomponentIds based on subareaIds
            HashSet<int> modelComponentIds = _uss.GetAllModelComponentIdsBySubAreaIds(subAreaIds);

            HashSet<int> currScenIds = _uss.GetScenarioIdsBySubAreaMcIds(subAreaIds, pu.ScenarioTypeId);

            // Remove all combined BMPs
            int largestSingleBMPId = _uow.GetRepository<BMPType>().Get().Select(o => o.Id).Max();

            return this.GetBMPGeometryCostEffectivenessDTOInternal(pu.ProjectSpatialUnitTypeId, pu.ScenarioTypeId, currScenIds, modelComponentIds, pu.StartYear, pu.EndYear, largestSingleBMPId);
        }

        public virtual List<ProjectSummaryDTO> GetProjectSummaryDTOs(List<ProjectDTO> projectDTOs)
        {
            // Set user id to current service
            this.SetUserId(1);

            // Define results
            List<ProjectSummaryDTO> res = new List<ProjectSummaryDTO>();

            Object lockMe = new object();

            // Foreach projectDTO
            Parallel.ForEach(projectDTOs, p =>
            {
                ProjectSummaryDTO r = new ProjectSummaryDTO
                {
                    EffectivenessSummaryDTOs = this.GetProjectEffectivenessSummaries(p.Id),
                    BMPSummaryDTOs = this.GetProjectBMPSummaries(p.Id),
                    Cost = this.GetProjectCost(p.Id),
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

                lock (lockMe)
                {
                    res.Add(r);
                }
            });


            // Return results
            return res;
        }

        public virtual List<ProjectSummaryDTO> GetProjectBaselineSummaryDTOs(List<ProjectDTO> projectDTOs)
        {
            // Set user id to current service
            this.SetUserId(1);

            // Define results
            List<ProjectSummaryDTO> res = new List<ProjectSummaryDTO>();

            Object lockMe = new object();
            // Foreach projectDTO
            Parallel.ForEach(projectDTOs, p =>
            {
                ProjectSummaryDTO d = new ProjectSummaryDTO
                {
                    EffectivenessSummaryDTOs = this.GetProjectBaselineEffectiveness(p.Id),
                    BMPSummaryDTOs = this.GetProjectBaselineBMPSummaries(p.Id),
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
                d.Cost = d.BMPSummaryDTOs.Select(x => x.TotalCost).Sum();

                lock (lockMe)
                {
                    res.Add(d);
                }
            });

            // Return results
            return res;
        }

        private int _userId = -999;

        private void SetUserId(int userId)
        {
            if (_userId != userId)
            {
                _userId = userId;
                _dtm = new DBToMemoryUser(_uow, userId);
            }
        }

        private List<BMPGeometryCostEffectivenessDTO> _BMPGeometryCostEffectivenessDTOs { get; set; }

        public int _projectId { get; set; } = -999;

        private void InitBMPGeometryCostEffectivenessDTO(int projectId)
        {
            if (_projectId == projectId) return;

            _BMPGeometryCostEffectivenessDTOs = GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId);
        }

        public virtual List<BMPTypeLocationsDTO> GetBMPTypeLocationsForBMPSelectionAndOverview(int projectId)
        {
            InitBMPGeometryCostEffectivenessDTO(projectId);

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

        public virtual List<Geometry> GetBMPGeometriesForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            this.InitBMPGeometryCostEffectivenessDTO(projectId);

            return _BMPGeometryCostEffectivenessDTOs.Where(x => x.BMPCombinationTypeId == bmpTypeId).Select(o => o.Geometry).ToList();
        }

        public virtual List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            this.InitBMPGeometryCostEffectivenessDTO(projectId);

            return _BMPGeometryCostEffectivenessDTOs.FindAll(x => x.BMPCombinationTypeId == bmpTypeId).ToList<BMPCostAllEffectivenessDTO>();
        }

        public virtual List<BMPCostSingleEffectivenessDTO> GetSingleBMPCostSingleEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId, int smrvTypeId)
        {
            this.InitBMPGeometryCostEffectivenessDTO(projectId);

            return (from b in _BMPGeometryCostEffectivenessDTOs.FindAll(x => x.BMPCombinationTypeId == bmpTypeId)
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
                    }).ToList();
        }

        public virtual List<BMPGeometryCostEffectivenessDTO> GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId)
        {
            return this.GetAllBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(projectId);
        }

        public virtual List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId)
        {
            this.InitBMPGeometryCostEffectivenessDTO(projectId);

            return _BMPGeometryCostEffectivenessDTOs.FindAll(x => x.BMPCombinationTypeId == bmpTypeId).ToList();
        }

        public List<LsdParcelBMPSummaryDTO> GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            throw new NotImplementedException();
        }

        public List<BMPCostSingleEffectivenessDTO> GetSingleBMPCostSingleEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId, int smrvTypeId)
        {
            throw new NotImplementedException();
        }

        public List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            throw new NotImplementedException();
        }

        public List<BMPTypeLocationsDTO> GetBMPTypeLocationsForBMPScopeAndIntelligentRecommendation(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<Geometry> GetBMPGeometriesForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId)
        {
            throw new NotImplementedException();
        }

        public ProjectSummaryDTO GetProjectSummaryDTO(int projectId)
        {
            throw new NotImplementedException();
        }

        public ProjectSummaryDTO GetProjectBaselineSummaryDTO(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<BMPSummaryDTO> GetProjectBMPSummaryDTOs(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<EffectivenessSummaryDTO> GetProjectEffectivenessSummaryDTOs(int projectId)
        {
            throw new NotImplementedException();
        }

        decimal IProjectSummaryService.GetProjectCost(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<BMPSummaryDTO> GetProjectBaselineBMPSummaryDTOs(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<EffectivenessSummaryDTO> GetProjectBaselineEffectivenessSummaryDTOs(int projectId)
        {
            throw new NotImplementedException();
        }

        protected class LocalEffectSummary
        {
            public decimal Value { get; set; }
            public decimal ValueChange { get; set; }
            public decimal PercentChange { get; set; }
        }
    }
}
