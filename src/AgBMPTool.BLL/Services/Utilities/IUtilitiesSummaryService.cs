using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Project;
using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Services.Utilities
{
    public interface IUtilitiesSummaryService
    {
        IntelligentRecommendationOptimizationDTO GetAllOptimizationBmpLocationsByProjectId(int projectId);

        IntelligentRecommendationOptimizationDTO GetSelectedOptimizationBmpLocationsByProjectId(int projectId);

        SolutionDTO GetSelectedSolutionBmpLocationsByProjectId(int projectId);

        SolutionDTO GetAllSolutionBmpLocationsByProjectId(int projectId);

        HashSet<int> GetAllModelComponentIdsBySubAreaIds(HashSet<int> subAreaIds);

        HashSet<int> GetSubAreaIds(int projectId, int userId, int userTypeId);

        HashSet<int> GetScenarioIdsBySubAreaMcIds(HashSet<int> subAreaMCIds, int scenarioTypeId);

        List<int> GetBMPTypeIdsByBMPCombinationTypeId(int bmpCombinationTypeId);

        HashSet<int> GetAllScenarioIdsByProjectId(int projectId, int scenTypeId);

        List<BMPCostAreaDTO> GetBMPCostAreaDTOs(
            OptimizationSolutionLocationTypeEnum locationType,
            HashSet<int> locationIds,
            HashSet<int> reachBMPMcIds,
            HashSet<int> scenIds,
            SolutionDTO solution);

        HashSet<int> GetSubAreaMCIds(IntelligentRecommendationOptimizationDTO bmpLocations, int projectSpatialUnitTypeId);

        /// <summary>
        /// Save solution to SolutionLegalSubDivisions, SolutionParcels, and SolutionModelComponents tables
        /// (Update IsSelected)
        /// </summary>
        /// <param name="solutionDTO">Solution DTO contains project id and list of unit solutions</param>
        /// <returns>List of ProjectSummaryDTO</returns>
        int SaveSolution(SolutionOptimizationBase solutionDTO, bool isFromOptimization);

        /// <summary>
        /// Save solution to SolutionLegalSubDivisions, SolutionParcels, and SolutionModelComponents tables
        /// (Update IsSelected)
        /// </summary>
        /// <param name="solutionDTO">Solution DTO contains project id and list of unit solutions</param>
        /// <returns>List of ProjectSummaryDTO</returns>
        int SaveOptimization(SolutionOptimizationBase optimizationDTO);

        List<ReachBMPGeometry> GetReachBMPByMCIds(HashSet<int> reachBMPMCIds, bool hasGeometry);
    }
}
