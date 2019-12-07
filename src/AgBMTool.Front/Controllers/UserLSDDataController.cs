using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgBMPTool.BLL.Services.Users;
using AgBMPTool.BLL.Models.Shared;

namespace AgBMTool.Front.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLSDDataController : ControllerBase
    {
        private IUserDataServices _iUserDataService;
        public UserLSDDataController(IUserDataServices _iUserDataService) {
            this._iUserDataService = _iUserDataService;
        }

        // GET: api/UserLSDData
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/UserLSDData/5
        [HttpGet("{id}", Name = "GetLSDData")]
        public List<MutiPolyGon> Get(int id)
        {
            return _iUserDataService.getUserLSD(1, -1, -1, -1);
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
