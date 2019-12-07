using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class ModelComponentType : Type.BaseType
    {
        /// <summary>
        /// If it's a structure
        /// </summary>
        public bool IsStructure { get; set; }
    }
}
