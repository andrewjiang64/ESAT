using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Type
{
    /// <summary>
    /// Base class for all types table
    /// </summary>
    public class BaseType : BaseItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int SortOrder { get; set; }
    }
}
