using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ModelComponent;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using AgBMPTool.BLL.DLLException;
using System.Reflection;
using AgBMTool.DBL.Interface;
using AgBMPTool.BLL.Services.Utilities;
using ESAT.Import.Utils.Database;
using System.Threading.Tasks;
using AgBMPTool.DBModel;
using AgBMPTool.BLL.Enumerators;

namespace ESAT.Import.Model
{
    public class UnitOptimizationSolutionBuilder
    {
        public readonly IUnitOfWork _uow;
        public readonly IUtilitiesSummaryService _uss;
        public UnitOptimizationSolutionBuilder(IUnitOfWork _iUnitOfWork, IUtilitiesSummaryService _iUtilitiesSummaryService)
        {
            _uow = _iUnitOfWork;
            _uss = _iUtilitiesSummaryService;
        }

        public List<UnitOptimizationSolution> GetUnitOptimizationSolutionsByWatershedIdParallel(int watershedId)
        {
            // Initial current UnitOptimizationSolutionId
            this.FindCurrUnitOptimizationSolutionId();

            // Get all subareaIds
            HashSet<int> subAreaIds = this.GetSubAreaIds(watershedId);

            // Get all modelcomponentIds based on subareaIds
            HashSet<int> modelComponentIds = _uss.GetAllModelComponentIdsBySubAreaIds(subAreaIds);

            // Include both single and combined BMPs
            int largestSingleBMPId = int.MaxValue;

            var res = new List<UnitOptimizationSolution>();

            // For all scenario types
            foreach (int scenarioTypeId in _uow.GetRepository<ScenarioType>().Query().Select(o => o.Id))
            {
                HashSet<int> currScenIds = _uss.GetScenarioIdsBySubAreaMcIds(subAreaIds, scenarioTypeId);
                int scenId = currScenIds.FirstOrDefault();

                // For each spatial unit type
                var projectSpatialUnitTypeIds = _uow.GetRepository<ProjectSpatialUnitType>().Get().Select(o => o.Id).ToList();

                var years = this.GetYears(scenId, modelComponentIds, largestSingleBMPId);

                // For each year
                Parallel.ForEach(years, year =>
                {
                    int projectSpatialUnitTypeId = 1; // LSD level

                    if (Program.IS_TESTING)
                    {
                        Console.WriteLine($"{Program.PROGRAM_NAME}>> ***Start***" +
                                $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                    }

                    // Get all BMP DTOs for LSD
                    var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext;
                    var bmpGeometryCostEffectivenessDTOs = this.GetBMPGeometryCostEffectivenessDTOInternalDbContext(db, projectSpatialUnitTypeId, scenarioTypeId, currScenIds, modelComponentIds, year, year, largestSingleBMPId, true);
                    db.Dispose();

                    lock (LockMe)
                    {
                        if (Program.IS_TESTING)
                        {
                            Console.WriteLine($"{Program.PROGRAM_NAME}>> ***Lock***" +
                                    $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                        }

                        // Add field BMP DTOs
                        this.ConvertBMPCostEffectivenessToUnitOptimizationSolutions(scenId, year,
                                bmpGeometryCostEffectivenessDTOs.FindAll(x => x.OptimizationSolutionLocationTypeId != (int)OptimizationSolutionLocationTypeEnum.ReachBMP),
                                res
                                );

                        if (Program.IS_TESTING)
                        {
                            Console.WriteLine($"{Program.PROGRAM_NAME}>> " +
                                    $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                        }

                        this.ConvertBMPCostEffectivenessToUnitOptimizationSolutions(scenId, year,
                                   bmpGeometryCostEffectivenessDTOs.FindAll(x => x.OptimizationSolutionLocationTypeId == (int)OptimizationSolutionLocationTypeEnum.ReachBMP),
                                   res
                                   );

                        if (Program.IS_TESTING)
                        {
                            Console.WriteLine($"{Program.PROGRAM_NAME}>> " +
                                $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-ReachBMP");
                        }
                    }

                    projectSpatialUnitTypeId = 2; // parcel level
                    // Get all BMP DTOs for parcel
                    db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext;
                    bmpGeometryCostEffectivenessDTOs = this.GetBMPGeometryCostEffectivenessDTOInternalDbContext(db, projectSpatialUnitTypeId, scenarioTypeId, currScenIds, modelComponentIds, year, year, largestSingleBMPId, false);
                    db.Dispose();

                    lock (LockMe)
                    {
                        if (Program.IS_TESTING)
                        {
                            Console.WriteLine($"{Program.PROGRAM_NAME}>> ***Lock***" +
                                    $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                        }

                        // Add field BMP DTOs
                        this.ConvertBMPCostEffectivenessToUnitOptimizationSolutions(scenId, year,
                                bmpGeometryCostEffectivenessDTOs.FindAll(x => x.OptimizationSolutionLocationTypeId != (int)OptimizationSolutionLocationTypeEnum.ReachBMP),
                                res
                                );

                        if (Program.IS_TESTING)
                        {
                            Console.WriteLine($"{Program.PROGRAM_NAME}>> " +
                                    $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                        }
                    }

                    if (Program.IS_TESTING)
                    {
                        Console.WriteLine($"{Program.PROGRAM_NAME}>> ***End***" +
                                $"ScenarioType-{scenarioTypeId} Year-{year} SpatialUnit-{projectSpatialUnitTypeId}");
                    }
                });
            }

            if (Program.IS_TESTING)
            {
                Console.WriteLine($"{Program.PROGRAM_NAME}>> " +
                    $"Saving to DB");
            }
            return res;
        }

        private readonly int ONE_TIME_INSERT_COUNT = 100000;

        public void BuildUnitOptimizationSolutionBulkInsert(int watershedId)
        {
            var uoss = this.GetUnitOptimizationSolutionsByWatershedIdParallel(watershedId);
            var uoses = uoss.SelectMany(o => o.UnitOptimizationSolutionEffectivenesses).ToList();

            var uossHelper = PostgresBulkInsertHelper.CreateHelper<UnitOptimizationSolution>("public", nameof(UnitOptimizationSolution));
            var uosesHelper = PostgresBulkInsertHelper.CreateHelper<UnitOptimizationSolutionEffectiveness>("public", nameof(UnitOptimizationSolutionEffectiveness));

            if (Program.IS_TESTING) { Console.WriteLine($"*** Start inserting BuildUnitOptimizationSolutionBulkInsert - uoss {uoss.Count} records - uoses {uoses.Count} records ***"); }

            using (var connection = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolNpgsqlConnection)
            {
                connection.Open();

                if (uoss.Count <= this.ONE_TIME_INSERT_COUNT)
                {
                    uossHelper.SaveAll(connection, uoss);
                }
                else
                {
                    int index = 0;
                    int left = uoss.Count;

                    while (left > 0)
                    {
                        int count = Math.Min(ONE_TIME_INSERT_COUNT, left);

                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE {count} RESULTS ****"); }
                        uossHelper.SaveAll(connection, uoss.GetRange(index, count));

                        index += count;
                        left -= count;
                    }
                }


                if (uoses.Count <= this.ONE_TIME_INSERT_COUNT)
                {
                    uosesHelper.SaveAll(connection, uoses);
                }
                else
                {
                    int index = 0;
                    int left = uoses.Count;

                    while (left > 0)
                    {
                        int count = Math.Min(ONE_TIME_INSERT_COUNT, left);

                        if (Program.IS_TESTING) { Console.WriteLine($"{Program.PROGRAM_NAME}>> **** SAVE {count} RESULTS ****"); }
                        uosesHelper.SaveAll(connection, uoses.GetRange(index, count));

                        index += count;
                        left -= count;
                    }
                }

                connection.Close();
            }

            if (Program.IS_TESTING) { Console.WriteLine($"*** End inserting BuildUnitOptimizationSolutionBulkInsert ***"); }
        }

        private int CurrUnitOptimizationSolutionId { get; set; }

        private void FindCurrUnitOptimizationSolutionId()
        {
            var uos = _uow.GetRepository<UnitOptimizationSolution>().Get();

            if (uos == null || uos.Count() == 0)
            {
                CurrUnitOptimizationSolutionId = 0;
            } else
            {
                CurrUnitOptimizationSolutionId = uos.Select(o => o.Id)?.Max() ?? 0;
            }            
        }

        private readonly object LockMe = new object();

        private void ConvertBMPCostEffectivenessToUnitOptimizationSolutions(int scenId, int year, List<BMPGeometryCostEffectivenessDTO> bmpGeometryCostEffectivenessDTOs, 
            List<UnitOptimizationSolution> inputRes)
        {
            var uosFind =
                (from r in inputRes
                join b in bmpGeometryCostEffectivenessDTOs
                    on $"{r.BMPCombinationId}_{r.LocationId}_{r.OptimizationSolutionLocationTypeId}_{r.ScenarioId}"
                    equals $"{b.BMPCombinationTypeId}_{b.LocationId}_{b.OptimizationSolutionLocationTypeId}_{scenId}"
                select new { r, b }).ToList();

            foreach (var uosFindrAndb in uosFind)
            {
                // Add cost to effectiveness
                uosFindrAndb.r.UnitOptimizationSolutionEffectivenesses.Add(new UnitOptimizationSolutionEffectiveness
                {
                    UnitOptimizationSolutionId = uosFindrAndb.r.Id,
                    BMPEffectivenessTypeId = 22,
                    Value = uosFindrAndb.b.Cost,
                    Year = year
                });

                // Add other effectiveness in BMPDTOs to effectiveness
                uosFindrAndb.r.UnitOptimizationSolutionEffectivenesses
                    .AddRange(uosFindrAndb.b.EffectivenessDTOs.Select(be => new UnitOptimizationSolutionEffectiveness
                    {
                        UnitOptimizationSolutionId = uosFindrAndb.r.Id,
                        BMPEffectivenessTypeId = be.BMPEffectivenessTypeId,
                        Value = be.EffectivenessValue,
                        Year = year
                    }).ToList());
            }

            var finders = uosFind.Select(o => $"{o.b.BMPCombinationTypeId}_{o.b.LocationId}_{o.b.OptimizationSolutionLocationTypeId}").Distinct().ToHashSet();

            foreach (var b in bmpGeometryCostEffectivenessDTOs.Where(b => !finders.Contains($"{b.BMPCombinationTypeId}_{b.LocationId}_{b.OptimizationSolutionLocationTypeId}")))
            {
                CurrUnitOptimizationSolutionId++;
                var uos = new UnitOptimizationSolution
                {
                    Id = CurrUnitOptimizationSolutionId,
                    BMPArea = b.BMPArea,
                    BMPCombinationId = b.BMPCombinationTypeId,
                    FarmId = b.FarmId,
                    Geometry = b.Geometry,
                    IsExisting = !b.IsSelectable,
                    LocationId = b.LocationId,
                    OptimizationSolutionLocationTypeId = b.OptimizationSolutionLocationTypeId,
                    ScenarioId = scenId,
                    UnitOptimizationSolutionEffectivenesses = new List<UnitOptimizationSolutionEffectiveness>()
                };

                // Add cost to effectiveness
                uos.UnitOptimizationSolutionEffectivenesses.Add(new UnitOptimizationSolutionEffectiveness
                {
                    UnitOptimizationSolutionId = uos.Id,
                    BMPEffectivenessTypeId = 22,
                    Value = b.Cost,
                    Year = year
                });

                // Add other effectiveness in BMPDTOs to effectiveness
                uos.UnitOptimizationSolutionEffectivenesses
                    .AddRange(b.EffectivenessDTOs.Select(be => new UnitOptimizationSolutionEffectiveness
                    {
                        UnitOptimizationSolutionId = uos.Id,
                        BMPEffectivenessTypeId = be.BMPEffectivenessTypeId,
                        Value = be.EffectivenessValue,
                        Year = year
                    }).ToList());

                inputRes.Add(uos);
            }
                        
        }

        private HashSet<int> GetSubAreaIds(int watershedId)
        {
            return (from sa in _uow.GetRepository<SubArea>().Query()
                    join mc in _uow.GetRepository<ModelComponent>().Query() on sa.ModelComponentId equals mc.Id
                    where mc.WatershedId == watershedId
                    select sa.Id).ToHashSet();
        }

        private List<int> GetYears(int scenId, HashSet<int> modelComponentIds, int largestSingleBMPId)
        {
            return (from u in _uow.GetRepository<UnitScenario>().Query()
                    join mc in _uow.GetRepository<ModelComponent>().Query() on u.ModelComponentId equals mc.Id
                    join ue in _uow.GetRepository<UnitScenarioEffectiveness>().Query() on u.Id equals ue.UnitScenarioId
                    where modelComponentIds.Contains(u.ModelComponentId) && scenId == u.ScenarioId && u.BMPCombinationId <= largestSingleBMPId
                    select ue.Year).Distinct().ToList();
        }

        public List<BMPGeometryCostEffectivenessDTO> GetBMPGeometryCostEffectivenessDTOInternalDbContext(
            AgBMPToolContext db,
            int ProjectSpatialUnitTypeId,
            int ScenarioTypeId,
            HashSet<int> currScenIds,
            HashSet<int> modelComponentIds,
            int StartYear,
            int EndYear,
            int largestSingleBMPId,
            bool hasReachBMP)
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

            if (!hasReachBMP)
            {
                availables = availables.FindAll(x => !x.IsStructure);
                existings = existings.FindAll(x => !x.IsStructure);
            }

            var bmpIdNames = availables.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure }).Union(existings.Select(o => new { o.BMPTypeId, o.BMPTypeName, o.IsStructure })).Distinct().ToList();

            var allSubAreaMCIdsWithBMP = availables.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)
                                       .Union(existings.Where(x => !x.IsStructure).Select(o => o.ModelComponentId)).Distinct().ToHashSet();

            var subAreaSpatialUnits = (from sa in db.SubAreas.Where(sa => allSubAreaMCIdsWithBMP.Contains(sa.ModelComponentId))
                                       select new
                                       {
                                           SubAreaMcId = sa.ModelComponentId,
                                           SpatialUnitId = ProjectSpatialUnitTypeId == (int)Enumerators.ProjectSpatialUnitTypeEnum.LSD
                                                            ? sa.LegalSubDivisionId : sa.ParcelId,
                                           sa.Area
                                       }).ToList();

            List<BMPGeometryCostEffectivenessDTO> res = new List<BMPGeometryCostEffectivenessDTO>();

            // Get reach BMP ModelComponent id and geometry
            var reachBMPAvailable = hasReachBMP ? this.GetReachBMPByMCIdsDbContext(db, availables.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true) : null;
            var reachBMPExisting = hasReachBMP ? this.GetReachBMPByMCIdsDbContext(db, existings.Where(o => o.IsStructure).Select(o => o.ModelComponentId).Distinct().ToHashSet(), true) : null;

            // Get modelcomponent ids including outlet reach MC id
            var availableReachMcIds = hasReachBMP ? reachBMPAvailable.Select(o => o.ReachMCId).Distinct().ToHashSet() : null;
            var existingReachMcIds = hasReachBMP ? reachBMPExisting.Select(o => o.ReachMCId).Distinct().ToHashSet() : null;
            var availableOutletReachMcIds = hasReachBMP ? availables.Select(o => o.OutletReachMCId).Distinct().ToHashSet() : null;
            var existingOutletReachMcIds = hasReachBMP ? existings.Select(o => o.OutletReachMCId).Distinct().ToHashSet() : null;
            var availableReachBMPSubAreaMcIds = hasReachBMP ? reachBMPAvailable.Select(o => o.SubAreaMCId).Distinct().ToHashSet() : null;
            var existingReachBMPSubAreaMcIds = hasReachBMP ? reachBMPExisting.Select(o => o.SubAreaMCId).Distinct().ToHashSet() : null;
            var availableSubAreaMcIds = availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(availables.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();
            var existingSubAreaMcIds = existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).Union(existings.Select(o => o.OutletReachMCId)).Distinct().ToHashSet();

            // Append Scenario model result value and value change
            /** There are reachBMP effectiveness that are based on subarea scenario model results
                1	Soil moisture onsite
                2	ET onsite
                3	Groundwater recharge onsite
                4	Runoff onsite
                12	Soil carbon onsite
                13	Biodiversity onsite
            */
            HashSet<int> reachBMPEffeBasedOnSubAreaSMR = new HashSet<int> { 1, 2, 3, 4, 12, 13 }; 

            var availablesReachSMRValue = hasReachBMP ? 
                                    (
                                        from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite && reachBMPEffeBasedOnSubAreaSMR.Contains(a.BMPEffectivenessTypeId)) // onsite effe. based on bmp subarea modelcomponent id
                                        join r in reachBMPAvailable on a.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachBMPSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.SubAreaMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (
                                        from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite && !reachBMPEffeBasedOnSubAreaSMR.Contains(a.BMPEffectivenessTypeId)) // onsite effe. based on bmp reach modelcomponent id
                                        join r in reachBMPAvailable on a.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.ReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).Union
                                     (
                                        from a in availables.Where(a => a.IsStructure && a.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                        join r in reachBMPAvailable on a.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{a.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && availableOutletReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{a.OutletReachMCId}_{a.ScenarioId}_{a.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { a.ModelComponentId, a.BMPTypeId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = a.PercentChange * smr.Value / 100, a.PercentChange }
                                     ).ToList() 
                                     : null;

            // Append Scenario model result value and value change
            var existingsReachSMRValue = hasReachBMP ? 
                                    (
                                        from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite && reachBMPEffeBasedOnSubAreaSMR.Contains(e.BMPEffectivenessTypeId)) // onsite effe. based on bmp subarea modelcomponent id
                                        join r in reachBMPExisting on e.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{1}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachBMPSubAreaMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.SubAreaMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (
                                        from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite && !reachBMPEffeBasedOnSubAreaSMR.Contains(e.BMPEffectivenessTypeId)) // onsite effe. based on bmp reach modelcomponent id
                                        join r in reachBMPExisting on e.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{r.ReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).Union
                                     (
                                        from e in existings.Where(e => e.IsStructure && e.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite) // offsite use outlet reach modelcomponent id
                                        join r in reachBMPExisting on e.ModelComponentId equals r.LocationId
                                        join smrt in db.ScenarioModelResultTypes on $"{e.ScenarioModelResultVariableTypeId}_{2}" equals $"{smrt.ScenarioModelResultVariableTypeId}_{smrt.ModelComponentTypeId}"
                                        join smr in db.ScenarioModelResults.Where(smr => smr.Year >= StartYear && smr.Year <= EndYear && existingOutletReachMcIds.Contains(smr.ModelComponentId) && currScenIds.Contains(smr.ScenarioId)) on $"{e.OutletReachMCId}_{e.ScenarioId}_{e.Year}_{smrt.Id}" equals $"{smr.ModelComponentId}_{smr.ScenarioId}_{smr.Year}_{smr.ScenarioModelResultTypeId}"
                                        select new { e.ModelComponentId, e.BMPTypeId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, SMRValue = smr.Value, ValueChange = e.PercentChange * smr.Value / 100, e.PercentChange }
                                     ).ToList() : null;

            // Get LSD/Parcel id and geometry
            var spatialUnitAvailable = this.GetSpatialUnitBySubAreaMCIdsDbContext(db, availables.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            var spatialUnitExisting = this.GetSpatialUnitBySubAreaMCIdsDbContext(db, existings.Where(o => !o.IsStructure).Select(o => o.ModelComponentId).ToHashSet(), ProjectSpatialUnitTypeId);
            
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
            var spatialUnitAvailableResults = 
                (
                from a in availablesSubAreaSMRValue.Where(x => x.BMPEffectivenessTypeId != 13) // Other than biodiversity
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                group a.ValueChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }
                ).Union
                (
                from a in availablesSubAreaSMRValue.Where(x => x.BMPEffectivenessTypeId == 13) // biodiversity
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                group new { a.ValueChange, sa2su.Area } by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessLocationTypeId, a.ScenarioModelResultVariableTypeId, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x.ValueChange * x.Area) / g.Sum(x => x.Area) }
                )
                .ToList();

            var spatialUnitExistingResults = 
                (
                from e in existingsSubAreaSMRValue.Where(x => x.BMPEffectivenessTypeId != 13) // Other than biodiversity
                join sa2su in subAreaSpatialUnits on e.ModelComponentId equals sa2su.SubAreaMcId
                group e.ValueChange by new { sa2su.SpatialUnitId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, e.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x) }
                ).Union
                (
                from e in existingsSubAreaSMRValue.Where(x => x.BMPEffectivenessTypeId == 13) // biodiversity
                join sa2su in subAreaSpatialUnits on e.ModelComponentId equals sa2su.SubAreaMcId
                group new { e.ValueChange, sa2su.Area } by new { sa2su.SpatialUnitId, e.Year, e.BMPEffectivenessLocationTypeId, e.ScenarioModelResultVariableTypeId, e.BMPEffectivenessTypeId, e.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessLocationTypeId, g.Key.ScenarioModelResultVariableTypeId, g.Key.BMPEffectivenessTypeId, ValueChange = g.Sum(x => x.ValueChange * x.Area) / g.Sum(x => x.Area) }
                )
                .ToList();

            // Aggregate cost to LSD/parcel level results
            var spatialUnitAvailableCost = (
                from a in availables
                join sa2su in subAreaSpatialUnits on a.ModelComponentId equals sa2su.SubAreaMcId
                where a.BMPEffectivenessTypeId == 22 // cost
                group a.PercentChange by new { sa2su.SpatialUnitId, a.Year, a.BMPEffectivenessTypeId, a.BMPTypeId } into g
                select new { g.Key.BMPTypeId, LocationId = g.Key.SpatialUnitId, g.Key.Year, g.Key.BMPEffectivenessTypeId, Cost = g.Sum(x => x) }).ToList();

            var spatialUnitExistingCost = (
                from e in existings
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
                        var dto = new BMPGeometryCostEffectivenessDTO
                        {
                            LocationId = sue.LocationId,
                            FarmId = sue.FarmId,
                            Geometry = sue.Geometry,
                            BMPCombinationTypeId = b.BMPTypeId,
                            BMPCombinationTypeName = b.BMPTypeName,
                            BMPArea = sue.Area,
                            OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.ProjectSpatialUnitTypeEnum.LSD
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
                            var dto = new BMPGeometryCostEffectivenessDTO
                            {
                                LocationId = sua.LocationId,
                                FarmId = sua.FarmId,
                                Geometry = sua.Geometry,
                                BMPCombinationTypeId = b.BMPTypeId,
                                BMPCombinationTypeName = b.BMPTypeName,
                                BMPArea = sua.Area,
                                OptimizationSolutionLocationTypeId = ProjectSpatialUnitTypeId == (int)Enumerators.ProjectSpatialUnitTypeEnum.LSD
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
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianWetland) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RiparianWetlands
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Lake) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Lakes
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.VegetativeFilterStrip) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.VegetativeFilterStrips
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RiparianBuffer) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RiparianBuffers
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.GrassedWaterway) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.GrassedWaterways
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Length * bmp.Width / 10000 }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.FlowDiversion) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.FlowDiversions
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Reservoir) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Reservoirs
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.SmallDam) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.SmallDams
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Wascob) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Wascobs
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ClosedDrain) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.ClosedDrains
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Dugout) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Dugouts
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.CatchBasin) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.CatchBasins
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.Feedlot) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.Feedlots
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.ManureStorage) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.ManureStorages
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = bmp.Area }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.RockChute) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.RockChutes
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = 0.01m }).Distinct()
             .Union(!modelComponentTypeIds.Contains((int)ModelComponentTypeEnum.PointSource) ? new List<ReachBMPGeometry>().AsEnumerable() :
                    from bmp in db.PointSources
                    join r in db.Reaches on bmp.ReachId equals r.Id
                    join sa in db.SubAreas on bmp.SubAreaId equals sa.Id
                    join p in db.Parcels on sa.ParcelId equals p.Id
                    join farm in db.Farms on p.OwnerId equals farm.OwnerId
                    where reachBMPMCIds.Contains(bmp.ModelComponentId)
                    select new ReachBMPGeometry { LocationId = bmp.ModelComponentId, FarmId = farm.Id, Geometry = hasGeometry ? bmp.Geometry : null, ReachMCId = r.ModelComponentId, SubAreaMCId = sa.ModelComponentId, Area = 0.01m }).Distinct()
             .ToList();
        }

        private List<LocationGeometry> GetSpatialUnitBySubAreaMCIdsDbContext(AgBMPToolContext db, HashSet<int> subAreaMCIds, int projectSpatialUnitTypeId)
        {
            if (projectSpatialUnitTypeId == (int)Enumerators.ProjectSpatialUnitTypeEnum.LSD)
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
    }
}
