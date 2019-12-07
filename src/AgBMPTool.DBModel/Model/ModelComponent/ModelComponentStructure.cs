using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    /// <summary>
    /// structure 
    /// </summary>
    public class ModelComponentStructure : BaseItem
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }

        public ModelComponent ModelComponent { get; set; }

        /// <summary>
        /// For on-site effectiveness
        /// </summary>
        [ForeignKey(nameof(SubArea))]
        public int SubAreaId { get; set; }

        public SubArea SubArea { get; set; }

        /// <summary>
        /// The reach where the structue is located
        /// </summary>
        [ForeignKey(nameof(Reach))]
        public int ReachId { get; set; }

        public Reach Reach { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }
    }
}
