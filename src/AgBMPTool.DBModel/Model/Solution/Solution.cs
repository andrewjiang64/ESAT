using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Solution
{
    public class Solution : BaseItem
    {

        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        public Project.Project Project { get; set; }

        /// <summary>
        /// If the solution is from optimization
        /// </summary>
        public bool FromOptimization { get; set; }

        public List<SolutionLegalSubDivisions> SolutionLegalSubDivisions { get; set; }

        public List<SolutionParcels> SolutionParcels { get; set; }

        public List<SolutionModelComponents> SolutionModelComponents { get; set; }
    }
}
