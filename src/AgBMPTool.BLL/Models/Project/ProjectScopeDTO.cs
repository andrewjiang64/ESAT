using AgBMPTool.DBModel.Model.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class ProjectScopeDTO
    {
        public ProjectScopeDTO() { }

        public ProjectScopeDTO(int id, string name, int scenarioResultSummarizationType)
        {
            Id = id;
            Name = name;
            ScenarioResultSummarizationTypeId = scenarioResultSummarizationType;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int ScenarioResultSummarizationTypeId { get; set; }
    }
}