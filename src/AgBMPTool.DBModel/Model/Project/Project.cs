using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Project
{
    public class Project : BaseItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public bool Active { get; set; }

        /// <summary>
        /// Start year of summary
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// End year of summary
        /// </summary>
        public int EndYear { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public User.User User { get; set; }

        [ForeignKey(nameof(ScenarioType))]
        public int ScenarioTypeId { get; set; }

        /// <summary>
        /// Scenario type
        /// </summary>
        public Scenario.ScenarioType ScenarioType { get; set; }

        [ForeignKey(nameof(ProjectSpatialUnitType))]
        public int ProjectSpatialUnitTypeId { get; set; }

        /// <summary>
        /// Spatial units, LSD or Parcel
        /// </summary>
        public ProjectSpatialUnitType ProjectSpatialUnitType { get; set; }

        /// <summary>
        /// Muncipalities assigned to the project
        /// </summary>
        public List<ProjectMunicipalities> ProjectMunicipalities { get; set; }

        /// <summary>
        /// Watersheds assigned to the project
        /// </summary>
        public List<ProjectWatersheds> ProjectWatersheds { get; set; }

        /// <summary>
        /// Optimization settings. It will be null for farmer
        /// </summary>
        public Optimization.Optimization Optimization { get; set; }

        /// <summary>
        /// The solution
        /// </summary>
        public Solution.Solution Solution { get; set; }
    }
}
