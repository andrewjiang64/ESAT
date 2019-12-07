using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Shared
{
    public class GridColumnsDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Should contain string without space 
        /// </summary>
        public string FieldName { get; set; }

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

        /// <summary>
        /// Model result type id
        /// </summary>
        public int ModelResultTypeId { get; set; } = 0;

        /// <summary>
        /// Set true if want to hide the col
        /// </summary>
        public bool IsHidden { get; set; } = false;
    }
}
