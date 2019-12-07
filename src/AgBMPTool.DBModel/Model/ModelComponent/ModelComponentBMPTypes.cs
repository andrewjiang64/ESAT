using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    /// <summary>
    /// It defined what type of bmps could be applied to a model component      
    /// </summary>
    public class ModelComponentBMPTypes : BaseItem
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }
        
        public ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(BMPType))]
        public int BMPTypeId { get; set; }

        public Scenario.BMPType BMPType { get; set; }
    }
}
