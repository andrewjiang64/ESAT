using AgBMPTool.DBModel.Model.Scenario;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public List<ProjectScopeDTO> Scope { get; set; }
        public int ScenarioTypeId { get; set; }
        public int SpatialUnitId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
	    public ScenarioType ScenarioType { get; set; }
    }
}
