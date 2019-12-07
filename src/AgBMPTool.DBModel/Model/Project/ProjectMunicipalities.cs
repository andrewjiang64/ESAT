using AgBMPTool.DBModel.Model.Boundary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Project
{
    public class ProjectMunicipalities : BaseItem
    {
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [ForeignKey(nameof(Municipality))]
        public int MunicipalityId { get; set; }

        public Municipality Municipality { get; set; }
    }
}
