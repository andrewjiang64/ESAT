using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Models.Utility;
using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Utilities;
using AgBMTool.Front.Model.Common;
using AgBMTool.Front.Model.Project;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AgBMTool.Front.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/BMPScopeAndIntelligenceData")]
    [ApiController]
    public class BMPScopeAndIntelligenceDataController : Controller
    {
        private readonly IProjectDataService _IProjectDataService;
        private readonly IUtilitiesDataService _IUtilitiesDataService;
        private readonly IProjectSummaryService _iProjectSummaryService;

        public BMPScopeAndIntelligenceDataController(IProjectDataService IProjectDataService, IUtilitiesDataService iUtilitiesDataService, IProjectSummaryService iProjectSummaryService)
        {
            this._IProjectDataService = IProjectDataService;
            this._IUtilitiesDataService = iUtilitiesDataService;
            this._iProjectSummaryService = iProjectSummaryService;
        }

        // Pos: api/RunIntelligentRecommendation
        [HttpGet("CheckIfBMPsSelectedinProject/{projectId:int}/{bmptypeId:int}/{locationType}")]
        public bool CheckIfBMPsSelectedinProject(int projectId, int bmptypeId, string locationType)
        {
            return _IProjectDataService.CheckIfBMPsSelectedinProject(projectId, bmptypeId, locationType);
        }

        /// <summary>
        /// Get Municipalties as per userType 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userTypeId"></param>
        /// <param name="summarizationTypeId"></param>
        /// <returns></returns>
        [HttpGet("GetProjectMunicipalitiesByProjectId/{projectId:int}/{userId:int}/{userTypeId:int}")]
        public List<ProjectScopeDTO> GetProjectMunicipalitiesByProjectId(int projectId, int userId, int userTypeId)
        {
            var projectsScope = _IProjectDataService.GetProjectMunicipalitiesByProjectId(projectId, userId, userTypeId);

            return projectsScope;
        }


        [HttpGet("GetProjectWatershedsByMunicipality/{projectId:int}/{userId:int}/{userTypeId:int}/{municipalityId:int}")]
        public List<BaseItemDTO> GetProjectWatershedsByMunicipality(int projectId, int userId, int userTypeId, int municipalityId)
        {
            return _IProjectDataService.GetProjectWatershedsByMunicipality(projectId, userId, userTypeId, municipalityId);
        }

        // GET: api/BMPScopeAndIntelligenceData
        [HttpGet("GetOptimizationTypeList/{projectId:int}")]
        public List<DropdownDTO> GetOptimizationTypeList(int projectId)
        {
            return _IProjectDataService.GetOptimizationTypeList(projectId);
        }

        [HttpGet("GetSubAreaModelResultType")]
        public List<ScenarioModelResultTypeDTO> GetSubAreaModelResultType()
        {
            var scenarioTypes = _IProjectDataService.GetSubAreaModelResultType().ToList();
            foreach (var scenarioType in scenarioTypes)
            {
                scenarioType.Name = scenarioType.Name + " (" + scenarioType.UnitSymbol + ")";
            }
            return scenarioTypes;
        }


        [HttpGet("GetScenarioModelResultVariableTypes")]
        public List<DropdownDTO> GetScenarioModelResultVariableTypes()
        {
            var scenarioTypes = _IUtilitiesDataService.GetScenarioModelResultVariableTypes().ToList();

            return scenarioTypes;
        }

        [HttpGet("GetBMPDefaultSelectionColorByBMPType/{bmpType}")]
        public string GetBMPDefaultSelectionColorByBMPType(string bmpType)
        {
            var defaultColor = _IProjectDataService.GetBMPDefaultSelectionColorByBMPType(bmpType);

            return defaultColor;
        }

        [HttpGet("GetLocationTypeByProjectIdAndBMPTypeId/{projectId}/{bmpTypeId}")]
        public string GetLocationTypeByProjectIdAndBMPTypeId(int projectId, int bmpTypeId)
        {
            var locationType = _IProjectDataService.GetLocationTypeByProjectIdAndBMPTypeId(projectId, bmpTypeId);

            return locationType;
        }

        [HttpGet("GetBMPEffectivenessLocationType")]
        public List<DropdownDTO> GetBMPEffectivenessLocationType()
        {
            var scenarioTypes = _IUtilitiesDataService.GetBMPEffectivenessLocationType().ToList();

            return scenarioTypes;
        }

        [HttpGet("GetBMPEffectivenessTypeByProjectId/{projectId:int}")]
        public OptimizationDTO GetBMPEffectivenessTypeByProjectId(int projectId)
        {
            List<BMPEffectivenessTypeDTO> bmpEffectivenessTypes = _IUtilitiesDataService.GetBMPEffectivenessType().
                Where(x => !x.name.Contains("BMP cost")).ToList();

            var optimization = _IProjectDataService.GetOptimizationByProjectId(projectId, bmpEffectivenessTypes);

            return optimization;
        }

        // GET: api/BMPScopeAndIntelligenceData
        [HttpGet("GetOptimizationConstraintValueTypeList")]
        public List<DropdownDTO> GetOptimizationConstraintValueTypeList()
        {
            return _IProjectDataService.GetOptimizationConstraintValueTypeList();
        }

        // GET: api/SaveBudget
        [HttpGet("SaveOptimizationType/{projectId:int}/{optimizationTypeId:int}")]
        public void SaveOptimizationType(int projectId, int optimizationTypeId)
        {
            _IProjectDataService.SaveOptimizationType(projectId, optimizationTypeId);
        }

        // GET: api/SaveBudget
        [HttpGet("SaveBudget/{projectId:int}/{budget}")]
        public void SaveBudget(int projectId, decimal budget)
        {
            _IProjectDataService.SaveBudget(projectId, budget);
        }

        // Pos: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpPost("SaveWeight/{projectId:int}")]
        public void SaveWeight(int projectId, [FromBody] string optimizationWeightsStr)
        {
            var optimizationWeights = JsonConvert.DeserializeObject<List<OptimizationWeightDTO>>(optimizationWeightsStr);

            _IProjectDataService.SaveWeight(projectId, optimizationWeights);
        }

        // Pos: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpPost("SaveConstraint/{projectId:int}")]
        public void SaveConstraint(int projectId, [FromBody] string optimizationConstraintsStr)
        {
            var optimizationConstraints = JsonConvert.DeserializeObject<OptimizationConstraintDTO>(optimizationConstraintsStr);

            _IProjectDataService.SaveConstraint(projectId, optimizationConstraints);
        }

        // GET: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpGet("DeleteConstraint/{projectId:int}/{bmpEffectivenessTypeId}")]
        public void DeleteConstraint(int projectId, int bmpEffectivenessTypeId)
        {
            _IProjectDataService.DeleteConstraint(projectId, bmpEffectivenessTypeId);
        }

        // Pos: api/RunIntelligentRecommendation
        [HttpGet("RunIntelligentRecommendation/{projectId:int}")]
        public bool RunIntelligentRecommendation(int projectId)
        {
            return _IProjectDataService.RunIntelligentRecommendation(projectId);
        }

        /// <summary>
        /// Get Overview page summary table data
        /// </summary>
        /// <param name="summerizationLevelId"></param>
        /// <param name="locationFilter_MunicipalityId"></param>
        /// <param name="locationFilter_WatershedId"></param>
        /// <param name="locationFilter_SubwatershedId"></param>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GetSummaryGridData/{projectId}/{bmpTypeId}")]
        public SummaryTableModel GetSummaryGridData(int projectId, int bmpTypeId)
        {
            SummaryTableModel summaryTable = new SummaryTableModel();

            var summaryTableDTO = _IProjectDataService.GetBMPScopeSummaryGridData(projectId, bmpTypeId);

            summaryTable.SummaryTableColumns = summaryTableDTO.SummaryTableColumns;
            summaryTable.SummaryTableData = summaryTableDTO.SummaryTableData;
            return summaryTable;
        }

        // Pos: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpPost("SaveLegalSubDivisions/{projectId:int}/{isOptimization}")]
        public void SaveLegalSubDivisions(int projectId, Boolean isOptimization, [FromBody] string lsdStr)
        {
            var optimizationLSD = JsonConvert.DeserializeObject<LegalSubDivisionDTO>(lsdStr);

            _IProjectDataService.SaveLegalSubDivisions(projectId, optimizationLSD, isOptimization);
        }

        // Pos: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpPost("SaveParcels/{projectId:int}/{isOptimization}")]
        public void SaveParcels(int projectId, Boolean isOptimization, [FromBody] string parcelsStr)
        {
            var optimizationParcels = JsonConvert.DeserializeObject<ParcelDTO>(parcelsStr);

            _IProjectDataService.SaveParcels(projectId, optimizationParcels, isOptimization);
        }

        // Pos: api/SaveConstraintAndWeightRunIntelligentRecommendation
        [HttpPost("SaveModelComponents/{projectId:int}/{isOptimization}")]
        public void SaveModelComponents(int projectId, Boolean isOptimization, [FromBody] string modelComponentsStr)
        {
            var optimizationModelComponents = JsonConvert.DeserializeObject<ModelComponentDTO>(modelComponentsStr);

            _IProjectDataService.SaveModelComponents(projectId, optimizationModelComponents, isOptimization);
        }

        [HttpGet("ApplyQuickSelection/{projectId}/{bmptypeId}/{isOptimization}/{isSelected}/{municipalityId}/{watershedId}/{subwatershedId}")]
        public List<int> ApplyQuickSelection(int projectId, int bmptypeId, Boolean isOptimization, Boolean isSelected, int municipalityId, int watershedId, int subwatershedId)
        {
            return _IProjectDataService.ApplyQuickSelection(projectId, bmptypeId, isOptimization, isSelected, municipalityId, watershedId, subwatershedId);
        }

        [HttpGet("GetSingleBMPCostData/{projectId}/{bmpTypeId}")]
        public SummaryTableModel GetSingleBMPCostData(int projectId, int bmpTypeId)
        {
            SummaryTableModel summaryTable = new SummaryTableModel();

            var summaryTableDTO = _IProjectDataService.GetSingleBMPCostGridData(projectId, bmpTypeId);

            summaryTable.SummaryTableColumns = summaryTableDTO.SummaryTableColumns;
            summaryTable.SummaryTableData = summaryTableDTO.SummaryTableData;
            return summaryTable;
        }

        // GET: api/ProjectData
        [HttpGet("GetProjectBMPSummaryData/{projectId:int}")]
        public List<BMPSummaryDTO> GetProjectBMPSummaryData(int projectId)
        {
            return _iProjectSummaryService.GetProjectBMPSummaryDTOs(projectId);
        }
        [HttpGet("getProjectEffectivenessSummary/{projectId:int}")]
        public List<EffectivenessSummaryDTO> getProjectEffectivenessSummary(int projectId)
        {
            return _iProjectSummaryService.GetProjectEffectivenessSummaryDTOs(projectId);
        }

        [HttpGet("getLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview/{projectId:int}")]
        public FieldBMPSummaryModel getLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(int projectId)
        {
            var ret = new FieldBMPSummaryModel();
            ret.ProjectSpatialUnitType = _IProjectDataService.GetProjectSpatialUnitTypeName(projectId);
            ret.Summary = _iProjectSummaryService.GetLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(projectId);
            return ret;
        }

        // GET: api/ProjectData
        [HttpGet("GetInvestorList/{userId:int}/{projectId:int}/{bmpTypeId:int}")]
        public List<BaseItemDTO> GetInvestorList(int userId,int projectId, int bmpTypeId)
        {
            return _IProjectDataService.GetInverstorList(userId, projectId, bmpTypeId);
        }
    }
}