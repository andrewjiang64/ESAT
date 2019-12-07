using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Optimization
{
    /// <summary>
    /// Optimization is a way to find the optimized bmps. It includes all the settings setup in the interface.
    /// </summary>
    public class Optimization : BaseItem
    {
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }

        public Project.Project Project { get; set; }

        [ForeignKey(nameof(OptimizationType))]
        public int OptimizationTypeId { get; set; }

        public OptimizationType OptimizationType { get; set; }

        /// <summary>
        /// All the constraint
        /// </summary>
        public List<OptimizationConstraints> OptimizationConstraints { get; set; }

        /// <summary>
        /// All the weights
        /// </summary>
        public List<OptimizationWeights> OptimizationWeights { get; set; }

        /// <summary>
        /// The budget target for budget mode
        /// </summary>
        public decimal? BudgetTarget { get; set; }

        public List<OptimizationLegalSubDivisions> OptimizationLegalSubDivisions {get;set;}

        public List<OptimizationParcels> OptimizationParcels { get; set; }
    }
}
