using AgBMPTool.BLL.Models.Project;
using AgBMPTool.DBModel.Model.Scenario;
using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Overview;

namespace AgBMPTool.BLL.Services.Projects
{
    public interface IProjectSummaryService
    {
        #region *** Project Summary ***
        /// <summary>
        /// Get project summary by project list. ProjectSummaryDTO contains project info (ProjectDTO),
        /// List of BMPSummaryDTO (BMPType, Count, Area, Cost)
        /// and List of EffectivenessSummaryDTO (BMPEffectivenessType, Percentage change, Value change)
        /// </summary>
        /// <param name="projectDTOs">List of ProjectDTO</param>
        /// <returns>List of ProjectSummaryDTO</returns>
        List<ProjectSummaryDTO> GetProjectSummaryDTOs(List<ProjectDTO> projectDTOs);

        /// <summary>
        /// Get project BMP summary by project id.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>List of BMPSummaryDTO (BMPType, Count, Area, Cost)</returns>
        List<BMPSummaryDTO> GetProjectBMPSummaryDTOs(int projectId);

        /// <summary>
        /// Get project effectiveness summary by project id.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>List of EffectivenessSummaryDTO (BMPEffectivenessType, Percentage change, Value change)</returns>
        List<EffectivenessSummaryDTO> GetProjectEffectivenessSummaryDTOs(int projectId);

        /// <summary>
        /// Get project cost by project id.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>Cost in $</returns>
        decimal GetProjectCost(int projectId);

        /// <summary>
        /// Get project baseline BMP summary by project id.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>List of BMPSummaryDTO (BMPType, Count, Area, Cost)</returns>
        List<BMPSummaryDTO> GetProjectBaselineBMPSummaryDTOs(int projectId);

        /// <summary>
        /// Get project baseline effectiveness summary by project id.
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>List of EffectivenessSummaryDTO (BMPEffectivenessType, Percentage change, Value change)</returns>
        List<EffectivenessSummaryDTO> GetProjectBaselineEffectivenessSummaryDTOs(int projectId);
        #endregion

        #region *** BMP Scope and Intelligent Recommendation ***
        /// <summary>
        /// Get list of BMPGeometryCostEffectivenessDTO by project id and bmp type for *** BMP Scope and Intelligent Recommendation ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpTypeId">BMP type id</param>
        /// <returns>List of BMPGeometryEffectivenessDTO (LocationId, BMPTypeId, BMPTypeName, Geometry, IsSelectable, List<EffectivnessDTO>) </returns>
        List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId);

        /// <summary>
        /// Get list of BMPCostAllEffectivenessDTO by project id and bmp type for *** BMP Scope and Intelligent Recommendation ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpTypeId">BMP type id</param>
        /// <returns>List of BMPCostAllEffectivenessDTO (LocationId, BMPTypeId, BMPTypeName, IsSelectable, List<EffectivnessDTO>) </returns>
        List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId, int bmpTypeId);

        /// <summary>
        /// Get list of BMPCostAllEffectivenessDTO by project id (including all BMP types) for *** BMP Scope and Intelligent Recommendation ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpTypeId">BMP type id</param>
        /// <returns>List of BMPCostAllEffectivenessDTO (LocationId, BMPTypeId, BMPTypeName, IsSelectable, List<EffectivnessDTO>) </returns>
        List<BMPGeometryCostEffectivenessDTO> GetAllBMPGeometryCostEffectivenessDTOForBMPScopeAndIntelligentRecommendation(int projectId);
        #endregion

        #region *** BMP Selection and Overview ***
        /// <summary>
        /// Get list of BMPGeometryCostEffectivenessDTO by project id and bmp type for *** BMP Selection and Overview ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpTypeId">BMP type id</param>
        /// <returns>List of BMPGeometryEffectivenessDTO (LocationId, BMPTypeId, BMPTypeName, Geometry, IsSelectable, List<EffectivnessDTO>) </returns>
        List<BMPGeometryCostEffectivenessDTO> GetSingleBMPGeometryCostEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId);

        /// <summary>
        /// Get list of BMPCostAllEffectivenessDTO by project id and bmp type for *** BMP Selection and Overview ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="bmpTypeId">BMP type id</param>
        /// <returns>List of BMPCostAllEffectivenessDTO (LocationId, BMPTypeId, BMPTypeName, IsSelectable, List<EffectivnessDTO>) </returns>
        List<BMPCostAllEffectivenessDTO> GetSingleBMPCostAllEffectivenessDTOForBMPSelectionAndOverview(int projectId, int bmpTypeId);

        /// <summary>
        /// Get LSD or parcel level BMP summary for user selected BMPs for *** BMP Selection and Overview ***
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <returns>List of LsdParcelBMPSummaryDTO which contains 
        /// ProjectSpatialUnitTypeId - LSD or parcel level,
        /// LsdOrParcelId - LSD/Parcel id
        /// FarmId - Farm id
        /// LsdOrParcelBmp - String of current LSD/Parcel BMP summary
        /// StructuralBmp - String of current LSD/Parcel structural BMP summary
        /// Cost - Total cost of current LSD/Parcel BMPs</returns>
        List<LsdParcelBMPSummaryDTO> GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(int projectId);
        #endregion
    }
}
