using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Common
{
    public class GridColumnsModel
    {
        /// <summary>
        /// Should contain string without space 
        /// </summary>
        public string FieldName {get; set;}

        /// <summary>
        /// Title for grid Column
        /// </summary>
        public string FieldTitle { get; set; }

        /// <summary>
        /// Column Unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// DataType for the grid field
        /// </summary>
        public string FieldType { get; set; }
    }
}
