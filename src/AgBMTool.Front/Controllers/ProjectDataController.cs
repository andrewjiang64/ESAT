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

namespace AgBMTool.Front.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/ProjectData")]
    [ApiController]
    public class ProjectDataController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IProjectDataService _IProjectDataService;
        private readonly IUtilitiesDataService _IUtilitiesDataService;


        public ProjectDataController(IUnitOfWork _iUnitOfWork, IProjectDataService IProjectDataService, IUtilitiesDataService IUtilitiesDataService)
        {
            this._uow = _iUnitOfWork;
            this._IProjectDataService = IProjectDataService;
            this._IUtilitiesDataService = IUtilitiesDataService;
        }

        // GET: api/ProjectData
        [HttpGet("GetProjectListByUserId/{userId:int}")]
        public List<ProjectListModel> GetProjectListByUserId(int userId)
        {
            List<ProjectListModel> projects = new List<ProjectListModel>();

            projects = _IProjectDataService.GetProjectListByUserId(userId).ToList().Select(x => ProjectListModel.Map(x)).ToList();

            return projects;
        }


        // GET: api/ProjectData/5
        [HttpGet("{id}", Name = "GetProjectTree")]
        public List<ProjectTree> Get(int id)
        {
            List<ProjectTree> treeList = new List<ProjectTree>();
            ProjectTree test = new ProjectTree();
            test.Id = -1;
            test.name = "Overview";
            test.typeId = 1;
            treeList.Add(test);
            test.iconclass = "overview-icon";

            test = new ProjectTree();
            test.Id = 0;
            test.name = "Projects";
            test.typeId = 2;
            test.iconclass = "projects-icon";
            treeList.Add(test);
            test.children = new List<ProjectTree>();

            var projects = GetProjectListByUserId(id);
            var projectbmptype = _IProjectDataService.getUserProjectBMPTypes(id);
            foreach (var p in projects)
            {
                ProjectTree project1 = new ProjectTree();
                project1.Id = p.Id;
                project1.name = p.Name;
                project1.projectName = p.Name;
                project1.typeId = 3;
                project1.iconclass = "project-icon";
                project1.projectId = p.Id;
                project1.children = new List<ProjectTree>();

                test.children.Add(project1);

                ProjectTree testBaseLine1 = new ProjectTree();
                testBaseLine1.Id = 2;
                testBaseLine1.projectName = p.Name;
                testBaseLine1.name = "Baseline information";
                testBaseLine1.typeId = 4;
                testBaseLine1.iconclass = "baseline-icon";
                testBaseLine1.projectId = p.Id;
                project1.children.Add(testBaseLine1);

                ProjectTree testBMPSCOP = new ProjectTree();
                testBMPSCOP.projectName = p.Name;
                testBMPSCOP.name = "BMP scope & intelligent recommendation";
                testBMPSCOP.iconclass = "bsair-icon";
                testBMPSCOP.Id = 10;
                testBMPSCOP.typeId = 5;
                testBMPSCOP.projectId = p.Id;
                testBMPSCOP.children = new List<ProjectTree>();
                testBMPSCOP.bmptypeIds = new List<int>();

                ProjectTree testBMPselection = new ProjectTree();
                testBMPselection.projectName = p.Name;
                testBMPselection.name = "BMP selection & overview";
                testBMPselection.iconclass = "bso-icon";
                testBMPselection.children = new List<ProjectTree>();
                testBMPselection.bmptypeIds = new List<int>();
                project1.children.Add(testBMPSCOP);
                project1.children.Add(testBMPselection);
                if (!projectbmptype.ContainsKey(p.Id))
                    continue;
               var bmptypes = projectbmptype[p.Id];
                if (bmptypes.Count == 0)
                    continue;
                testBMPSCOP.Id = bmptypes.ElementAt(0).Id;
                testBMPSCOP.typeId = 5;
                testBMPselection.typeId = 6;
                testBMPselection.projectId = p.Id;
                testBMPselection.bmptypeIds.AddRange(bmptypes.Select(x => x.Id).ToList());
                foreach (var bmptype in bmptypes) {
                    ProjectTree BMPType = new ProjectTree();
                    BMPType.bmptypeIds = new List<int>();
                    BMPType.projectName = p.Name;
                    BMPType.Id = bmptype.Id;
                    BMPType.typeId = 5;
                    BMPType.projectId = p.Id;
                    BMPType.name = bmptype.Name;
                    BMPType.iconclass = bmptype.Name + "-icon";
                    ProjectTree solutionbmptype = BMPType.Clone();
                    solutionbmptype.typeId = 6;
                    testBMPSCOP.children.Add(BMPType);
                    testBMPselection.children.Add(solutionbmptype);
                }
  
            }

            return treeList;
        }


        [HttpGet("GetBMPEffectivenessType")]
        public List<BMPEffectivenessTypeModel> GetBMPEffectivenessType()
        {
            List<BMPEffectivenessTypeModel> baselineInfo = _IUtilitiesDataService.GetBMPEffectivenessType().
                Where(x => !x.name.Contains("BMP cost")).Select(x => new BMPEffectivenessTypeModel(x)).ToList();

            return baselineInfo;
        }


        [HttpGet("GetProjectsBMPCostByEffectivenessTypeId/{userId}/{effectivenessTypeId}")]
        public List<ProjectsCostEffectivenessChartModel> GetProjectsBMPCostByEffectivenessTypeId(int userId, int effectivenessTypeId)
        {
            var projectsBMPCostEffectiveness = _IProjectDataService.GetProjectsBMPCostByEffectivenessTypeId(
                userId, effectivenessTypeId).Select(x => new ProjectsCostEffectivenessChartModel(x)).ToList();

            return projectsBMPCostEffectiveness;
        }

        // GET: api/ProjectData
        [HttpGet("GetSummarizationTypeList")]
        public List<DropdownDTO> GetSummarizationTypeList()
        {
            List<DropdownDTO> summarizationTypes = new List<DropdownDTO>();

            summarizationTypes = _IUtilitiesDataService.GetSummarizationTypeOptions()
                .Where(x => x.ItemId == (int)ScenarioResultSummarizationTypeEnum.Watershed || x.ItemId == (int)ScenarioResultSummarizationTypeEnum.Municipality).ToList();

            return summarizationTypes;
        }

        // GET: api/ProjectData
        [HttpGet("GetMunicipalitiesByUserId/{userId:int}/{userTypeId:int}")]
        public List<ProjectScopeDTO> GetMunicipalitiesByUserId(int userId, int userTypeId)
        {
            var projectsScope = _IProjectDataService.GetMunicipalitiesByUserId(userId, userTypeId);

            return projectsScope;
        }

        // GET: api/ProjectData
        [HttpGet("GetWatershedByMunicipalityId/{userId:int}/{userTypeId:int}/{municipalityId:int}")]
        public List<BaseItemDTO> GetWatershedByMunicipalityId(int userId, int userTypeId, int municipalityId)
        {
            var projectsScope = _IProjectDataService.GetWatershedsByMunicipality(userId, userTypeId, municipalityId);

            return projectsScope;
        }

        // GET: api/ProjectData
        [HttpGet("GetProjectSpatialUnitType")]
        public List<BaseItemDTO> GetProjectSpatialUnitType()
        {
            return _IProjectDataService.GetProjectSpatialUnitType();
        }

        // POST: api/ProjectData
        [HttpPost("saveproject/{userId:int}")]
        public void SaveProject(int userId, [FromBody] string data)
        {
            data = data.Replace("name", "Name").Replace("id", "Id").ToString();
            ProjectDTO project = JsonConvert.DeserializeObject<ProjectDTO>(data);

            _IProjectDataService.SaveProject(userId, project);
        }

        // DELETE
        [HttpDelete("DeleteProject/{projectId:int}")]
        public void DeleteProject(int projectId)
        {
            _IProjectDataService.DeleteProject(projectId);
        }

        // POST: api/ProjectData
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/ProjectData/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpGet("exportshapefile/{projectId:int}/{bmpTypeId:int}")]
        public IActionResult ExportShapefile(int projectId, int bmpTypeId)
        {
            var stream =  _IProjectDataService.ExportToShapefile(projectId, bmpTypeId); 

            if (stream == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            return File(stream, ".zip    application/zip, application/octet-stream"); // returns a FileStreamResult
        }
    }
}
