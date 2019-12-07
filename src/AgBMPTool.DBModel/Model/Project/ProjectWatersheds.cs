using AgBMPTool.DBModel.Model.ModelComponent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Project
{
    public class ProjectWatersheds : BaseItem
    {
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [ForeignKey(nameof(Watershed))]
        public int WatershedId { get; set; }

        public Watershed Watershed { get; set; }
    }
}
