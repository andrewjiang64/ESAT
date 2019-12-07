using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    /// <summary>
    /// BMP Types
    /// </summary>
    public class BMPType : Type.BaseType
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentTypeId { get; set; }

        public ModelComponent.ModelComponentType ModelComponentType {get;set;}
    }
}
