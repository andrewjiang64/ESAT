using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.BLL.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AgBMPTool.BLL.Services.Projects;
using Newtonsoft.Json;

namespace AgBMTool.Front.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/{controller}/{action}")]
    [ApiController]
    public class UserGeometryDataController : ControllerBase
    {
        private IUserDataServices _iUserDataService;
        private IProjectDataService _iProjectDataService;
        public UserGeometryDataController(IUserDataServices _iUserDataService, IProjectDataService _iProjectDataService) {
            this._iUserDataService = _iUserDataService;
            this._iProjectDataService = _iProjectDataService;
        }

        // GET: api/UserLSDData
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getReachData")]
        public List<PolyLine> getReachData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
            return _iUserDataService.getUserReach(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getLSDData")]
        public List<MutiPolyGon> getLSDData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
            return _iUserDataService.getUserLSD(id, municipalityId, waterShedId,subWaterShedId);
        }

        
        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getParcelData")]
        public List<MutiPolyGon> getParcelData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
           return _iUserDataService.getUserParcel(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getFarmData")]
        public List<MutiPolyGon> getFarmData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
            return _iUserDataService.getUserFarm(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getMunicipalitiesData")]
        public List<MutiPolyGon> getMunicipalitiesData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
            return _iUserDataService.getUserMunicipalities(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getUserWaterShedData")]
        public List<MutiPolyGon> getUserWaterShedData(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
            return _iUserDataService.getUserWaterShed(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{id}/{municipalityId}/{waterShedId}/{subWaterShedId}", Name = "GetLSDDataAction")]
        [ActionName("getUserSubWaterShed")]
        public List<MutiPolyGon> getUserSubWaterShed(int id, int municipalityId, int waterShedId, int subWaterShedId)
        {
           return _iUserDataService.getUserSubWaterShed(id, municipalityId, waterShedId, subWaterShedId);
        }

        [HttpGet("{userId}/{projectId}/{bmptypeId}/{isOptimization}", Name = "getUserBMPGeomtries")]
        [ActionName("getUserBMPGeomtries")]
        public modelCompoentGeometryLayer getUserBMPGeomtries( int projectId, int userId,
       int bmptypeId, Boolean isOptimization)
        {
            return _iProjectDataService.getBMPTypeGeometries(projectId,userId, bmptypeId, isOptimization);
        }

        [HttpGet("{projectId}/{userId}/{bmptypeIdstring}/{isOptimization}", Name = "getUserBMPTyeListGeomtries")]
        [ActionName("getUserBMPTyeListGeomtries")]
        public List<modelCompoentGeometryLayer> getUserBMPTyeListGeomtries(int projectId, int userId,
       string bmptypeIdstring, Boolean isOptimization)
        {
            String st = JsonConvert.DeserializeObject<String>(bmptypeIdstring,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var bmptypeIds = st.Split(',').Select(x => Int32.Parse(x)).ToList();
            return _iProjectDataService.getBMPTypeListGeometries(projectId, userId, bmptypeIds, isOptimization);
        }

        [HttpGet("{projectId}", Name = "getProjectWaterShedsGeometry")]
        [ActionName("getProjectWaterShedsGeometry")]
        public List<MutiPolyGon> getProjectWaterShedsGeometry(int projectId)
        {
            return _iProjectDataService.getProjectWatershedGeometry(projectId);
        }

        [HttpGet("{projectId}", Name = "getProjectMunicipilitiesGeometry")]
        [ActionName("getProjectMunicipilitiesGeometry")]
        public List<MutiPolyGon> getProjectMunicipilitiesGeometry(int projectId)
        {
            return _iProjectDataService.getProjectMunicipilitiesGeometry(projectId);
        }

        [HttpGet("{projectId}", Name = "getProjectReachesGeometry")]
        [ActionName("getProjectReachesGeometry")]
        public List<PolyLine> getProjectReachesGeometry(int projectId)
        {
            return _iProjectDataService.getProjectReachesGeometry(projectId);
        }

        [HttpGet("{bmptypeIdsstr}", Name = "getBMPTypeGeometryLayerDic")]
        [ActionName("getBMPTypeGeometryLayerDic")]
        public Dictionary<String, GeometryStyle> getBMPTypeGeometryLayerDic(string bmptypeIdsstr)
        {
            String st = JsonConvert.DeserializeObject<String>(bmptypeIdsstr,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var bmptypeIds = st.Split(',').Select(x => Int32.Parse(x)).ToList();
            return _iProjectDataService.getBMPTypeGeometryLayerDic(bmptypeIds);
        }

        [HttpGet("{userId}", Name = "GetUserCenterPoint")]
        [ActionName("getUserCenterPoint")]
        public double[] getUserCenterPoint(int userId)
        {
            return _iUserDataService.getUserMapCenter(userId);
        }

        [HttpGet("{userId}", Name = "GetUserExtent")]
        [ActionName("getUserExtent")]
        public ExtentModel getUserExtent(int userId)
        {
            return _iUserDataService.getUserExtent(userId);
        }

        // POST: api/UserLSDData
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/UserLSDData/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
