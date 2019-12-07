using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgBMPTool.BLL.Models.Shared;

namespace AgBMTool.Front.Model.Project
{
    public class ProjectTree
    {
        public int Id { get; set; }
        public String name { get; set; }
        public int parentId { get; set; }
        public int typeId { get; set; }
        public string iconclass { get; set; }
        public int projectId { get; set; }
        public string projectName { get; set; }
        public List<int> bmptypeIds { get; set; }
        public List<ProjectTree> children { get; set; }

        public ProjectTree Clone() {
            return MemberwiseClone() as ProjectTree;
        }
    }
}
