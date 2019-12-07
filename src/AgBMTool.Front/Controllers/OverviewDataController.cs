using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AgBMPTool.BLL.Models.Overview;
using AgBMPTool.BLL.Models.Project;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Models.Utility;
using AgBMPTool.BLL.Services.Projects;
using AgBMPTool.BLL.Services.Utilities;
using AgBMTool.DBL.Interface;
using AgBMTool.Front.Model.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgBMTool.Front.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/OverviewData")]
    [ApiController]
    public class OverviewDataController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IProjectDataService _IProjectDataService;
        private readonly IUtilitiesDataService _IUtilitiesDataService;

        public OverviewDataController(IUnitOfWork _iUnitOfWork, IProjectDataService IProjectDataService, IUtilitiesDataService IUtilitiesDataService)
        {
            this._uow = _iUnitOfWork;
            this._IProjectDataService = IProjectDataService;
            this._IUtilitiesDataService = IUtilitiesDataService;
        }

        [HttpGet("GetBaseLineOptions")]
        public List<DropdownDTO> GetBaseLineOptions()
        {
            List<DropdownDTO> baselineInfo = _IUtilitiesDataService.GetScenarioTypeOptions().ToList();

            return baselineInfo;
        }

        #region Location filter Dropdowns

        /// <summary>
        /// Get Municipalties as per userType 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userTypeId"></param>
        /// <param name="summarizationTypeId"></param>
        /// <returns></returns>
        [HttpGet("GetMunicipalitiesByUserId/{userId:int}/{userTypeId:int}")]
        public List<ProjectScopeDTO> GetMunicipalitiesByUserId(int userId, int userTypeId)
        {
            var projectsScope = _IProjectDataService.GetMunicipalitiesByUserId(userId, userTypeId);

            return projectsScope;
        }


        [HttpGet("GetWatershedsByMunicipality/{userId:int}/{userTypeId:int}/{municipalityId:int}")]
        public List<BaseItemDTO> GetWatershedsByMunicipality(int userId, int userTypeId, int municipalityId)
        {
            return _IProjectDataService.GetWatershedsByMunicipality(userId, userTypeId, municipalityId);
        }


        [HttpGet("GetSubWatershedsByWatershedId/{userId:int}/{userTypeId:int}/{municipalityId:int}/{watershedId:int}")]
        public List<BaseItemDTO> GetSubWatershedsByWatershedId(int userId, int userTypeId, int municipalityId, int watershedId)
        {
            return _IProjectDataService.GetSubWatershedsByWatershedId(userId, userTypeId, municipalityId, watershedId);
        }

        #endregion

        /// <summary>
        /// Get Start And End Year For Add Project By UserId And UserType
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        [HttpGet("GetStartAndEndYearByUserIdAndUserType/{userId:int}/{userType:int}/{scenarioTypeId:int}/{isAddProject:bool}")]
        public StartAndEndYearModel GetStartAndEndYearByUserIdAndUserType(int userId, int userType, int scenarioTypeId, bool isAddProject)
        {
            var modelRange = new StartAndEndYearModel();
            if (isAddProject)
            {
                var range = _IProjectDataService.GetStartAndEndYearForAddProjectByUserIdAndUserType(userId);

                modelRange.StartYear = range.StartYear;
                modelRange.EndYear = range.EndYear;
            }
            else
            {
                var range = _IProjectDataService.GetStartAndEndYearForOverviewByUserIdAndUserType(userId, scenarioTypeId);

                modelRange.StartYear = range.StartYear;
                modelRange.EndYear = range.EndYear;
            }
            return modelRange;
        }

        /// <summary>
        /// Get summarizationTypes (LSD, Parcel, Farm, Municipality, Watershed, Subwatershed)
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetScenarioResultSummarizationType")]
        public List<DropdownDTO> GetScenarioResultSummarizationType()
        {
            List<DropdownDTO> summarizationTypes = _IUtilitiesDataService.GetSummarizationTypeOptions().ToList();

            return summarizationTypes;
        }


        /// <summary>
        /// Get Overview page summary table data
        /// </summary>
        /// <param name="baselineId"></param>
        /// <param name="summerizationLevelId"></param>
        /// <param name="locationFilter_MunicipalityId"></param>
        /// <param name="locationFilter_WatershedId"></param>
        /// <param name="locationFilter_SubwatershedId"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GetOverViewSummaryResultByParameters/{baselineId}/{summerizationLevelId}/{locationFilter_MunicipalityId}/{locationFilter_WatershedId}/{locationFilter_SubwatershedId}/{startYear}/{endYear}/{userId}")]
        public SummaryTableModel GetOverviewSummaryResultByParameters(int baselineId, int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId, int locationFilter_SubwatershedId, int startYear, int endYear, int userId)
        {
            SummaryTableModel summaryTable = new SummaryTableModel();

            var summaryTableDTO = _IProjectDataService.GetOverviewSummaryTable(baselineId, summerizationLevelId, locationFilter_MunicipalityId, locationFilter_WatershedId, locationFilter_SubwatershedId, startYear, endYear, userId);


            summaryTable.SummaryTableColumns = summaryTableDTO.SummaryTableColumns;
            summaryTable.SummaryTableData = summaryTableDTO.SummaryTableData;
            return summaryTable;
        }
    }
}
