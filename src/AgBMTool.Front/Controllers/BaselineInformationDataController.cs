using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgBMTool.Front.Model;
using AgBMPTool.BLL.Services.Projects;
using AgBMTool.Front.Model.Project;
using AgBMTool.DBL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AgBMPTool.BLL.Services.Utilities;
using AgBMTool.Front.Model.Common;
using AgBMPTool.BLL.Models.Shared;
using AgBMPTool.BLL.Models.Project;
using Newtonsoft.Json;
using AgBMPTool.BLL.Models.Utility;
using static AgBMPTool.BLL.Enumerators.Enumerators;
using AgBMPTool.BLL.Models.BaselineInformation;

namespace AgBMTool.Front.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/BaselineInformationData")]
    [ApiController]
    public class BaselineInformationDataController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IProjectDataService _IProjectDataService;
       
        private readonly IUtilitiesDataService _IUtilitiesDataService;


        public BaselineInformationDataController(IUnitOfWork _iUnitOfWork, IProjectDataService IProjectDataService,IUtilitiesDataService IUtilitiesDataService)
        {
            this._uow = _iUnitOfWork;
            this._IProjectDataService = IProjectDataService;
            this._IUtilitiesDataService = IUtilitiesDataService;
        }

        // GET: api/BaselineInformationData
        [HttpGet("GetSummarizationTypeList")]
        public List<DropdownDTO> GetSummarizationTypeList()
        {
            List<DropdownDTO> baselineInfo = _IUtilitiesDataService.GetSummarizationTypeOptions().ToList();

            return baselineInfo;
        }

        // GET: api/ProjectData
        [HttpGet("GetWatershedsByMunicipality/{userId:int}/{userTypeId:int}/{municipalityId:int}")]
        public List<BaseItemDTO> GetWatershedsByMunicipality(int userId, int userTypeId, int municipalityId)
        {
            return _IProjectDataService.GetWatershedsByMunicipality(userId, userTypeId, municipalityId);
        }

        // GET: api/ProjectData
        [HttpGet("GetSubWatershedsByWatershedId/{userId:int}/{userTypeId:int}/{municipalityId:int}/{watershedId:int}")]
        public List<BaseItemDTO> GetSubWatershedsByWatershedId(int userId, int userTypeId, int municipalityId, int watershedId)
        {
            return _IProjectDataService.GetSubWatershedsByWatershedId(userId, userTypeId, municipalityId, watershedId);
        }

        // GET: api/ProjectData
        [HttpGet("GetBaselineBMPSummaryData/{userId:int}/{projectId:int}")]
        public List<BaselineBMPSummaryDTO> GetBaselineBMPSummaryData(int userId, int projectId)
        {
            return _IProjectDataService.GetBaselineBMPSummaryData(userId, projectId);
        }

        
        

        // GET: api/ProjectData
        [HttpGet("GetBaselineBMPEffectivenessData/{userId:int}/{projectId:int}")]
        public List<BMPEffectivenessSummaryDTO> GetBaselineBMPEffectivenessData(int userId, int projectId)
        {
            return _IProjectDataService.GetBaselineBMPEffectivenessData(userId, projectId);
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
        [HttpGet("GetSummaryGridData/{summerizationLevelId}/{locationFilter_MunicipalityId}/{locationFilter_WatershedId}/{locationFilter_SubwatershedId}/{projectId}/{userId}")]
        public SummaryTableModel GetSummaryGridData(int summerizationLevelId, int locationFilter_MunicipalityId, int locationFilter_WatershedId, 
            int locationFilter_SubwatershedId, int projectId, int userId)
        {
            SummaryTableModel summaryTable = new SummaryTableModel();

            var summaryTableDTO = _IProjectDataService.GetBaselineSummaryTable(summerizationLevelId, locationFilter_MunicipalityId, locationFilter_WatershedId, 
                locationFilter_SubwatershedId, projectId, userId);

            summaryTable.SummaryTableColumns = summaryTableDTO.SummaryTableColumns;
            summaryTable.SummaryTableData = summaryTableDTO.SummaryTableData;
            return summaryTable;
        }
    }
}
