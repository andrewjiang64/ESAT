using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using AgBMPTool.BLL.Models.BaselineInformation;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Models.Utility;

namespace AgBMPTool.BLL.Services.Projects
{
    public interface IProjectDataService
    {
        List<ProjectDTO> GetProjectListByUserId(int userId);

        List<ScenarioModelResultTypeDTO> GetSubAreaModelResultType();

        SummaryTableDTO GetOverviewSummaryTable(int baselineId, int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId,
                    int locationFilter_SubwatershedId, int startYear, int endYear, int userId);

        List<ProjectsCostEffectivenessChartDTO> GetProjectsBMPCostByEffectivenessTypeId(int userId, int effectivenessTypeId);

        #region Location Filters

        List<ProjectScopeDTO> GetMunicipalitiesByUserId(int userId, int userTypeId);

        List<BaseItemDTO> GetWatershedsByMunicipality(int userId, int userTypeId, int municipalityId);

        List<BaseItemDTO> GetSubWatershedsByWatershedId(int userId, int userTypeId, int municipalityId, int watershedId);

        List<ProjectScopeDTO> GetProjectMunicipalitiesByProjectId(int projectId, int userId, int userTypeId);

        List<BaseItemDTO> GetProjectWatershedsByMunicipality(int projectId, int userId, int userTypeId, int municipalityId);

        #endregion

        StartAndEndRangeDTO GetStartAndEndYearForOverviewByUserIdAndUserType(int userId, int scenarioTypeId);

        StartAndEndRangeDTO GetStartAndEndYearForAddProjectByUserIdAndUserType(int userId);

        List<BaseItemDTO> GetProjectSpatialUnitType();

        void SaveProject(int userId, ProjectDTO project);

        void DeleteProject(int projectId);

        List<BaselineBMPSummaryDTO> GetBaselineBMPSummaryData(int userId, int projectId);

        List<BMPEffectivenessSummaryDTO> GetBaselineBMPEffectivenessData(int userId, int projectId);

        SummaryTableDTO GetBaselineSummaryTable(int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId,
            int locationFilter_SubwatershedId, int projectId, int userId);

        /// <summary>
        /// Get optimization type list
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        List<DropdownDTO> GetOptimizationTypeList(int projectId);

        Dictionary<int, List<BMPTypeDTO>> getUserProjectBMPTypes(int userId);

        modelCompoentGeometryLayer getBMPTypeGeometries(int projectId, int userId,
                int bmptypeId, Boolean isOptimization);

        bool CheckIfBMPsSelectedinProject(int projectId, int bmptypeId, string locationType);

        List<modelCompoentGeometryLayer> getBMPTypeListGeometries(int projectId, int userId, List<int> bmptypeIds, Boolean isoptimization);
        SummaryTableDTO GetBMPScopeSummaryGridData(int projectId, int bmpTypeId);
        SummaryTableDTO GetSingleBMPCostGridData(int projectId, int bmpTypeId);
        string GetBMPDefaultSelectionColorByBMPType(string bmpType);

        /// <summary>
        /// Get location type
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpTypeId"></param>
        /// <returns></returns>
        string GetLocationTypeByProjectIdAndBMPTypeId(int projectId, int bmpTypeId);

        /// <summary>
        /// Get ScenarioType option (Conventional & Existing) by project Id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List of Scenario Type</returns>
        OptimizationDTO GetOptimizationByProjectId(int projectId, List<BMPEffectivenessTypeDTO> bmpEffectivenessTypes);

        /// <summary>
        /// Get Optimization Constraint Value Types
        /// </summary>
        /// <returns></returns>
        List<DropdownDTO> GetOptimizationConstraintValueTypeList();

        /// <summary>
        /// Run BMPScope and Intelligence
        /// </summary>
        /// <param name="projectId"></param>
        bool RunIntelligentRecommendation(int projectId);

        /// <summary>
        /// Save optimization by project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="OptimizationTypeId"></param>
        void SaveOptimizationType(int projectId, int OptimizationTypeId);

        /// <summary>
        /// Save Budget
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="budget"></param>
        void SaveBudget(int projectId, decimal budget);

        List<MutiPolyGon> getProjectWatershedGeometry(int projectId);
        List<MutiPolyGon> getProjectMunicipilitiesGeometry(int projectId);
        List<PolyLine> getProjectReachesGeometry(int projectId);

        /// <summary>
        /// export bmp selection to shapefile
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpTypeId"></param>
        /// <returns></returns>
        System.IO.Stream ExportToShapefile(int projectId, int bmpTypeId);

        List<BMPTypeDTO> getProjectDefaultBMPLocations(int projectId);

        /// <summary>
        /// Save Weight for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationWeights"></param>
        void SaveWeight(int projectId, List<OptimizationWeightDTO> optimizationWeightDTO);

        /// <summary>
        /// Save Constraint for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationConstraints"></param>
        void SaveConstraint(int projectId, OptimizationConstraintDTO optimizationConstraintDTO);

        /// <summary>
        /// Delete Constraint for project in BMP Scope and Intelligent
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bmpEffectivenessTypeId"></param>
        void DeleteConstraint(int projectId, int bmpEffectivenessTypeId);

        /// <summary>
        /// Save optimization LSD
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationLSD"></param>
        void SaveLegalSubDivisions(int projectId, LegalSubDivisionDTO subDivisionDTOLSDDTO, Boolean isOptimization);

        /// <summary>
        /// Save optimization parcels
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationParcels"></param>
        void SaveParcels(int projectId, ParcelDTO parcelDTO, Boolean isOptimization);

        /// <summary>
        /// Save optimization Model Component
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="optimizationModelComponent"></param>
        void SaveModelComponents(int projectId, ModelComponentDTO modelComponentDTO, Boolean isOptimization);


        /// <summary>
        /// Apply quick selection used in intelligen recommedation and manual selection.
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="bmptypeId">The bmp type id</param>
        /// <param name="isOptimization">True = optimization, False = solution</param>
        /// <param name="isSelected">True = Select All, False = Deselect All</param>
        /// <param name="municipalityId">Municipality Id</param>
        /// <param name="watershedId">Watershed Id</param>
        /// <param name="subwatershedId">Subwatershed Id</param>
        /// <returns>The location ids affected by the quick selection. </returns>
        List<int> ApplyQuickSelection(int projectId, int bmptypeId, Boolean isOptimization, Boolean isSelected, int municipalityId, int watershedId, int subwatershedId);
        List<BaseItemDTO> GetInverstorList(int userId, int projectId, int bmptypeId);

        Dictionary<String, GeometryStyle> getBMPTypeGeometryLayerDic(List<int> bmptypeIds);

        string GetProjectSpatialUnitTypeName(int projectId);
    }
}