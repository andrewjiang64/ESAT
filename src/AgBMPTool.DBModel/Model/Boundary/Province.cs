using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    public class Province : Type.BaseType
    {
        [MaxLength(2)]
        public string Code { get; set; }

        public Country Country { get; set; }

        [ForeignKey(nameof(Country))]
        public int CountryId { get; set; }
    }
}
