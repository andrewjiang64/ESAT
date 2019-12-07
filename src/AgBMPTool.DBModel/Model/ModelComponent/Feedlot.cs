using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AgBMPTool.DBModel.Model.Type;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class Feedlot : ModelComponentStructurePolygon
    {
        /// <summary>
        /// The animial type
        /// </summary>
        public int AnimalTypeId { get; set; }

        [ForeignKey("AnimalTypeId")]
        public AnimalType AnimalType { get; set; }

        /// <summary>
        /// Number of Animals raised per year
        /// </summary>
        public int AnimalNumber { get; set; }

        /// <summary>
        /// The ratio of adult animal
        /// </summary>
        [Column(TypeName = "numeric(3,3)")]
        public decimal AnimalAdultRatio { get; set; }

        /// <summary>
        /// Maximum surface area, ha
        /// </summary>
        [Column(TypeName = "numeric(10,4)")]
        public decimal Area { get; set; }
    }
}
