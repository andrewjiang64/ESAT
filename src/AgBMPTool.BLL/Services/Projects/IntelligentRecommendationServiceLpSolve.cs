using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AgBMPTool.BLL.DLLException;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Models.IntelligentRecommendation;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.DBModel.Model.Scenario;
using AgBMTool.DBL.Interface;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.BLL.Services.Utilities;
using static AgBMPTool.BLL.Services.Utilities.LpSolve;

namespace AgBMPTool.BLL.Services.Projects
{
    public class IntelligentRecommendationServiceLpSolve : IInteligentRecommendationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IProjectSummaryService _pss;
        public readonly IUtilitiesSummaryService _uss;

        public IntelligentRecommendationServiceLpSolve(
            IUnitOfWork _iUnitOfWork,
            IProjectSummaryService _iProjectSummaryService,
            IUtilitiesSummaryService _iUtilitiesSummaryService
            )
        {
            _uow = _iUnitOfWork;
            _pss = _iProjectSummaryService;
            _uss = _iUtilitiesSummaryService;
        }

        public EffectivenessBoundDTO GetConstraintBound(int projectId, int bmpEffectivenessTypeId)
        {
            throw new NotImplementedException();
        }

        public BudgetBoundDTO GetBudgetBound(int projectId)
        {
            throw new NotImplementedException();
        }

        public SolutionDTO GetRecommendedSolution(int projectId, bool isPrintLp)
        {
            var optProject = (from o in _uow.GetRepository<Optimization>().Get()
                              join p in _uow.GetRepository<Project>().Get() on o.ProjectId equals p.Id
                              where p.Id == projectId
                              select new { ProjectId = p.Id, OptimizationId = o.Id, p.StartYear, p.EndYear, o.OptimizationTypeId, o.BudgetTarget, p.ScenarioTypeId, p.ProjectSpatialUnitTypeId })
                            .FirstOrDefault();

            if (optProject != null)
            {
                // Get bmpLocations
                var bmpLocations = _uss.GetSelectedOptimizationBmpLocationsByProjectId(projectId);

                if (bmpLocations.UnitSolutions.Count > 0)
                {
                    // Get SubAreaMCIds
                    var subAreaMCIds = _uss.GetSubAreaMCIds(bmpLocations, optProject.ProjectSpatialUnitTypeId);

                    // Get all reachBMPMCIds
                    var reachBMPMcIds = bmpLocations.OptimizationModelComponents.Select(o => o.LocationId).ToHashSet();

                    // Get all modelcomponentIds including both SubAreas and Reach BMPs                    
                    HashSet<int> modelComponentIds = reachBMPMcIds.Union(subAreaMCIds).ToHashSet();

                    HashSet<int> currScenIds = _uss.GetScenarioIdsBySubAreaMcIds(modelComponentIds, optProject.ScenarioTypeId);

                    // include all BMPs including combinations
                    //int largestBMPId = Int16.MaxValue;

                    List<BMPGeometryCostEffectivenessDTO> bmpEffects = _pss.GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId)
                        .Where(x => x.IsSelectable).ToList();
                    var selected = _uss.GetSelectedOptimizationBmpLocationsByProjectId(projectId);

                    bmpEffects = bmpEffects.Where(b =>
                            !_uss.GetBMPTypeIdsByBMPCombinationTypeId(b.BMPCombinationTypeId).Except(
                                    selected.UnitSolutions.Where(s =>
                                        b.OptimizationSolutionLocationTypeId == (int)s.LocationType && s.LocationId == b.LocationId)
                                    .Select(o => o.BMPTypeId).ToList()
                                ).Any()
                            ).ToList();

                    List<ConstraintInput> constraintInputs;
                    List<ObjectiveConstraintInput> ObjectiveConstraintInputs;
                    LpSolverInput lpSolverInput;
                    List<CostEffectivenessInput> costEffectivenessInputs;
                    List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>();
                    OptimizationObjectiveTypeEnum optObjectiveType;
                    List<ConstraintTarget> constraintTargets;

                    if (optProject.OptimizationTypeId == (int)OptimizationTypeEnum.Budget)
                    {
                        optObjectiveType = OptimizationObjectiveTypeEnum.MaximizeEcoService;
                        constraintInputs = new List<ConstraintInput> { new ConstraintInput { BMPEffectivenessTypeId = 22, Value = (decimal)optProject.BudgetTarget, OptimizationConstraintValueTypeId = OptimizationConstraintValueTypeEnum.AbsoluteValue } };

                        esWeights =
                            (from w in _uow.GetRepository<OptimizationWeights>().Get()
                             where w.OptimizationId == optProject.OptimizationId && w.Weight > 0
                             select new EcoServiceValueWeight { BMPEffectivenessTypeId = w.BMPEffectivenessTypeId, Value = w.Weight }).ToList();

                        decimal sumWeight = esWeights.Select(o => o.Value).Sum();

                        // Normalize weights to 1.0 total
                        esWeights = esWeights.Select(o => { o.Value = o.Value / sumWeight; return o; }).ToList();

                        // Use ES weights to define BMP effectiveness types
                        costEffectivenessInputs = BuildCostEffectivenessInput(bmpEffects, esWeights.Select(o => o.BMPEffectivenessTypeId).ToHashSet());

                        ObjectiveConstraintInputs = BuildObjectiveConstraintInputs(optObjectiveType, costEffectivenessInputs, constraintInputs.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);

                        constraintTargets = BuildConstraintTargetByConstraint(optObjectiveType, null, null, constraintInputs, -999, -999);
                    }
                    else
                    {
                        optObjectiveType = OptimizationObjectiveTypeEnum.MinimizeBudget;
                        constraintInputs =
                            (from c in _uow.GetRepository<OptimizationConstraints>().Get()
                             where c.OptimizationId == optProject.OptimizationId
                             select new ConstraintInput
                             {
                                 BMPEffectivenessTypeId = c.BMPEffectivenessTypeId,
                                 Value = c.Constraint,
                                 OptimizationConstraintValueTypeId = (OptimizationConstraintValueTypeEnum)c.OptimizationConstraintValueTypeId
                             }).ToList();

                        // Use constraint to define BMP effectiveness types
                        costEffectivenessInputs = BuildCostEffectivenessInput(bmpEffects, constraintInputs.Select(o => o.BMPEffectivenessTypeId).ToHashSet());

                        ObjectiveConstraintInputs = BuildObjectiveConstraintInputs(optObjectiveType, costEffectivenessInputs, constraintInputs.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);

                        constraintTargets = BuildConstraintTargetByConstraint(optObjectiveType,
                            subAreaMCIds.Union(reachBMPMcIds).Distinct().ToHashSet(),
                            currScenIds, constraintInputs, optProject.StartYear, optProject.EndYear);
                    }

                    ObjectiveConstraintInputs = BuildObjectiveConstraintInputs(optObjectiveType, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
                    lpSolverInput = BuildOptimizationLpSolverInputDTO(ObjectiveConstraintInputs, constraintTargets, _uow.GetRepository<BMPCombinationType>().Get().ToDictionary(t => t.Id, t => t.Name));

                    return RunOptimizationUsingLpSolve(projectId, optObjectiveType, lpSolverInput, isPrintLp);
                }
                else
                {
                    return new SolutionDTO { ProjectId = projectId };
                }
            }
            else
            {
                throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Project id {projectId} not found in IntelligentRecommendation table!");
            }
        }

        private List<CostEffectivenessInput> BuildCostEffectivenessInput(List<BMPGeometryCostEffectivenessDTO> bmpEffects, HashSet<int> bmpEffectivenessIds)
        {
            return
                (from be in bmpEffects
                 select new CostEffectivenessInput
                 {
                     BMPCombinationTypeId = be.BMPCombinationTypeId,
                     Cost = be.Cost,
                     LocationId = be.LocationId,
                     LocationType = (OptimizationSolutionLocationTypeEnum)be.OptimizationSolutionLocationTypeId,
                     EffectivessValues = be.EffectivenessDTOs
                         .Where(x => bmpEffectivenessIds.Contains(x.BMPEffectivenessTypeId))
                         .Select(o => new UnitEffectiveness { BMPEffectivenessTypeId = o.BMPEffectivenessTypeId, Value = o.EffectivenessValue }).ToList()
                 }).ToList();
        }

        public SolutionDTO RunOptimizationUsingLpSolve(int projectId, OptimizationObjectiveTypeEnum optObjective, LpSolverInput inputDTO, bool isPrintLp)
        {
            var start = DateTime.Now;

            string NewLine = "\n";

            /* MakeLp */
            IntPtr lp = LpSolve.make_lp(0, inputDTO.ColNumber);

            /* Now redirect all output to a file */
            LpSolve.set_outputfile(lp, "LpSolveLog.txt");

            #region Setup input
            /* SetObjFn */
            if (isPrintLp) LpSolve.print_str(lp, "SetObjFn" + NewLine);
            LpSolve.set_obj_fn(lp, inputDTO.ObjectiveFnCoefficients.ToArray());
            if (isPrintLp) LpSolve.print_lp(lp);

            /* SetBinary */
            if (isPrintLp) LpSolve.print_str(lp, "SetObjFn" + NewLine);
            for (int col = 0; col < inputDTO.ColNumber; col++)
            {
                /* LP Column base 1 */
                LpSolve.set_binary(lp, col + 1, Convert.ToByte(true));
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* SetMax/Min */
            if (isPrintLp) LpSolve.print_str(lp, "SetMax/Min" + NewLine);
            if (optObjective == OptimizationObjectiveTypeEnum.MinimizeBudget)
            {
                LpSolve.set_minim(lp);
            }
            else
            {
                LpSolve.set_maxim(lp);
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* Add equal constraint */
            if (isPrintLp) LpSolve.print_str(lp, "Add equal constraint" + NewLine);
            foreach (var cons in inputDTO.EqualConstraints)
            {
                LpSolve.add_constraint(lp, cons.LHS.ToArray(), cons.Type, cons.RHS);
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* Add less or equal constraint */
            if (isPrintLp) LpSolve.print_str(lp, "Add less or equal constraint" + NewLine);
            foreach (var cons in inputDTO.LessOrEqualConstraints)
            {
                LpSolve.add_constraint(lp, cons.LHS.ToArray(), cons.Type, cons.RHS);
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* Add more or equal constraint */
            if (isPrintLp) LpSolve.print_str(lp, "Add more or equal constraint" + NewLine);
            foreach (var cons in inputDTO.GreaterOrEqualConstraints)
            {
                LpSolve.add_constraint(lp, cons.LHS.ToArray(), cons.Type, cons.RHS);
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* SetColNames */
            if (isPrintLp) LpSolve.print_str(lp, "SetColNames" + NewLine);
            for (int col = 1; col < inputDTO.Columns.Count; col++)
            {
                /* LP Column base 1 */
                LpSolve.set_col_name(lp, col, inputDTO.Columns[col].ColumnName);
            }
            if (isPrintLp) LpSolve.print_lp(lp);

            /* SetRowNames */
            if (isPrintLp) LpSolve.print_str(lp, "SetRowNames" + NewLine);
            for (int i = 0; i < inputDTO.Rows.Count; i++)
            {
                LpSolve.set_row_name(lp, i, inputDTO.Rows[i].RowName);
            }
            if (isPrintLp) LpSolve.print_lp(lp);
            #endregion

            /* Solve lp */
            if (isPrintLp) LpSolve.print_str(lp, "Solve lp" + NewLine);
            LpSolve.solve(lp);
            if (isPrintLp) LpSolve.print_lp(lp);

            #region Get output
            /* GetObjective */
            if (isPrintLp) LpSolve.print_str(lp, "GetObjective: ");
            double objFnValue = LpSolve.get_objective(lp);
            if (isPrintLp) LpSolve.print_str(lp, objFnValue + NewLine);

            bool flag;
            /* GetVariables */
            double[] decisionVarsValues = new double[LpSolve.get_Ncolumns(lp)];
            flag = Convert.ToBoolean(LpSolve.get_variables(lp, decisionVarsValues));
            if (isPrintLp) LpSolve.print_str(lp, "GetDecisionVariables: ");
            if (isPrintLp) LpSolve.print_str(lp, $"{string.Join(" ", decisionVarsValues)}" + NewLine);

            if (!flag)
            {
                throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"IntelligentRecommendation get variables error!");
            }

            /* GetPtrConstraints */
            double[] lHSConstraints = new double[LpSolve.get_Nrows(lp)];
            flag = Convert.ToBoolean(LpSolve.get_constraints(lp, lHSConstraints));
            if (isPrintLp) LpSolve.print_str(lp, "GetPtrConstraints: ");
            if (isPrintLp) LpSolve.print_str(lp, $"{string.Join(" ", lHSConstraints)}" + NewLine);

            if (!flag)
            {
                throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"IntelligentRecommendation get variables error!");
            }

            /* GetConstrType */
            lpsolve_constr_types[] constraintTypes = new lpsolve_constr_types[lHSConstraints.Length];
            for (int row = 0; row < constraintTypes.Length; row++)
            {
                /* LP Row base 1 */
                constraintTypes[row] = LpSolve.get_constr_type(lp, row + 1);
            }
            if (isPrintLp) LpSolve.print_str(lp, "GetConstrType: ");
            if (isPrintLp) LpSolve.print_str(lp, $"{string.Join(" ", constraintTypes)}" + NewLine);

            /* GetRh */
            if (isPrintLp) LpSolve.print_str(lp, "GetRh" + NewLine);
            double[] rhsConstraints = new double[lHSConstraints.Length];
            for (int row = 0; row < constraintTypes.Length; row++)
            {
                /* LP Row base 1 */
                rhsConstraints[row] = LpSolve.get_rh(lp, row + 1);
            }
            if (isPrintLp) LpSolve.print_str(lp, $"{string.Join(" ", rhsConstraints)}" + NewLine);

            /* GetMat */
            if (isPrintLp) LpSolve.print_str(lp, "GetMat" + NewLine);
            int nRow = LpSolve.get_Nrows(lp) + 1;
            int nCol = LpSolve.get_Ncolumns(lp);

            double[,] lPSolveMatrix = new double[nRow, nCol];
            for (int row = 0; row < nRow; row++)
            {
                for (int col = 0; col < nCol; col++)
                {
                    lPSolveMatrix[row, col] = LpSolve.get_mat(lp, row, 1 + col);
                }
            }

            Console.WriteLine("----------------- lPSolveMatrix ------------------");
            int rowLength = lPSolveMatrix.GetLength(0);
            int colLength = lPSolveMatrix.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write(string.Format("{0} ", lPSolveMatrix[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }

            Console.WriteLine("----------------- IntelligentRecommendation results ------------------");
            #endregion

            /* DeleteLp */
            if (isPrintLp) LpSolve.print_str(lp, "DeleteLp" + NewLine);
            if (isPrintLp) LpSolve.print_lp(lp);
            LpSolve.print_str(lp, $"Total LP run time: {(DateTime.Now - start).TotalSeconds} seconds ...");
            LpSolve.delete_lp(lp);

            /* Export results */
            Console.WriteLine("----------------- IntelligentRecommendation results ------------------");

            Console.WriteLine($"Objective: {objFnValue}");
            Console.WriteLine($"Constraint: [{string.Join(", ", lHSConstraints)}]");

            Console.WriteLine("----------------- BMP recommendation ------------------");

            var UnitSolutions = new List<UnitSolution>();
            for (int col = 0; col < nCol - 1; col++)
            {
                if (decisionVarsValues[col] > 0)
                {
                    string bmp = inputDTO.Columns[col + 1].BMPCombinationTypeName;
                    int locationId = inputDTO.Columns[col + 1].LocationId;

                    Console.WriteLine($"BMP: {bmp}; Location: {locationId}");

                    foreach (int bmpTypeId in _uss.GetBMPTypeIdsByBMPCombinationTypeId(inputDTO.Columns[col + 1].BMPCombinationTypeId))
                    {
                        UnitSolutions.Add(new UnitSolution
                        {
                            BMPTypeId = bmpTypeId,
                            LocationId = locationId,
                            LocationType = inputDTO.Columns[col + 1].LocationType
                        });
                    }
                }
            }

            return new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = UnitSolutions
            };
        }

        private List<ConstraintTarget> BuildConstraintTargetByConstraint(OptimizationObjectiveTypeEnum objectiveType, HashSet<int> allSubAreaReachMcIds, HashSet<int> currentScenIds, List<ConstraintInput> constraintInputs, int startYear, int endYear)
        {
            if (objectiveType == OptimizationObjectiveTypeEnum.MaximizeEcoService) /* Budget mode always use absolute value */
            {
                return (
                    from c in constraintInputs
                    join b in _uow.GetRepository<BMPEffectivenessType>().Get() on c.BMPEffectivenessTypeId equals b.Id
                    select new ConstraintTarget
                    {
                        BMPEffectivenessTypeId = c.BMPEffectivenessTypeId,
                        UpperValue = c.Value,
                        LowerValue = 0
                    }).ToList();
            }
            else
            {
                if (constraintInputs.Select(o => o.OptimizationConstraintValueTypeId).Contains(OptimizationConstraintValueTypeEnum.Percent))
                {
                    var smrtIds =
                        (from betId in constraintInputs.Where(x => x.OptimizationConstraintValueTypeId == OptimizationConstraintValueTypeEnum.Percent).Select(o => o.BMPEffectivenessTypeId)
                         join bet in _uow.GetRepository<BMPEffectivenessType>().Get() on betId equals bet.Id
                         select bet.ScenarioModelResultTypeId).ToHashSet();

                    List<ScenarioModelResultDTO> scenarioModelResultDTOs = BuildScenarioModelResultDTOs(allSubAreaReachMcIds, currentScenIds, smrtIds, startYear, endYear);

                    /* Onsite constraints input as percentage convert to absolute value */
                    var onsitePtoV = (
                         from smr in scenarioModelResultDTOs.SelectMany(o => o.OnsiteScenarioModelResults)
                         join b in _uow.GetRepository<BMPEffectivenessType>().Get() on smr.ScenarioModelResultTypeId equals b.ScenarioModelResultTypeId
                         join c in constraintInputs on b.Id equals c.BMPEffectivenessTypeId
                         where c.OptimizationConstraintValueTypeId == OptimizationConstraintValueTypeEnum.Percent && b.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Onsite
                         group smr.Value by new { c.BMPEffectivenessTypeId, ConstraintPercent = c.Value, b.UserEditableConstraintBoundTypeId, b.UserNotEditableConstraintBoundValue } into g
                         select new ConstraintTarget
                         {
                             BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                             UpperValue = g.Key.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.GreaterThan ?
                                             decimal.MaxValue
                                            : g.Sum(o => o) * g.Key.ConstraintPercent / 100,
                             LowerValue = g.Key.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.LessThan ?
                                             decimal.MinValue
                                            : g.Sum(o => o) * g.Key.ConstraintPercent / 100
                         }
                         ).ToList();

                    /* Offsite constraints input as percentage convert to absolute value */
                    var offsitePtoV = (
                         from smr in scenarioModelResultDTOs.SelectMany(o => o.OffsiteScenarioModelResults)
                         join b in _uow.GetRepository<BMPEffectivenessType>().Get() on smr.ScenarioModelResultTypeId equals b.ScenarioModelResultTypeId
                         join c in constraintInputs on b.Id equals c.BMPEffectivenessTypeId
                         where c.OptimizationConstraintValueTypeId == OptimizationConstraintValueTypeEnum.Percent && b.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite
                         group smr.Value by new { c.BMPEffectivenessTypeId, ConstraintPercent = c.Value, b.UserEditableConstraintBoundTypeId, b.UserNotEditableConstraintBoundValue } into g
                         select new ConstraintTarget
                         {
                             BMPEffectivenessTypeId = g.Key.BMPEffectivenessTypeId,
                             UpperValue = g.Key.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.GreaterThan ?
                                             decimal.MaxValue
                                            : g.Average(o => o) * g.Key.ConstraintPercent / 100,
                             LowerValue = g.Key.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.LessThan ?
                                             decimal.MinValue
                                            : g.Average(o => o) * g.Key.ConstraintPercent / 100
                         }
                         ).ToList();

                    /* Constraints input as absolute value */
                    var absoluteConstraints = (from c in constraintInputs.Where(x => x.OptimizationConstraintValueTypeId == OptimizationConstraintValueTypeEnum.AbsoluteValue)
                                               join b in _uow.GetRepository<BMPEffectivenessType>().Get() on c.BMPEffectivenessTypeId equals b.Id
                                               select new ConstraintTarget
                                               {
                                                   BMPEffectivenessTypeId = c.BMPEffectivenessTypeId,
                                                   UpperValue = b.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.GreaterThan ? decimal.MaxValue : c.Value,
                                                   LowerValue = b.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.LessThan ? decimal.MinValue : c.Value
                                               }).ToList();

                    /* Union all and return */
                    return onsitePtoV.Union(offsitePtoV).Union(absoluteConstraints).ToList();
                }
                else
                {
                    return (
                        from c in constraintInputs
                        join b in _uow.GetRepository<BMPEffectivenessType>().Get() on c.BMPEffectivenessTypeId equals b.Id
                        select new ConstraintTarget
                        {
                            BMPEffectivenessTypeId = c.BMPEffectivenessTypeId,
                            UpperValue = b.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.GreaterThan ? decimal.MaxValue : c.Value,
                            LowerValue = b.UserEditableConstraintBoundTypeId == (int)OptimizationConstraintBoundTypeEnum.LessThan ? decimal.MinValue : c.Value
                        }).ToList();
                }
            }
        }

        private List<ScenarioModelResultDTO> BuildScenarioModelResultDTOs(HashSet<int> allSubAreaReachMcIds, HashSet<int> currentScenIds, HashSet<int?> smrtIds, int startYear, int endYear)
        {
            var onsites = (from smr in _uow.GetRepository<ScenarioModelResult>().Query()
                           join b in _uow.GetRepository<BMPEffectivenessType>().Query() on smr.ScenarioModelResultTypeId equals b.ScenarioModelResultTypeId
                           join mc in _uow.GetRepository<ModelComponent>().Query() on smr.ModelComponentId equals mc.Id
                           join w in _uow.GetRepository<Watershed>().Query() on mc.WatershedId equals w.Id
                           join r in _uow.GetRepository<Reach>().Query() on w.OutletReachId equals r.Id
                           where allSubAreaReachMcIds.Contains(smr.ModelComponentId) && currentScenIds.Contains(smr.ScenarioId) && smr.Year >= startYear && smr.Year <= endYear
                           group smr.Value by new { OnsiteMcId = smr.ModelComponentId, smr.ScenarioModelResultTypeId, OffsiteMcId = r.ModelComponentId } into g
                           select new { g.Key.OnsiteMcId, g.Key.OffsiteMcId, g.Key.ScenarioModelResultTypeId, Value = g.Average(o => o) }).ToList();

            var outletMcIds = onsites.Select(o => o.OffsiteMcId).ToHashSet();

            var offsites = (from smr in _uow.GetRepository<ScenarioModelResult>().Query()
                            join b in _uow.GetRepository<BMPEffectivenessType>().Query() on smr.ScenarioModelResultTypeId equals b.ScenarioModelResultTypeId
                            where outletMcIds.Contains(smr.ModelComponentId) && currentScenIds.Contains(smr.ScenarioId) && smr.Year >= startYear && smr.Year <= endYear && b.BMPEffectivenessLocationTypeId == (int)BMPEffectivenessLocationTypeEnum.Offsite
                            group smr.Value by new { OffsiteMcId = smr.ModelComponentId, smr.ScenarioModelResultTypeId } into g
                            select new { g.Key.OffsiteMcId, g.Key.ScenarioModelResultTypeId, Value = g.Average(o => o) }).ToList();

            var res = new List<ScenarioModelResultDTO>();

            foreach (var onOffIds in onsites.Select(o => new { o.OnsiteMcId, o.OffsiteMcId }).Distinct())
            {
                var smrDTO = new ScenarioModelResultDTO
                {
                    OnsiteModelComponentId = onOffIds.OnsiteMcId,
                    OffsiteModelComponentId = onOffIds.OffsiteMcId
                };

                smrDTO.OnsiteScenarioModelResults = onsites.Where(x => x.OnsiteMcId == onOffIds.OnsiteMcId)
                    .Select(o => new UnitScenarioModelResult
                    {
                        ScenarioModelResultTypeId = o.ScenarioModelResultTypeId,
                        Value = o.Value
                    }).ToList();

                smrDTO.OffsiteScenarioModelResults = offsites.Where(x => x.OffsiteMcId == onOffIds.OffsiteMcId)
                    .Select(o => new UnitScenarioModelResult
                    {
                        ScenarioModelResultTypeId = o.ScenarioModelResultTypeId,
                        Value = o.Value
                    }).ToList();

                res.Add(smrDTO);
            }

            return res;
        }

        public List<ObjectiveConstraintInput> BuildObjectiveConstraintInputs(OptimizationObjectiveTypeEnum ObjectiveType, List<CostEffectivenessInput> inputs, HashSet<int> constraintBMPEffectivenessIds, List<EcoServiceValueWeight> weights)
        {
            if (ObjectiveType == OptimizationObjectiveTypeEnum.MaximizeEcoService)
            {
                return (from i in inputs
                        join ni in NormalizeEffectivenessValues(inputs) on $"{i.BMPCombinationTypeId}_{i.LocationId}" equals $"{ni.BMPCombinationTypeId}_{ni.LocationId}"
                        select new ObjectiveConstraintInput
                        {
                            BMPCombinationTypeId = i.BMPCombinationTypeId,
                            ConstraintValues = new List<UnitEffectiveness> { new UnitEffectiveness { BMPEffectivenessTypeId = 22, Value = i.Cost } },
                            LocationId = i.LocationId,
                            LocationType = i.LocationType,
                            ObjectiveValue = GetEcoServiceValue(ni.EffectivessValues, weights)
                        }).ToList();
            }
            else if (ObjectiveType == OptimizationObjectiveTypeEnum.MinimizeBudget)
            {
                return (from i in inputs
                        select new ObjectiveConstraintInput
                        {
                            BMPCombinationTypeId = i.BMPCombinationTypeId,
                            ConstraintValues = new List<UnitEffectiveness>(i.EffectivessValues.FindAll(x => constraintBMPEffectivenessIds.Contains(x.BMPEffectivenessTypeId))),
                            LocationId = i.LocationId,
                            LocationType = i.LocationType,
                            ObjectiveValue = i.Cost
                        }).ToList();
            }

            throw new MainException(new System.Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"IntelligentRecommendation mode error!");
        }

        private List<CostEffectivenessInput> NormalizeEffectivenessValues(List<CostEffectivenessInput> inputs)
        {
            List<CostEffectivenessInput> res = new List<CostEffectivenessInput>();

            foreach (var r in inputs)
            {
                CostEffectivenessInput cei = new CostEffectivenessInput
                {
                    BMPCombinationTypeId = r.BMPCombinationTypeId,
                    Cost = r.Cost,
                    LocationId = r.LocationId,
                    LocationType = r.LocationType
                };
                foreach (var e in r.EffectivessValues)
                {
                    cei.EffectivessValues.Add(new UnitEffectiveness
                    {
                        BMPEffectivenessTypeId = e.BMPEffectivenessTypeId,
                        Value = e.Value / r.Cost
                    });
                }
                res.Add(cei);
            }

            // Find max, min values and upper/lower bound type in each BMPEffectivenessTypeId
            var normalizeVariable =
                (from betId in res.SelectMany(o => o.EffectivessValues.Select(oo => oo.BMPEffectivenessTypeId)) // Find all BMPEffectivenessTypeIds in inputs
                 join eff in res.SelectMany(o => o.EffectivessValues) on betId equals eff.BMPEffectivenessTypeId
                 join bet in _uow.GetRepository<BMPEffectivenessType>().Get() on betId equals bet.Id
                 group eff.Value by new { betId, bet.UserEditableConstraintBoundTypeId } into g
                 select new { Max = g.Max(o => o), Min = g.Min(o => o), BMPEffectivenessTypeId = g.Key.betId, Bound = g.Key.UserEditableConstraintBoundTypeId })
                .ToDictionary(t => t.BMPEffectivenessTypeId, t => new { t.Max, t.Min, t.Bound });

            /* Normalize inputs EffectivenessValues using 
             * Eq. for upper bound : (Value - Max)/(Max - Min)
             * Eq. for lower bound : (Value - Min)/(Min - Max) 
             */
            foreach (int betId in normalizeVariable.Keys)
            {
                if (normalizeVariable[betId].Max > normalizeVariable[betId].Min)
                {
                    if (normalizeVariable[betId].Bound
                       == (int)OptimizationConstraintBoundTypeEnum.GreaterThan)
                    {
                        foreach (var eff in res.SelectMany(o => o.EffectivessValues).Where(x => x.BMPEffectivenessTypeId == betId))
                        {
                            eff.Value = (eff.Value - normalizeVariable[eff.BMPEffectivenessTypeId].Min)
                               / (normalizeVariable[eff.BMPEffectivenessTypeId].Max - normalizeVariable[eff.BMPEffectivenessTypeId].Min) * 100;
                        }
                    }
                    else
                    {
                        foreach (var eff in res.SelectMany(o => o.EffectivessValues).Where(x => x.BMPEffectivenessTypeId == betId))
                        {
                            eff.Value = (eff.Value - normalizeVariable[eff.BMPEffectivenessTypeId].Max)
                                / (normalizeVariable[eff.BMPEffectivenessTypeId].Min - normalizeVariable[eff.BMPEffectivenessTypeId].Max) * 100;
                        }
                    }
                }
                else
                {
                    foreach (var eff in res.SelectMany(o => o.EffectivessValues).Where(x => x.BMPEffectivenessTypeId == betId))
                    {
                        eff.Value = 100;
                    }
                }
            }

            return res;
        }

        private decimal GetEcoServiceValue(List<UnitEffectiveness> normalizedValues, List<EcoServiceValueWeight> weights)
        {
            return (from nv in normalizedValues
                    join w in weights on nv.BMPEffectivenessTypeId equals w.BMPEffectivenessTypeId
                    select nv.Value * w.Value).Sum();
        }

        public LpSolverInput BuildOptimizationLpSolverInputDTO(List<ObjectiveConstraintInput> inputDTOs, List<ConstraintTarget> constraintTargets, Dictionary<int, string> bmpCombinationTypes)
        {
            var res = new LpSolverInput();

            /* Build ObjectiveFnCoefficients and Columns */
            /* Add No BMP column */
            res.Columns.Add(new LpSolverInput.Column());

            foreach (var locTypeAndId in inputDTOs.Select(o => new { o.LocationType, o.LocationId }).Distinct())
            {
                var locTypeIdBMPs = inputDTOs.FindAll(x => x.LocationType == locTypeAndId.LocationType && x.LocationId == locTypeAndId.LocationId);

                if (locTypeIdBMPs.Count > 0)
                {
                    if (locTypeAndId.LocationType == OptimizationSolutionLocationTypeEnum.ReachBMP)
                    {

                        /* Add all BMPs */
                        foreach (var inputDTO in locTypeIdBMPs)
                        {
                            /* Add BMP not selected first */
                            res.Columns.Add(new LpSolverInput.Column
                            {
                                LocationId = inputDTO.LocationId,
                                BMPCombinationTypeId = 0,
                                BMPCombinationTypeName = bmpCombinationTypes.ContainsKey(inputDTO.BMPCombinationTypeId) ? $"{bmpCombinationTypes[inputDTO.BMPCombinationTypeId]}_NotSelect" : "NoBMP",
                                LocationType = inputDTO.LocationType
                            });
                            res.ObjectiveFnCoefficients.Add((double)0);

                            /* Add BMP selected */
                            res.Columns.Add(new LpSolverInput.Column
                            {
                                LocationId = inputDTO.LocationId,
                                BMPCombinationTypeId = inputDTO.BMPCombinationTypeId,
                                BMPCombinationTypeName = bmpCombinationTypes.ContainsKey(inputDTO.BMPCombinationTypeId) ? bmpCombinationTypes[inputDTO.BMPCombinationTypeId] : "NoBMP",
                                LocationType = inputDTO.LocationType
                            });
                            res.ObjectiveFnCoefficients.Add((double)inputDTO.ObjectiveValue);
                        }
                    }
                    else
                    {
                        /* Add no BMP first */
                        res.Columns.Add(new LpSolverInput.Column
                        {
                            LocationId = locTypeAndId.LocationId,
                            BMPCombinationTypeId = 0,
                            BMPCombinationTypeName = "NoBMP",
                            LocationType = locTypeAndId.LocationType
                        });
                        res.ObjectiveFnCoefficients.Add(0);

                        /* Add all BMPs and their combinations */
                        foreach (var inputDTO in locTypeIdBMPs)
                        {
                            res.Columns.Add(new LpSolverInput.Column
                            {
                                LocationId = inputDTO.LocationId,
                                BMPCombinationTypeId = inputDTO.BMPCombinationTypeId,
                                BMPCombinationTypeName = bmpCombinationTypes.ContainsKey(inputDTO.BMPCombinationTypeId) ? bmpCombinationTypes[inputDTO.BMPCombinationTypeId] : "NoBMP",
                                LocationType = inputDTO.LocationType
                            });
                            res.ObjectiveFnCoefficients.Add((double)inputDTO.ObjectiveValue);
                        }
                    }
                }
            }

            /* Build ColNumber */
            res.ColNumber = res.Columns.Count;

            /* First row has name 0 */
            res.Rows.Add(new LpSolverInput.Row());

            /* Build Rows */
            foreach (int lsdId in inputDTOs.FindAll(x => x.LocationType == OptimizationSolutionLocationTypeEnum.LegalSubDivision).Select(o => o.LocationId).Distinct())
            {
                res.Rows.Add(new LpSolverInput.Row
                {
                    BMPTypeName = "LegalSubDivision",
                    LocationId = lsdId,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                });
            }

            foreach (int parcelId in inputDTOs.FindAll(x => x.LocationType == OptimizationSolutionLocationTypeEnum.Parcel).Select(o => o.LocationId).Distinct())
            {
                res.Rows.Add(new LpSolverInput.Row
                {
                    BMPTypeName = "Parcel",
                    LocationId = parcelId,
                    LocationType = OptimizationSolutionLocationTypeEnum.Parcel
                });
            }

            foreach (var reachBMP in inputDTOs.FindAll(x => x.LocationType == OptimizationSolutionLocationTypeEnum.ReachBMP).Select(o => new { o.BMPCombinationTypeId, o.LocationId }).Distinct())
            {
                res.Rows.Add(new LpSolverInput.Row
                {
                    BMPTypeName = bmpCombinationTypes[reachBMP.BMPCombinationTypeId],
                    LocationId = reachBMP.LocationId,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                });
            }

            /* Build Equal constraint for BMP selection */
            foreach (var row in res.Rows.GetRange(1, res.Rows.Count - 1))
            {
                LpSolverInput.Constraint c = new LpSolverInput.Constraint
                {
                    Type = lpsolve_constr_types.EQ,
                    RHS = 1.0,
                    LHS = new List<double> { 0 }
                };

                for (int col = 1; col < res.Columns.Count; col++)
                {
                    if (row.LocationType == res.Columns[col].LocationType && row.LocationId == res.Columns[col].LocationId)
                    {
                        c.LHS.Add(1.0);
                    }
                    else
                    {
                        c.LHS.Add(0.0);
                    }
                }

                res.EqualConstraints.Add(c);
            }

            /* Build Less or equal constraint cLE */
            /* Build More or equal constraint cGE */
            foreach (var ct in constraintTargets)
            {
                LpSolverInput.Constraint cLE = new LpSolverInput.Constraint
                {
                    Type = lpsolve_constr_types.LE,
                    RHS = (double)ct.UpperValue,
                    LHS = new List<double> { 0 }
                };

                LpSolverInput.Constraint cGE = new LpSolverInput.Constraint
                {
                    Type = lpsolve_constr_types.GE,
                    RHS = (double)ct.LowerValue,
                    LHS = new List<double> { 0 }
                };

                // start from col 1 to match LpSolve base 1
                for (int col = 1; col < res.Columns.Count; col++)
                {
                    var v = inputDTOs.Find(x => x.LocationId == res.Columns[col].LocationId
                            && x.BMPCombinationTypeId == res.Columns[col].BMPCombinationTypeId
                            && x.LocationType == res.Columns[col].LocationType);

                    double value =
                        v == null ? 0.0 :
                        (double)(v.ConstraintValues.Find(x => x.BMPEffectivenessTypeId == ct.BMPEffectivenessTypeId)?
                        .Value ?? 0.0m);

                    cLE.LHS.Add(value);
                    cGE.LHS.Add(value);
                }

                res.LessOrEqualConstraints.Add(cLE);

                res.GreaterOrEqualConstraints.Add(cGE);
            }

            return res;
        }

        public bool BuildRecommendedSolution(int projectId, bool isPrintLp)
        {
            /* Get recommended solution first */
            SolutionDTO solution = this.GetRecommendedSolution(projectId, isPrintLp);

            /* If solution unit solution list is not empty */
            return _uss.SaveSolution(solution, true) > 0;
        }

        public bool SaveOptimizationSettings(int projectId, OptimizationSettingDTO settingDTO)
        {
            /* Find optimization id */
            var opt = _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId).FirstOrDefault();

            /* If find */
            if (opt != null)
            {
                /* Update optimization type */
                _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId).Select(o =>
                {
                    o.OptimizationTypeId = (int)settingDTO.OptimizationType;
                    return o;
                }).ToList();

                if (settingDTO.OptimizationType == OptimizationTypeEnum.EcoService)/* If Eco-Service mode */
                {
                    /* Remove existing constraints*/
                    foreach (var c in _uow.GetRepository<OptimizationConstraints>().Get(x => x.OptimizationId == opt.Id))
                    {
                        _uow.GetRepository<OptimizationConstraints>().Delete(c);
                    }

                    /* Save constraints */
                    foreach (var c in settingDTO.Constraints)
                    {
                        _uow.GetRepository<OptimizationConstraints>().Add(new OptimizationConstraints
                        {
                            BMPEffectivenessTypeId = c.BMPEffectivenessTypeId,
                            OptimizationId = opt.Id,
                            OptimizationConstraintValueTypeId = (int)c.OptimizationConstraintValueTypeId,
                            Constraint = c.Value
                        });
                    }


                }
                else /* Else Budget mode */
                {
                    /* Update budget target */
                    _uow.GetRepository<Optimization>().Get(x => x.ProjectId == projectId).Select(o =>
                    {
                        o.BudgetTarget = settingDTO.BudgetTarget;
                        return o;
                    }).ToList();

                    /* Update ES value weights */
                    _uow.GetRepository<OptimizationWeights>().Get(x => x.OptimizationId == opt.Id).Select(o =>
                    {
                        o.Weight = Convert.ToInt16(settingDTO.EsWeights.Find(x => x.BMPEffectivenessTypeId == o.BMPEffectivenessTypeId)?.Value ?? 0);
                        return o;
                    }).ToList();
                }

                /* Update optimization */
                //_uow.GetRepository<Optimization>().Update(opt);

                /* Save database */
                _uow.Commit();

                /* return true */
                return true;
            }
            else /* If not find return false */
            {
                return false;
            }
        }
    }
}
