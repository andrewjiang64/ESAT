using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Models.IntelligentRecommendation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using System.IO;
using AgBMPTool.DBModel.Model.Optimization;
using AgBMPTool.BLL.Services.Projects;

namespace AgBMPTool.BLL.TestDB.Test
{
    class ManualInput_IntelligentRecommendationService_UTest
    {
        [Test]
        public void MASBx3_ISWETx3_BudgetMode3000_BuildObjectiveConstraintInputs()
        {
            // Assign
            OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MaximizeEcoService;
            decimal expectObjValue = 245.34733m;
            decimal expectConsValue = 6700m;

            // Act
            List<ObjectiveConstraintInput> res = ServicesTestDB.IRS.BuildObjectiveConstraintInputs(optMode, this.demoMASBx3_ISWETx3_CostEffectivenessInputs, null, this.demoEsoServiceValueWeights);

            // Assert
            Assert.Zero((int)(1000 * (expectObjValue - res.Select(o => o.ObjectiveValue).Sum()))); // difference less than +-0.001 
            Assert.Zero((int)(1000 * (expectConsValue - res.SelectMany(o => o.ConstraintValues.Select(oo => oo.Value)).Sum()))); // difference less than +-0.001 
        }

        [Test]
        public void MASBx3_ISWETx3_BudgetMode3000_RunOptimizationUsingLpSolve()
        {
            // Assign
            IntelligentRecommendationServiceLpSolve solver = ServicesTestDB.IRS;
            int projectId = 1;
            OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MaximizeEcoService;
            List<ConstraintTarget> constraintTargets = new List<ConstraintTarget>(demoBudgetTarget3000);
            List<CostEffectivenessInput> costEffectivenessInputs = new List<CostEffectivenessInput>(demoMASBx3_ISWETx3_CostEffectivenessInputs);
            List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>(demoEsoServiceValueWeights);
            List<ObjectiveConstraintInput> inputDTOs = ServicesTestDB.IRS.BuildObjectiveConstraintInputs(optMode, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
            Dictionary<int, string> bmpCombinationTypes = demoBMPCombinationTypes;
            LpSolverInput input = ServicesTestDB.IRS.BuildOptimizationLpSolverInputDTO(inputDTOs, constraintTargets, bmpCombinationTypes);

            SolutionDTO expectRes = new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 1,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    }
                }
            };

            // Act
            SolutionDTO res = ServicesTestDB.IRS.RunOptimizationUsingLpSolve(projectId, optMode, input, true);

            // Assert
            Assert.IsTrue(expectRes.Equals(res));
        }

        // Disable due to added BMP 41 AOPANoMASB
        //[Test]
        //public void MASBx3_MI48Hx3_ISWETx3_BudgetMode3000_RunOptimizationUsingLpSolve()
        //{
        //    // Assign
        //    int projectId = 1;
        //    OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MaximizeEcoService;
        //    List<ConstraintTarget> constraintTargets = demoBudgetTarget3000;
        //    Dictionary<int, string> bmpCombinationTypes = demoBMPCombinationTypes;
        //    List<CostEffectivenessInput> costEffectivenessInputs = new List<CostEffectivenessInput>(this.demoMASBx3_MI48Hx3_ISWETx3_CostEffectivenessInputs);
        //    List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>(demoEsoServiceValueWeights);
        //    List<ObjectiveConstraintInput> inputDTOs = Services.IntelligentServiceSingleton.BuildObjectiveConstraintInputs(optMode, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
        //    LpSolverInput input = Services.IntelligentServiceSingleton.BuildOptimizationLpSolverInputDTO(inputDTOs, constraintTargets, bmpCombinationTypes);

        //    SolutionDTO expectRes = new SolutionDTO
        //    {
        //        ProjectId = projectId,
        //        UnitSolutions = new List<UnitSolution> {
        //            new UnitSolution{
        //                BMPTypeId = 18,
        //                LocationId = 1,
        //                LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
        //            },
        //            new UnitSolution{
        //                BMPTypeId = 19,
        //                LocationId = 1,
        //                LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
        //            },
        //            new UnitSolution{
        //                BMPTypeId = 18,
        //                LocationId = 3,
        //                LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
        //            },
        //            new UnitSolution{
        //                BMPTypeId = 18,
        //                LocationId = 4,
        //                LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
        //            },
        //            new UnitSolution{
        //                BMPTypeId = 1,
        //                LocationId = 3,
        //                LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
        //            }
        //        }
        //    };

        //    // Act
        //    SolutionDTO res = Services.IntelligentServiceSingleton.RunOptimizationUsingLpSolve(projectId, optMode, input);

        //    // Assert
        //    Assert.IsTrue(expectRes.Equals(res));
        //}

        [Test]
        public void MASBx3_ISWETx3_ESModeTP20_RunOptimizationUsingLpSolve()
        {
            // Assign
            int projectId = 1;
            OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MinimizeBudget;
            List<ConstraintTarget> constraintTargets = demoESTargetTP20;
            List<CostEffectivenessInput> costEffectivenessInputs = new List<CostEffectivenessInput>(demoMASBx3_ISWETx3_CostEffectivenessInputs);
            List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>(demoEsoServiceValueWeights);
            List<ObjectiveConstraintInput> inputDTOs = ServicesTestDB.IRS.BuildObjectiveConstraintInputs(optMode, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
            Dictionary<int, string> bmpCombinationTypes = demoBMPCombinationTypes;
            LpSolverInput input = ServicesTestDB.IRS.BuildOptimizationLpSolverInputDTO(inputDTOs, constraintTargets, bmpCombinationTypes);

            SolutionDTO expectRes = new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    }
                }
            };

            // Act
            SolutionDTO res = ServicesTestDB.IRS.RunOptimizationUsingLpSolve(projectId, optMode, input, true);

            // Assert
            Assert.IsTrue(expectRes.Equals(res));
        }

        [Test]
        public void MASBx3_ISWETx3_ESModeTP17_RunOptimizationUsingLpSolve()
        {
            // Assign
            int projectId = 1;
            OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MinimizeBudget;
            List<ConstraintTarget> constraintTargets = demoESTargetTP17;
            List<CostEffectivenessInput> costEffectivenessInputs = new List<CostEffectivenessInput>(demoMASBx3_ISWETx3_CostEffectivenessInputs);
            List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>(demoEsoServiceValueWeights);
            List<ObjectiveConstraintInput> inputDTOs = ServicesTestDB.IRS.BuildObjectiveConstraintInputs(optMode, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
            Dictionary<int, string> bmpCombinationTypes = demoBMPCombinationTypes;
            LpSolverInput input = ServicesTestDB.IRS.BuildOptimizationLpSolverInputDTO(inputDTOs, constraintTargets, bmpCombinationTypes);

            SolutionDTO expectRes = new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 1,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    }
                }
            };

            // Act
            SolutionDTO res = ServicesTestDB.IRS.RunOptimizationUsingLpSolve(projectId, optMode, input, true);

            // Assert
            Assert.IsTrue(expectRes.Equals(res));
        }

        [Test]
        public void MASBx3_ISWETx3_ESModeTP15SM15D1_RunOptimizationUsingLpSolve()
        {
            // Assign
            int projectId = 1;
            OptimizationObjectiveTypeEnum optMode = OptimizationObjectiveTypeEnum.MinimizeBudget;
            List<ConstraintTarget> constraintTargets = demoESTargetTP15SM15D1;
            List<CostEffectivenessInput> costEffectivenessInputs = new List<CostEffectivenessInput>(this.demoMASBx3_ISWETx3_CostEffectivenessInputs);
            List<EcoServiceValueWeight> esWeights = new List<EcoServiceValueWeight>(demoEsoServiceValueWeights);
            List<ObjectiveConstraintInput> inputDTOs = ServicesTestDB.IRS.BuildObjectiveConstraintInputs(optMode, costEffectivenessInputs, constraintTargets.Select(o => o.BMPEffectivenessTypeId).ToHashSet(), esWeights);
            Dictionary<int, string> bmpCombinationTypes = demoBMPCombinationTypes;
            LpSolverInput input = ServicesTestDB.IRS.BuildOptimizationLpSolverInputDTO(inputDTOs, constraintTargets, bmpCombinationTypes);

            SolutionDTO expectRes = new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 3,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    },
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 1,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    }
                }
            };

            // Act
            SolutionDTO res = ServicesTestDB.IRS.RunOptimizationUsingLpSolve(projectId, optMode, input, true);

            // Assert
            Assert.IsTrue(expectRes.Equals(res));
        }

        private readonly List<CostEffectivenessInput> demoMASBx3_ISWETx3_CostEffectivenessInputs = new List<CostEffectivenessInput> {
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 500,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 0.1m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -5.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 2,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 1000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 0.05m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -7.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 700,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = -0.05m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -10.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 1500,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 5.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -15.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 2,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 2000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 10.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -13.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 1000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 15.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -12.0m }
                    }
                }
            };
        private readonly List<CostEffectivenessInput> demoMASBx3_MI48Hx3_ISWETx3_CostEffectivenessInputs = new List<CostEffectivenessInput> {
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 500,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 0.1m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -5.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 2,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 1000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 0.05m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -7.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 19,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 700,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = -0.05m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -10.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 1500,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 5.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -15.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 2,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 2000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 10.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -13.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 1,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP,
                    Cost = 1000,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 15.0m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -12.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 18,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 250,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 1m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -2.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 18,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 350,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 2m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -5.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 18,
                    LocationId = 4,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 750,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 3m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -8.0m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 47,
                    LocationId = 1,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 625,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 1.05m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -6.5m }
                    }
                },
                new CostEffectivenessInput{
                    BMPCombinationTypeId = 47,
                    LocationId = 3,
                    LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision,
                    Cost = 875,
                    EffectivessValues = new List<UnitEffectiveness>
                    {
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 1, Value = 1.95m },
                        new UnitEffectiveness{ BMPEffectivenessTypeId = 11, Value = -14m }
                    }
                }
            };
        private readonly List<EcoServiceValueWeight> demoEsoServiceValueWeights = new List<EcoServiceValueWeight> {
            new EcoServiceValueWeight{ BMPEffectivenessTypeId = 1, Value = 0.2m },
            new EcoServiceValueWeight{ BMPEffectivenessTypeId = 11, Value = 0.8m }
        };
        private readonly Dictionary<int, string> demoBMPCombinationTypes = new Dictionary<int, string> {
            { 1, "ISWET" },
            { 18, "MI48H" },
            { 19, "MASB" },
            { 47, "MI48H_MASB" }
        };
        private readonly List<ConstraintTarget> demoBudgetTarget3000 = new List<ConstraintTarget> {
            new ConstraintTarget { BMPEffectivenessTypeId = 22, LowerValue = 0, UpperValue = 3000 }
        };
        private readonly List<ConstraintTarget> demoBudgetTarget3500 = new List<ConstraintTarget> {
            new ConstraintTarget { BMPEffectivenessTypeId = 22, LowerValue = 0, UpperValue = 3500 }
        };
        private readonly List<ConstraintTarget> demoESTargetTP20 = new List<ConstraintTarget> {
            new ConstraintTarget { BMPEffectivenessTypeId = 11, LowerValue = Decimal.MinValue, UpperValue = -20 }
        };
        private readonly List<ConstraintTarget> demoESTargetTP17 = new List<ConstraintTarget> {
            new ConstraintTarget { BMPEffectivenessTypeId = 11, LowerValue = Decimal.MinValue, UpperValue = -17 }
        };
        private readonly List<ConstraintTarget> demoESTargetTP15SM15D1 = new List<ConstraintTarget> {
            new ConstraintTarget { BMPEffectivenessTypeId = 11, LowerValue = Decimal.MinValue, UpperValue = -15 },
            new ConstraintTarget { BMPEffectivenessTypeId = 1, LowerValue = 15.1m, UpperValue = Decimal.MaxValue }
        };
    }

    class InterfaceTest_IntelligentRecommendationService_UTest
    {
        private readonly Dictionary<string, ProjectDTO> TestDBProjectDTOs = new Dictionary<string, ProjectDTO>
        {
            ["AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM",
                StartYear = 2001,
                EndYear = 2010,
                ScenarioTypeId = 1,
                SpatialUnitId = 1,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Watershed,
                        Id = 1
                    }
                }
            },
            ["AllBMPs|ESMode_TPOff20P|LSD_Existing_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|LSD_Existing_2001_2010_WM",
                StartYear = 2001,
                EndYear = 2010,
                ScenarioTypeId = 2,
                SpatialUnitId = 1,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Watershed,
                        Id = 1
                    }
                }
            },
            ["AllBMPs|ESMode_TPOff20P|Parcel_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|Parcel_Conv_2001_2010_WM",
                StartYear = 2001,
                EndYear = 2010,
                ScenarioTypeId = 1,
                SpatialUnitId = 2,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Watershed,
                        Id = 1
                    }
                }
            },
            ["AllBMPs|ESMode_TPOff20P|Parcel_Existing_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|Parcel_Existing_2001_2010_WM",
                StartYear = 2001,
                EndYear = 2010,
                ScenarioTypeId = 2,
                SpatialUnitId = 2,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Watershed,
                        Id = 1
                    }
                }
            },
            ["LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1|Budget2000_TPOn80_SM20|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1|Budget2000_TPOn80_SM20|LSD_Conv_2001_2010_WM",
                StartYear = 2001,
                EndYear = 2010,
                ScenarioTypeId = 1,
                SpatialUnitId = 1,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Watershed,
                        Id = 1
                    }
                }
            },
        };

        private readonly Dictionary<string, IntelligentRecommendationOptimizationDTO> TestDBOptimizationDTOs = new Dictionary<string, IntelligentRecommendationOptimizationDTO>
        {
            ["AllBMPs"] = new IntelligentRecommendationOptimizationDTO { }, // use default solution
            ["LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1"] = new IntelligentRecommendationOptimizationDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 1, LocationId = 36, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP },
                    new UnitSolution { BMPTypeId = 1, LocationId = 37, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP },
                    new UnitSolution { BMPTypeId = 1, LocationId = 38, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP },
                    new UnitSolution { BMPTypeId = 4, LocationId = 4, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 4, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 6, LocationId = 4, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 13, LocationId = 58, LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP },
                    new UnitSolution { BMPTypeId = 18, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 19, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 27, LocationId = 4, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 27, LocationId = 5, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                }
            },
        };

        private readonly Dictionary<string, OptimizationSettingDTO> TestDBOptimizationSettingDTOs = new Dictionary<string, OptimizationSettingDTO>
        {
            ["ESMode_TPOff20P"] = new OptimizationSettingDTO
            {
                OptimizationType = OptimizationTypeEnum.EcoService,
                Constraints =
                new List<ConstraintInput> {
                    new ConstraintInput {
                        BMPEffectivenessTypeId = 21,
                        OptimizationConstraintValueTypeId = OptimizationConstraintValueTypeEnum.Percent,
                        Value = -20
                    }
                }
            }, // don't need weight

            ["Budget2000_TPOn80_SM20"] = new OptimizationSettingDTO
            {
                OptimizationType = OptimizationTypeEnum.Budget,
                BudgetTarget = 2000,
                EsWeights = new List<EcoServiceValueWeight>
                {
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 1, Value = 20 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 2, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 3, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 4, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 5, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 6, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 7, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 8, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 9, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 10, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 11, Value = 80 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 12, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 13, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 14, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 15, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 16, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 17, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 18, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 19, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 20, Value = 0 },
                    new EcoServiceValueWeight { BMPEffectivenessTypeId = 21, Value = 0 }
                }
            },

        };

        private readonly int TestUserId = 1;

        private int GetProjectId(string projectName)
        {
            var p = ServicesTestDB.UoW.GetRepository<Project>().Get(x => x.Name == projectName).FirstOrDefault();

            if (p != null)
            {
                return p.Id;
            }
            else
            {
                string[] token = projectName.Split("|");

                /* Add project to DB first */
                ServicesTestDB.ProjectDataService.SaveProject(TestUserId, TestDBProjectDTOs[projectName]);

                int pId = ServicesTestDB.UoW.GetRepository<Project>().Get(x => x.Name == projectName).FirstOrDefault().Id;

                IntelligentRecommendationOptimizationDTO o = TestDBOptimizationDTOs[token[0]];

                o.ProjectId = pId;
                ServicesTestDB.USS.SaveOptimization(o);

                ServicesTestDB.IRS.SaveOptimizationSettings(pId, TestDBOptimizationSettingDTOs[token[1]]);

                return pId;
            }
        }

        [Test]
        public void AllBMPs_ESMode_TPOff20P_LSD_Conv_2001_2010_WM_GetRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM";
            int projectId = this.GetProjectId(projectName);

            // Act
            SolutionDTO res = ServicesTestDB.IRS.GetRecommendedSolution(projectId, true);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void AllBMPs_ESMode_TPOff20P_LSD_Conv_2001_2010_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM";
            int projectId = this.GetProjectId(projectName);

            // Act
            bool res = ServicesTestDB.IRS.BuildRecommendedSolution(projectId, true);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1_Budget2000_TPOn80_SM20_LSD_Conv_2001_2010_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1|Budget2000_TPOn80_SM20|LSD_Conv_2001_2010_WM";
            int projectId = this.GetProjectId(projectName);

            // Act
            bool res = ServicesTestDB.IRS.BuildRecommendedSolution(projectId, true);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1_Budget2000_TPOn80_SM20_LSD_Conv_2001_2010_WM_BuildRecommendedSolution_VerifyResult()
        {
            // Assign
            string projectName = "LSD_VFSTx2_GWWx1_MI48Hx1_MASBx1_ROGZx2_ISWETx3_MCBIx1|Budget2000_TPOn80_SM20|LSD_Conv_2001_2010_WM";
            int projectId = this.GetProjectId(projectName);

            SolutionDTO expectRes = new SolutionDTO
            {
                ProjectId = projectId,
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 36,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    },
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 37,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    },
                    new UnitSolution{
                        BMPTypeId = 1,
                        LocationId = 38,
                        LocationType = OptimizationSolutionLocationTypeEnum.ReachBMP
                    },
                    new UnitSolution{
                        BMPTypeId = 27,
                        LocationId = 5,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 4,
                        LocationId = 4,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 6,
                        LocationId = 4,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 27,
                        LocationId = 4,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 4,
                        LocationId = 6,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    },
                    new UnitSolution{
                        BMPTypeId = 19,
                        LocationId = 6,
                        LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision
                    }
                }
            };

            // Act
            ServicesTestDB.IRS.BuildRecommendedSolution(projectId, true);
            var res = ServicesTestDB.USS.GetSelectedSolutionBmpLocationsByProjectId(projectId);

            // Assert
            Assert.IsTrue(expectRes.Equals(res));
        }

        //[Test]
        public void SaveProjectBMPCostEffectiveness()
        {
            string projectName, outPath;
            string dir = @"C:\Users\Shawn\Desktop\ESATTestDBCostEffectiveness\";

            projectName = "AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM";
            outPath = $"{dir}{projectName.Replace("|", "_")}.txt";
            this.GetAllBMPCostEffectiveness(projectName, outPath);

            projectName = "AllBMPs|ESMode_TPOff20P|LSD_Existing_2001_2010_WM";
            outPath = $"{dir}{projectName.Replace("|", "_")}.txt";
            this.GetAllBMPCostEffectiveness(projectName, outPath);

            projectName = "AllBMPs|ESMode_TPOff20P|Parcel_Conv_2001_2010_WM";
            outPath = $"{dir}{projectName.Replace("|", "_")}.txt";
            this.GetAllBMPCostEffectiveness(projectName, outPath);

            projectName = "AllBMPs|ESMode_TPOff20P|Parcel_Existing_2001_2010_WM";
            outPath = $"{dir}{projectName.Replace("|", "_")}.txt";
            this.GetAllBMPCostEffectiveness(projectName, outPath);

            // Assert
            Assert.Pass();
        }

        //[Test]
        private void GetAllBMPCostEffectiveness(string projectName, string outPath)
        {
            int projectId = this.GetProjectId(projectName);

            List<BMPGeometryCostEffectivenessDTO> res = ServicesTestDB.PSS.GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(projectId);

            StringBuilder sb = new StringBuilder();

            string header = $"LocationType|LocationId|BMPType|BMPTypeId|BMPArea(ha)|Cost($)|Carbon(ton)|Biodiversity|" +
                $"Runoff_onsite(m3)|SoilMoisture(m3)|TSS_onsite(ton)|TN_onsite(kg)|TP_onsite(kg)|" +
                $"Runoff_offsite(m3)|TSS_offsite(ton)|TN_offsite(kg)|TP_offsite(kg)|";

            sb.AppendLine(header);

            /*
             * Id   BMPEffectivenessType
                1	"Soil moisture onsite"
                2	"ET onsite"
                3	"Groundwater recharge onsite"
                4	"Runoff onsite"
                5	"TSS onsite"
                6	"DN onsite"
                7	"PN onsite"
                8	"TN onsite"
                9	"DP onsite"
                10	"PP onsite"
                11	"TP onsite"
                12	"Soil carbon onsite"
                13	"Biodiversity onsite"
                14	"Runoff offsite"
                15	"TSS offsite"
                16	"DN offsite"
                17	"PN offsite"
                18	"TN offsite"
                19	"DP offsite"
                20	"PP offsite"
                21	"TP offsite"
                22	"BMP cost"
             */

            foreach (var r in res)
            {
                sb.AppendLine(
                    $"{r.OptimizationSolutionLocationTypeId}|{r.LocationId}|{r.BMPCombinationTypeName}|{r.BMPCombinationTypeId}|" +
                    $"{r.BMPArea}|{r.Cost}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 12)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 13)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 4)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 1)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 5)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 8)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 11)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 14)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 15)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 18)?.EffectivenessValue ?? -999}|" +
                    $"{r.EffectivenessDTOs.Find(x => x.BMPEffectivenessTypeId == 21)?.EffectivenessValue ?? -999}|"
                    );
            }

            File.WriteAllText(outPath, sb.ToString());            
        }
    }
}
