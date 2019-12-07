using AgBMPTool.BLL.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Project
{
    public class ProjectListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }

        public static ProjectListModel Map(ProjectDTO project)
        {
            var projects = new ProjectListModel();

            projects.Id = project.Id;
            projects.Name = project.Name;
            projects.CreatedDate = project.CreatedDate;
            projects.Description = project.Description;
        
            return projects;
        }
    }
}
