using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    /// <summary>
    /// A unit OptimizationSolution is the application of one or more bmps on a single location 
    /// e.g.OptimizationSolutionLocationType such as LegalSubDivisionId, ParcelId or reach BMP ModelComponentId. 
    /// It's different from the unit scenario where subarea is the base unit.
    /// </summary>
    public class UnitOptimizationSolution : BaseItem
    {
        /// <summary>
        /// The LocationId for different OptimizationSolutionLocationType is
        /// LegalSubDivision - LegalSubDivisionId, Parcel - ParcelId or reach BMP - reach BMP ModelComponentId
        /// </summary>
        public int LocationId { get; set; }

        [ForeignKey(nameof(Farm))]
        public int FarmId { get; set; }
        public Boundary.Farm Farm { get; set; }

        /// <summary>
        /// BMP area in ha within current location
        /// </summary>
        public decimal BMPArea { get; set; }

        public bool IsExisting { get; set; }
        public Geometry Geometry { get; set; }

        [ForeignKey(nameof(OptimizationSolutionLocationType))]
        public int OptimizationSolutionLocationTypeId { get; set; }

        /// <summary>
        /// The OptimizationSolutionLocationType can be LegalSubDivision - 1, Parcel - 2 or reach BMP - 3
        /// </summary>
        public OptimizationSolutionLocationType OptimizationSolutionLocationType { get; set; }

        [ForeignKey(nameof(Scenario))]
        public int ScenarioId { get; set; }

        /// <summary>
        /// The base scenario of the unit scenario, either conventional or existing
        /// </summary>
        public Scenario Scenario { get; set; }

        [ForeignKey(nameof(BMPCombination))]
        public int BMPCombinationId { get; set; }

        /// <summary>
        /// All bmp types at this location. This is the bmp combo in Shawn's design, i.e. combination of bmps
        /// </summary>
        public BMPCombinationType BMPCombination { get; set; }

        /// <summary>
        /// Effectiveness
        /// </summary>
        public List<UnitOptimizationSolutionEffectiveness> UnitOptimizationSolutionEffectivenesses { get; set; }
    }

}
