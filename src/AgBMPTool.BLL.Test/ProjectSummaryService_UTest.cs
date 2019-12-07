using AgBMPTool.BLL.Models.Project;
using AgBMPTool.DBModel.Model.Project;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.TestDB.Test
{
    public class ProjectSummaryServiceUTest
    {
        #region Local support data and methods
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

                SolutionDTO o = token[0] == "AllBMPs" ? ServicesTestDB.USS.GetAllSolutionBmpLocationsByProjectId(pId)
                    : TestDBSolutionDTOs[token[0]];

                o.ProjectId = pId;
                ServicesTestDB.USS.SaveSolution(o, false);

                return pId;
            }
        }

        private readonly Dictionary<string, ProjectDTO> TestDBProjectDTOs = new Dictionary<string, ProjectDTO>
        {
            ["AllBMPs|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|LSD_Conv_2001_2010_WM",
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
            ["AllBMPs|LSD_Existing_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|LSD_Existing_2001_2010_WM",
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
            ["AllBMPs|Parcel_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|Parcel_Conv_2001_2010_WM",
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
            ["AllBMPs|Parcel_Existing_2001_2010_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|Parcel_Existing_2001_2010_WM",
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
            ["LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM",
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
            ["LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM"] = new ProjectDTO
            {
                Name = "LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM",
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
            ["LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"] = new ProjectDTO
            {
                Name = "LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM",
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
            ["LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM"] = new ProjectDTO
            {
                Name = "LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM",
                StartYear = 2002,
                EndYear = 2006,
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
            ["Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"] = new ProjectDTO
            {
                Name = "Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM",
                StartYear = 2002,
                EndYear = 2006,
                ScenarioTypeId = 1,
                SpatialUnitId = 2,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Municipality,
                        Id = 1
                    }
                }
            },
            ["Parcel_Existing_All_MI48H|Parcel_Existing_2002_2006_MM"] = new ProjectDTO
            {
                Name = "Parcel_Existing_All_MI48H|Parcel_Existing_2002_2006_MM",
                StartYear = 2002,
                EndYear = 2006,
                ScenarioTypeId = 2,
                SpatialUnitId = 2,
                Scope = new List<ProjectScopeDTO> {
                    new ProjectScopeDTO
                    {
                        ScenarioResultSummarizationTypeId = (int)ScenarioResultSummarizationTypeEnum.Municipality,
                        Id = 1
                    }
                }
            },
        };

        private readonly Dictionary<string, SolutionDTO> TestDBSolutionDTOs = new Dictionary<string, SolutionDTO>
        {
            ["AllBMPs"] = new SolutionDTO { }, // use default solution
            ["LSD_Conv_All_MI48H"] = new SolutionDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 18, LocationId = 15, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 17, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 2, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 7, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 8, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 1, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 16, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 9, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                }
            },
            ["LSD_Conv_All_GWW"] = new SolutionDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 6, LocationId = 17, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 6, LocationId = 4, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 6, LocationId = 12, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 6, LocationId = 13, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                }
            },
            ["LSD_Existing_All_MI48H"] = new SolutionDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 18, LocationId = 15, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 17, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 7, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 8, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 16, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },
                    new UnitSolution { BMPTypeId = 18, LocationId = 9, LocationType = OptimizationSolutionLocationTypeEnum.LegalSubDivision },}
            },
            ["Parcel_Conv_All_MI48H"] = new SolutionDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 18, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                    new UnitSolution { BMPTypeId = 18, LocationId = 5, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                    new UnitSolution { BMPTypeId = 18, LocationId = 2, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                }
            },
            ["Parcel_Existing_All_MI48H"] = new SolutionDTO
            {
                UnitSolutions = new List<UnitSolution> {
                    new UnitSolution { BMPTypeId = 18, LocationId = 6, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                    new UnitSolution { BMPTypeId = 18, LocationId = 5, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                    new UnitSolution { BMPTypeId = 18, LocationId = 2, LocationType = OptimizationSolutionLocationTypeEnum.Parcel },
                }
            },
        };
        #endregion

        #region *** Project Summary ***
        [Test]
        public void TestSingleFieldBMP_ValueChange_GetProjectEffectivenessSummaryByEffectivenessType()
        {
            // Assign
            int projectId1 = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM");
            int projectId2 = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM");
            int bmpEffectivenessTypeId1 = 21; // TP off-site
            int bmpEffectivenessTypeId2 = 11; // TP on-site
            decimal expectVC11 = (decimal)-3.232; // TP off-site (Conv, GWW, 2001-2010) = -2.089% * 154.74 = -3.232
            decimal expectVC21 = (decimal)-2.312; // TP off-site (Exis, MI48H, 2002-2006) = -1.608% * 143.75 = -2.312
            decimal expectVC12 = (decimal)-1.886480; // TP on-site (Conv, GWW, 2001-2010) = -1.886480
            decimal expectVC22 = (decimal)-1.426899; // TP on-site (Exis, MI48H, 2002-2006) = -1.426899

            // Act
            var res1 = ServicesTestDB.PSS.GetProjectEffectivenessSummaryDTOs(projectId1);
            var res2 = ServicesTestDB.PSS.GetProjectEffectivenessSummaryDTOs(projectId2);

            EffectivenessSummaryDTO res11 = res1.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);
            EffectivenessSummaryDTO res21 = res2.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);

            EffectivenessSummaryDTO res12 = res1.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);
            EffectivenessSummaryDTO res22 = res2.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);

            // Assert
            Assert.AreEqual(res11.BMPEffectivenessType.Id, bmpEffectivenessTypeId1);
            Assert.Zero((int)(100 * (expectVC11 - res11.ValueChange))); // difference less than +-0.01 
            Assert.AreEqual(res21.BMPEffectivenessType.Id, bmpEffectivenessTypeId1);
            Assert.Zero((int)(100 * (expectVC21 - res21.ValueChange))); // difference less than +-0.01 
            Assert.AreEqual(res12.BMPEffectivenessType.Id, bmpEffectivenessTypeId2);
            Assert.Zero((int)(100 * (expectVC12 - res12.ValueChange))); // difference less than +-0.01 
            Assert.AreEqual(res22.BMPEffectivenessType.Id, bmpEffectivenessTypeId2);
            Assert.Zero((int)(100 * (expectVC22 - res22.ValueChange))); // difference less than +-0.01 
        }

        [Test]
        public void TestSingleFieldBMP_GetEffectivenessSummaryByProjectList()
        {
            // Assign
            List<ProjectDTO> projectId1 = new List<ProjectDTO> { new ProjectDTO { Id = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM") },
                                                new ProjectDTO { Id = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM") }};
            int bmpEffectivenessTypeId1 = 21; // TP off-site
            int bmpEffectivenessTypeId2 = 11; // TP on-site
            decimal expectVC11 = (decimal)-3.232; // TP off-site (Conv, GWW, 2001-2010) = -2.089% * 154.74 = -3.232
            decimal expectVC21 = (decimal)-2.312; // TP off-site (Exis, MI48H, 2002-2006) = -1.608% * 143.75 = -2.312
            decimal expectVC12 = (decimal)-1.886480; // TP on-site (Conv, GWW, 2001-2010) = -1.886480
            decimal expectVC22 = (decimal)-1.426899; // TP on-site (Exis, MI48H, 2002-2006) = -1.426899

            // Act
            var res = ServicesTestDB.PSS.GetProjectSummaryDTOs(projectId1);
            ProjectSummaryDTO res1 = res[0];
            ProjectSummaryDTO res2 = res[1];

            EffectivenessSummaryDTO res11 = res1.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);
            EffectivenessSummaryDTO res21 = res2.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);

            EffectivenessSummaryDTO res12 = res1.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);
            EffectivenessSummaryDTO res22 = res2.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);

            // Assert
            Assert.Zero((int)(100 * (expectVC11 - res11.ValueChange))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (expectVC21 - res21.ValueChange))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (expectVC12 - res12.ValueChange))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (expectVC22 - res22.ValueChange))); // difference less than +-0.01 
        }

        [Test]
        public void TestSingleFieldBMP_GetProjectCost()
        {
            // Assign
            int projectId1 = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM");
            int projectId2 = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM");
            decimal expectCost1 = (decimal)430.71; // Cost (Conv, GWW, 2001-2010) = 430.71
            decimal expectCost2 = (decimal)305.17; // Cost (Exis, MI48H, 2002-2006) = 305.17

            // Act
            decimal res1 = ServicesTestDB.PSS.GetProjectCost(projectId1);
            decimal res2 = ServicesTestDB.PSS.GetProjectCost(projectId2);

            // Assert
            Assert.Zero((int)(100 * (expectCost1 - res1))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (expectCost2 - res2))); // difference less than +-0.01 
        }

        [Test]
        public void TestSingleFieldBMP_GetCostByProjectList()
        {
            // Assign
            List<ProjectDTO> projectId1 = new List<ProjectDTO> { new ProjectDTO { Id = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM") },
                                                                 new ProjectDTO { Id = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM") }};

            decimal expectCost1 = (decimal)430.71; // Cost (Conv, GWW, 2001-2010) = 430.71
            decimal expectCost2 = (decimal)305.17; // Cost (Exis, MI48H, 2002-2006) = 305.17

            // Act
            var res = ServicesTestDB.PSS.GetProjectSummaryDTOs(projectId1);
            ProjectSummaryDTO res1 = res[0];
            ProjectSummaryDTO res2 = res[1];

            // Assert
            Assert.Zero((int)(100 * (expectCost1 - res1.Cost))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (expectCost2 - res2.Cost))); // difference less than +-0.01 
        }

        [Test]
        public void TestSingleFieldBMP_GetProjectBMPSummaries()
        {
            // Assign
            int projectId1 = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM");
            int projectId2 = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2002_2006_WM");

            int expectBMPNum1 = 1;
            int expectCount1 = 4; // Cost (Conv, GWW, 2001-2010) = 4 LSDs
            decimal expectArea1 = (decimal)44.165927382; // Area (Conv, GWW, 2001-2010) = 3.625000
            decimal expectCost1 = (decimal)430.712000; // Cost (Conv, GWW, 2001-2010) = 430.712000

            int expectBMPNum2 = 1;
            int expectCount2 = 7; // Count (Exis, MI48H, 2002-2006) = 7 LSDs
            decimal expectArea2 = (decimal)48.739510; // Area (Exis, MI48H, 2002-2006) = 48.739510
            decimal expectCost2 = (decimal)305.170000; // Cost (Exis, MI48H, 2002-2006) = 305.170000

            // Act
            var res1 = ServicesTestDB.PSS.GetProjectBMPSummaryDTOs(projectId1);
            var res2 = ServicesTestDB.PSS.GetProjectBMPSummaryDTOs(projectId2);

            // Assert
            Assert.AreEqual(expectBMPNum1, res1.Count);
            Assert.AreEqual(expectCount1, res1.Find(x => x.BMPTypeId == 6).ModelComponentCount);
            Assert.Zero((int)(100 * (expectArea1 - res1.Find(x => x.BMPTypeId == 6).TotalArea))); // difference less than +-0.0001 
            Assert.Zero((int)(100 * (expectCost1 - res1.Find(x => x.BMPTypeId == 6).TotalCost))); // difference less than +-0.01 

            Assert.AreEqual(expectBMPNum2, res2.Count);
            Assert.AreEqual(expectCount2, res2.Find(x => x.BMPTypeId == 18).ModelComponentCount);
            Assert.Zero((int)(100 * (expectArea2 - res2.Find(x => x.BMPTypeId == 18).TotalArea))); // difference less than +-0.0001 
            Assert.Zero((int)(100 * (expectCost2 - res2.Find(x => x.BMPTypeId == 18).TotalCost))); // difference less than +-0.01 
        }

        //[Test]
        //public void TestSingleFieldBMP_PercentChange_GetProjectEffectivenessSummaryByEffectivenessType()
        //{
        //    // Assign
        //    List<ProjectDTO> projectId1 = new List<ProjectDTO> { new ProjectDTO { Id = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM") } };
        //    int userId1 = 1;
        //    List<ProjectDTO> projectId2 = new List<ProjectDTO> { new ProjectDTO { Id = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM") } };
        //    int userId2 = 2;
        //    int bmpEffectivenessTypeId1 = 21; // TP off-site
        //    int bmpEffectivenessTypeId2 = 11; // TP on-site
        //    decimal expectPC11 = (decimal)-2.089; // TP off-site (Conv, GWW, 2001-2010) = -2.089
        //    decimal expectPC21 = (decimal)-1.608; // TP off-site (Exis, MI48H, 2002-2006) = -1.608
        //    decimal expectPC12 = (decimal)-5.861910; // TP on-site (Conv, GWW, 2001-2010) = -5.861910
        //    decimal expectPC22 = (decimal)-3.204066; // TP on-site (Exis, MI48H, 2002-2006) = -3.204066

        //    // Act
        //    ProjectSummaryDTO res1 = Services.PSS.GetProjectSummaryDTOs(projectId1, userId1)[0];
        //    ProjectSummaryDTO res2 = Services.PSS.GetProjectSummaryDTOs(projectId2, userId2)[0];

        //    EffectivenessSummaryDTO res11 = res1.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);
        //    EffectivenessSummaryDTO res21 = res2.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);

        //    EffectivenessSummaryDTO res12 = res1.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);
        //    EffectivenessSummaryDTO res22 = res2.EffectivenessSummaryDTOs.Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId2);

        //    // Assert
        //    Assert.AreEqual(res11.BMPEffectivenessType.Id, bmpEffectivenessTypeId1);
        //    Assert.Zero((int)(100 * (expectPC11 - res11.PercentChange))); // difference less than +-0.01 
        //    Assert.AreEqual(res21.BMPEffectivenessType.Id, bmpEffectivenessTypeId1);
        //    Assert.Zero((int)(100 * (expectPC21 - res21.PercentChange))); // difference less than +-0.01 
        //    Assert.AreEqual(res12.BMPEffectivenessType.Id, bmpEffectivenessTypeId2);
        //    Assert.Zero((int)(100 * (expectPC12 - res12.PercentChange))); // difference less than +-0.01 
        //    Assert.AreEqual(res22.BMPEffectivenessType.Id, bmpEffectivenessTypeId2);
        //    Assert.Zero((int)(100 * (expectPC22 - res22.PercentChange))); // difference less than +-0.01 
        //}
        #endregion

        #region *** Baseline Project Summary ***
        [Test]
        public void TotalCost_AllBMPs_Parcel_Existing_2001_2010_WM_GetProjectBaselineBMPSummaryDTOs()
        {
            // Assign
            int projectId1 = this.GetProjectId("AllBMPs|Parcel_Existing_2001_2010_WM");

            decimal exp = 0; // 0

            // Act
            var res = ServicesTestDB.PSS.GetProjectBaselineBMPSummaryDTOs(projectId1);

            // Assert
            foreach (var bmpSummary in res)
            {
                Assert.Zero((int)(100 * (exp - bmpSummary.TotalCost))); // difference less than +-0.01 
            }
        }

        [Test]
        public void ModelComponentCount_AllBMPs_Parcel_Existing_2001_2010_WM_GetProjectBaselineBMPSummaryDTOs()
        {
            // Assign
            int projectId1 = this.GetProjectId("AllBMPs|Parcel_Existing_2001_2010_WM");

            decimal exp1 = 1; // Parcel 2 MASB (19)
            decimal exp2 = 1; // Parcel 4 ROGZ (27)
            decimal exp3 = 1; // Parcel 5 MI48H (18)

            // Act
            var res = ServicesTestDB.PSS.GetProjectBaselineBMPSummaryDTOs(projectId1);

            // Assert
            Assert.Zero((int)(100 * (exp1 - res.Where(o => o.BMPTypeId == 19).Select(o => o.ModelComponentCount).Sum()))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (exp2 - res.Where(o => o.BMPTypeId == 27).Select(o => o.ModelComponentCount).Sum()))); // difference less than +-0.01 
            Assert.Zero((int)(100 * (exp3 - res.Where(o => o.BMPTypeId == 18).Select(o => o.ModelComponentCount).Sum()))); // difference less than +-0.01 
        }

        [Test]
        public void TotalArea_AllBMPs_Parcel_Existing_2001_2010_WM_GetProjectBaselineBMPSummaryDTOs()
        {
            // Assign
            int projectId1 = this.GetProjectId("AllBMPs|Parcel_Existing_2001_2010_WM");

            decimal exp1 = 18.987929m; // Parcel 2 MASB (19)
            decimal exp2 = 6.395667m; // Parcel 4 ROGZ (27)
            decimal exp3 = 5.2384424m; // Parcel 5 MI48H (18)

            // Act
            var res = ServicesTestDB.PSS.GetProjectBaselineBMPSummaryDTOs(projectId1);

            // Assert
            Assert.Zero((int)(100 * (exp1 - res.Where(o => o.BMPTypeId == 19).Select(o => o.TotalArea).Sum()))); // difference less than +-0.0001 
            Assert.Zero((int)(100 * (exp2 - res.Where(o => o.BMPTypeId == 27).Select(o => o.TotalArea).Sum()))); // difference less than +-0.0001 
            Assert.Zero((int)(100 * (exp3 - res.Where(o => o.BMPTypeId == 18).Select(o => o.TotalArea).Sum()))); // difference less than +-0.0001 
        }


        //[Test]
        //public void PercentChange_Offsite_GetProjectBaselineEffectiveness()
        //{
        //    // Assign
        //    int projectId1 = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM");
        //    int projectId2 = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM");
        //    int bmpEffectivenessTypeId1 = 21; // TP off-site
        //    decimal expectPC11 = (decimal)0; // TP off-site SUM = 0
        //    decimal expectPC21 = (decimal)-1.098; // TP off-site SUM = -1.098; MI48H = -0.174, MASB = -0.658, ROGZ = -0.266

        //    // Act
        //    decimal actPC1 = ServicesTestDB.PSS.GetProjectBaselineEffectivenessSummaryDTOs(projectId1).Count;
        //    EffectivenessSummaryDTO res21 = ServicesTestDB.PSS.GetProjectBaselineEffectivenessSummaryDTOs(projectId2).Find(x => x.BMPEffectivenessType.Id == bmpEffectivenessTypeId1);

        //    // Assert
        //    Assert.Zero((int)(100 * (expectPC11 - actPC1))); // difference less than +-0.01 
        //    Assert.AreEqual(res21.BMPEffectivenessType.Id, bmpEffectivenessTypeId1);
        //    Assert.Zero((int)(100 * (expectPC21 - res21.PercentChange))); // difference less than +-0.01 
        //}

        //[Test]
        //public void GetProjectBaselineBMPSummaries()
        //{
        //    // Assign
        //    int projectId1 = this.GetProjectId("LSD_Conv_All_GWW|LSD_Conv_2001_2010_WM");
        //    int projectId2 = this.GetProjectId("Parcel_Existing_All_MI48H|Parcel_Existing_2002_2006_MM");

        //    int expectBMPNum1 = 0;
        //    int expectCount1 = 0; // Cost (Conv, GWW, 2001-2010) = 0
        //    decimal expectArea1 = (decimal)0; // Area (Conv, GWW, 2001-2010) = 0
        //    decimal expectCost1 = (decimal)0; // Cost (Conv, GWW, 2001-2010) = 0

        //    int expectBMPNum2 = 3; // MI48H, MASB, ROGZ
        //    int expectCount2 = 3; // MI48H 1 parcel, MASB 1 parcel, ROGZ 1 parcel in baseline total = 3
        //    decimal expectArea2 = (decimal)30.622035; // Area = 30.622035
        //    decimal expectCost2 = (decimal)217.704; // SUM = 217.704; MI48H = 35.402, MASB = 129.222, ROGZ = 53.08

        //    // Act                
        //    var res1 = ServicesTestDB.PSS.GetProjectBaselineBMPSummaryDTOs(projectId1);
        //    var res2 = ServicesTestDB.PSS.GetProjectBaselineBMPSummaryDTOs(projectId2);

        //    int actCount2 = 0;
        //    decimal actArea = (decimal)0.0;
        //    decimal actCost = (decimal)0.0;
        //    foreach (var r in res2)
        //    {
        //        actCount2 += r.ModelComponentCount;
        //        actArea += r.TotalArea;
        //        actCost += r.TotalCost;
        //    }

        //    // Assert
        //    Assert.AreEqual(expectBMPNum1, res1.Count);
        //    Assert.AreEqual(expectCount1, res1.Count);
        //    Assert.Zero((int)(10000 * (expectArea1 - res1.Count))); // difference less than +-0.0001 
        //    Assert.Zero((int)(100 * (expectCost1 - res1.Count))); // difference less than +-0.01 

        //    Assert.AreEqual(expectBMPNum2, res2.Count);
        //    Assert.AreEqual(expectCount2, actCount2);
        //    Assert.Zero((int)(10000 * (expectArea2 - actArea))); // difference less than +-0.0001 
        //    Assert.Zero((int)(100 * (expectCost2 - actCost))); // difference less than +-0.01 
        //}
        #endregion

        #region *** BMP Geometry Cost Effectiveness ***
        [Test]
        public void LocationCount_LSD_MI48H_Existing_2001_2010_GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager        
            int bmpTypeId = 18;

            int expect = 7; // correct = 7 LSD

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void LocationCount_LSD_MI48H_Conventional_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM"); // Conventional_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;

            int expect = 9; // correct = 9 LSD

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void LocationCount_LSD_ISWET_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 1;

            int expect = 3; // correct = 3 Wetlands

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void LocationCount_LSD_MCBI_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 13;

            int expect = 1; // correct = 1 Catch basin

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void LocationCount_Parcel_MI48H_Conventional_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 18;

            int expect = 3; // correct = 3 Parcels

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void LocationCount_Parcel_ISWET_Existing_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 1;

            int expect = 3; // correct = 3 Wetlands

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.AreEqual(expect, res.FindAll(x => x.BMPCombinationTypeId == bmpTypeId && x.IsSelectable).Count);
        }

        [Test]
        public void Effectiveness_TP_Onsite_LSD_MI48H_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;
            int lsdId = 6;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.3625318; // correct = -0.3625318

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == lsdId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Onsite_LSD_MI48H_Conventional_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM"); // Conventional_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;
            int locationId = 6;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.3623252; // correct = -0.3623252

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Onsite_LSD_ISWET_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 1;
            int locationId = 36;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-1.1835289; // correct = -1.1835289

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Onsite_LSD_MCBI_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 13;
            int locationId = 58;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-5.4374332; // correct = -5.4374332

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Onsite_Parcel_MI48H_Conventional_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 18;
            int locationId = 6;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.4771632; // correct = -0.4771632

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Onsite_Parcel_ISWET_Existing_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 1;
            int locationId = 36;
            int bmpEffectivenessLocationId = 1; // On-site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-1.1723976; // correct = -1.1723976

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_LSD_MI48H_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;
            int lsdId = 6;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.6132449; // correct = -0.6132449

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == lsdId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_LSD_MI48H_Conventional_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM"); // Conventional_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;
            int locationId = 6;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.6515719; // correct = -0.6515719

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_LSD_ISWET_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 1;
            int locationId = 36;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-1.659759; // correct = -1.659759

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_LSD_MCBI_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 13;
            int locationId = 58;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-6.05542; // correct = -6.05542

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_Parcel_MI48H_Conventional_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 18;
            int locationId = 6;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-0.797364; // correct = -0.797364

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Effectiveness_TP_Offsite_Parcel_ISWET_Existing_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 1;
            int locationId = 36;
            int bmpEffectivenessLocationId = 2; // Off-Site
            int scenarioModelResultVariableTypeId = 13; // TP

            decimal expect = (decimal)-1.7404128; // correct = -1.7404128

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId)
                                .EffectivenessDTOs
                                .Find(xx => xx.BMPEffectivenessLocationTypeId == bmpEffectivenessLocationId
                                        && xx.ScenarioModelResultVariableTypeId == scenarioModelResultVariableTypeId)
                                .EffectivenessValue))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_LSD_MI48H_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18; // MI48H
            int locationId = 6;

            decimal expect = (decimal)86.146; // correct = 86.146

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_LSD_MI48H_Conventional_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Conv_All_MI48H|LSD_Conv_2001_2010_WM"); // Conventional_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 18;
            int locationId = 6;

            decimal expect = (decimal)87.395; // correct = 87.395

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_LSD_ISWET_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 1;
            int locationId = 36;

            decimal expect = (decimal)200; // correct = 200
            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_LSD_MCBI_Existing_2001_2010_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("LSD_Existing_All_MI48H|LSD_Existing_2001_2010_WM"); // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int bmpTypeId = 13;
            int locationId = 58;

            decimal expect = (decimal)1500; // correct = 1500

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_Parcel_MI48H_Conventional_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Conv_All_MI48H|Parcel_Conv_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 18;
            int locationId = 6;

            decimal expect = (decimal)105.798; // correct = 105.798

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }

        [Test]
        public void Cost_Parcel_ISWET_Existing_2002_2006_GetBMPGeometryCostEffectivenessDTO()
        {
            // Assign
            int proj = this.GetProjectId("Parcel_Existing_All_MI48H|Parcel_Existing_2002_2006_MM"); // Conventional_Parcel_MI48H_2002_2006_Municipality Manager
            int bmpTypeId = 1;
            int locationId = 36;

            decimal expect = (decimal)200; // correct = 200

            // Act
            var res = ServicesTestDB.PSS.GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(proj, bmpTypeId);

            // Assert
            Assert.Zero((int)(1000 * (expect - res.Find(x => x.BMPCombinationTypeId == bmpTypeId && x.LocationId == locationId).Cost))); // difference less than +-0.001 
        }
        #endregion

        #region *** LSD / Parcel BMP Summary ***
        [Test]
        public void LsdParcelBMPSummary_LSD_AllBMPs_Existing_2001_2010_GetLsdParcelBMPSummaryDTOs()
        {
            // Assign
            int proj = this.GetProjectId("AllBMPs|LSD_Existing_2001_2010_WM");

            decimal expectCost = 3477.946m; // correct = 3477.946

            // Act
            var res = ServicesTestDB.PSS.GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(proj);

            // Assert
            Console.WriteLine($"LSD|Farm|LSD BMP|Stuctural BMP|Cost($)");

            foreach (var r in res)
            {
                Console.WriteLine($"{r.LsdOrParcelId}|{r.FarmId}|{r.LsdOrParcelBmp}|{r.StructuralBmp}|{r.Cost.ToString("C2")}");
            }

            Assert.Zero((int)(1000 * (expectCost - res.Select(o => o.Cost).Sum()))); // difference less than +-0.001 
        }

        [Test]
        public void LsdParcelBMPSummary_Parcel_AllBMPs_Existing_2001_2010_GetLsdParcelBMPSummaryDTOs()
        {
            // Assign
            int proj = this.GetProjectId("AllBMPs|Parcel_Existing_2001_2010_WM");

            decimal expectCost = 3384.41m; // correct = 3384.41

            // Act
            var res = ServicesTestDB.PSS.GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(proj);

            // Assert
            Console.WriteLine($"Parcel|Farm|Parcel BMP|Stuctural BMP|Cost($)");

            foreach (var r in res)
            {
                Console.WriteLine($"{r.LsdOrParcelId}|{r.FarmId}|{r.LsdOrParcelBmp}|{r.StructuralBmp}|{r.Cost.ToString("C2")}");
            }

            // Assert
            Assert.Zero((int)(1000 * (expectCost - res.Select(o => o.Cost).Sum()))); // difference less than +-0.001 
        }

        [Test]
        public void LsdParcelBMPSummary_LSD_AllBMPs_Conv_2001_2010_GetLsdParcelBMPSummaryDTOs()
        {
            // Assign
            int proj = this.GetProjectId("AllBMPs|LSD_Conv_2001_2010_WM");

            decimal expectCost = 4325.804m; // correct = 4325.804

            // Act
            var res = ServicesTestDB.PSS.GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(proj);

            // Assert
            Console.WriteLine($"LSD|Farm|LSD BMP|Stuctural BMP|Cost($)");

            foreach (var r in res)
            {
                Console.WriteLine($"{r.LsdOrParcelId}|{r.FarmId}|{r.LsdOrParcelBmp}|{r.StructuralBmp}|{r.Cost.ToString("C2")}");
            }

            Assert.Zero((int)(1000 * (expectCost - res.Select(o => o.Cost).Sum()))); // difference less than +-0.001 
        }

        [Test]
        public void LsdParcelBMPSummary_Parcel_AllBMPs_Conv_2001_2010_GetLsdParcelBMPSummaryDTOs()
        {
            // Assign
            int proj = this.GetProjectId("AllBMPs|Parcel_Conv_2001_2010_WM");

            decimal expectCost = 4177.843m; // correct = 4177.843

            // Act
            var res = ServicesTestDB.PSS.GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(proj);

            // Assert
            Console.WriteLine($"Parcel|Farm|Parcel BMP|Stuctural BMP|Cost($)");

            foreach (var r in res)
            {
                Console.WriteLine($"{r.LsdOrParcelId}|{r.FarmId}|{r.LsdOrParcelBmp}|{r.StructuralBmp}|{r.Cost.ToString("C2")}");
            }

            // Assert
            Assert.Zero((int)(1000 * (expectCost - res.Select(o => o.Cost).Sum()))); // difference less than +-0.001 
        }
        #endregion
    }
}