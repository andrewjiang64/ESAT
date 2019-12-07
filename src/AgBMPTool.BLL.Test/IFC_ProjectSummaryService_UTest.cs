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

namespace AgBMPTool.BLL.IFC.Test
{
    class IFC_ProjectSummaryService_UTest
    {
        private readonly int TestUserId = 1;

        private int GetProjectIdForSolution(string projectName)
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

                SolutionDTO o = token[0] == "AllBMPs" ? ServicesIFC.USS.GetAllSolutionBmpLocationsByProjectId(pId)
                    : TestDBSolutionDTOs[token[0]];

                o.ProjectId = pId;
                ServicesIFC.USS.SaveSolution(o, false);

                return pId;
            }
        }        

        private readonly Dictionary<string, ProjectDTO> TestDBProjectDTOs = new Dictionary<string, ProjectDTO>
        {
            ["AllBMPs|LSD_Conv_1978_2017_WM"] = new ProjectDTO
            {
                Name = "AllBMPs|LSD_Conv_1978_2017_WM",
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
        };

        private readonly Dictionary<string, SolutionDTO> TestDBSolutionDTOs = new Dictionary<string, SolutionDTO>
        {
            ["AllBMPs"] = new SolutionDTO { }, // use default solution
        };

        [Test]
        public void Execution_AllBMPs_LSD_Conv_1978_2017_WM_GetProjectBMPSummaries()
        {
            // Assign
            int projectId1 = this.GetProjectIdForSolution("AllBMPs|LSD_Conv_1978_2017_WM");

            // Act
            var res1 = ServicesIFC.PSS.GetProjectBMPSummaryDTOs(projectId1);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Cost_AllBMPs_LSD_Conv_1978_2017_WM_GetProjectBMPSummaries_ExecutionPrint()
        {
            // Assign
            int projectId1 = this.GetProjectIdForSolution("AllBMPs|LSD_Conv_1978_2017_WM");

            decimal expectCost = (decimal)104460.795474; // 104460.795474

            // Act
            var res = ServicesIFC.PSS.GetProjectBMPSummaryDTOs(projectId1);

            // Assert
            Console.WriteLine($"BMP|Count|Area(ha)|Cost($)");

            foreach (var bmp in res)
            {
                Console.WriteLine($"{bmp.BMPTypeName}|{bmp.ModelComponentCount}|{bmp.TotalArea}|{bmp.TotalCost}");
            }
            Assert.Zero((int)((expectCost - res.Select(o => o.TotalCost).Sum()))); // difference less than +-0 
        }
    }
}
