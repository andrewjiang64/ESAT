using AgBMPTool.BLL.Models.IntelligentRecommendation;
using System.Collections.Generic;
using System.Linq;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using AgBMPTool.DBModel.Model.Project;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Project;
using NUnit.Framework;

namespace AgBMPTool.BLL.IFC.Test
{
    public class IFC_IntelligentRecommendation_UTest
    {
        private readonly int TestUserId = 1;
        private int GetProjectIdForOptimization(string projectName)
        {
            var p = ServicesIFC.UoW.GetRepository<Project>().Get(x => x.Name == projectName).FirstOrDefault();

            if (p != null)
            {
                return p.Id;
            }
            else
            {
                string[] token = projectName.Split("|");

                /* Add project to DB first */
                ServicesIFC.ProjectDataService.SaveProject(TestUserId, TestDBProjectDTOs[projectName]);

                int pId = ServicesIFC.UoW.GetRepository<Project>().Get(x => x.Name == projectName).FirstOrDefault().Id;

                IntelligentRecommendationOptimizationDTO o = TestDBOptimizationDTOs[token[0]];

                o.ProjectId = pId;
                ServicesIFC.USS.SaveOptimization(o);

                ServicesIFC.IRS.SaveOptimizationSettings(pId, TestDBOptimizationSettingDTOs[token[1]]);

                return pId;
            }
        }

        private readonly Dictionary<string, ProjectDTO> TestDBProjectDTOs = new Dictionary<string, ProjectDTO>
        {
            ["AllBMPs|ESMode_TPOff20P|LSD_Conv_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|LSD_Conv_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff20P|LSD_Existing_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|LSD_Existing_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff20P|Parcel_Conv_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|Parcel_Conv_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff20P|Parcel_Existing_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff20P|Parcel_Existing_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff05P|LSD_Conv_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff05P|LSD_Conv_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff05P|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff05P|LSD_Conv_2001_2010_WM",
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
            ["AllBMPs|ESMode_TPOff05P|LSD_Existing_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff05P|LSD_Existing_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff05P|Parcel_Conv_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff05P|Parcel_Conv_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
            ["AllBMPs|ESMode_TPOff02P|Parcel_Existing_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|ESMode_TPOff02P|Parcel_Existing_1978_2017_WM",
                StartYear = 1978,
                EndYear = 2017,
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
        };

        private readonly Dictionary<string, IntelligentRecommendationOptimizationDTO> TestDBOptimizationDTOs = new Dictionary<string, IntelligentRecommendationOptimizationDTO>
        {
            ["AllBMPs"] = new IntelligentRecommendationOptimizationDTO { }, // use default solution
            
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

            ["ESMode_TPOff05P"] = new OptimizationSettingDTO
            {
                OptimizationType = OptimizationTypeEnum.EcoService,
                Constraints =
                new List<ConstraintInput> {
                    new ConstraintInput {
                        BMPEffectivenessTypeId = 21,
                        OptimizationConstraintValueTypeId = OptimizationConstraintValueTypeEnum.Percent,
                        Value = -5
                    }
                }
            }, // don't need weight

            ["ESMode_TPOff02P"] = new OptimizationSettingDTO
            {
                OptimizationType = OptimizationTypeEnum.EcoService,
                Constraints =
                new List<ConstraintInput> {
                    new ConstraintInput {
                        BMPEffectivenessTypeId = 21,
                        OptimizationConstraintValueTypeId = OptimizationConstraintValueTypeEnum.Percent,
                        Value = -2
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

        [Test]
        public void AllBMPs_ESMode_TPOff20P_LSD_Conv_1978_2017_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff20P|LSD_Conv_1978_2017_WM";
            int projectId = this.GetProjectIdForOptimization(projectName);

            // Act
            bool res = ServicesIFC.IRS.BuildRecommendedSolution(projectId, false);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void AllBMPs_ESMode_TPOff20P_LSD_Conv_2001_2010_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff20P|LSD_Conv_2001_2010_WM";
            int projectId = this.GetProjectIdForOptimization(projectName);

            // Act
            bool res = ServicesIFC.IRS.BuildRecommendedSolution(projectId, false);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void AllBMPs_ESMode_TPOff20P_Parcel_Conv_1978_2017_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff20P|Parcel_Conv_1978_2017_WM";
            int projectId = this.GetProjectIdForOptimization(projectName);

            // Act
            bool res = ServicesIFC.IRS.BuildRecommendedSolution(projectId, false);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void AllBMPs_ESMode_TPOff05P_LSD_Existing_1978_2017_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff05P|LSD_Existing_1978_2017_WM";
            int projectId = this.GetProjectIdForOptimization(projectName);

            // Act
            bool res = ServicesIFC.IRS.BuildRecommendedSolution(projectId, false);

            // Assert
            Assert.True(res);
        }

        [Test]
        public void AllBMPs_ESMode_TPOff02P_Parcel_Existing_1978_2017_WM_BuildRecommendedSolution_ExecutionTest()
        {
            // Assign
            string projectName = "AllBMPs|ESMode_TPOff02P|Parcel_Existing_1978_2017_WM";
            int projectId = this.GetProjectIdForOptimization(projectName);

            // Act
            bool res = ServicesIFC.IRS.BuildRecommendedSolution(projectId, false);

            // Assert
            Assert.True(res);
        }
    }
}
