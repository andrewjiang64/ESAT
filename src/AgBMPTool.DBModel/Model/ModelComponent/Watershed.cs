using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AgBMPTool.DBModel.Model;
using NetTopologySuite.Geometries;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class Watershed : BaseItemWithBoundary
    {        
        public string Name { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public decimal Area { get; set; }

        public int OutletReachId { get; set; }

        public DateTimeOffset Modified { get; set; }

        /// <summary>
        /// All model components
        /// </summary>
        public List<ModelComponent> ModelComponents { get; set; }

        /// <summary>
        /// All subwatersheds
        /// </summary>
        public List<SubWatershed> SubWatersheds { get; set; }

        /// <summary>
        /// All scenarios
        /// </summary>
        public List<Scenario.Scenario> Scenarios { get; set; }
    }
}
