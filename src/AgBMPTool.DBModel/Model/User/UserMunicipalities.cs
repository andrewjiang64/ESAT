using AgBMPTool.DBModel.Model.Boundary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.User
{
    public class UserMunicipalities : UserConnectionItem
    {
        public Municipality Municipality { get; set; }

        [ForeignKey(nameof(Municipality))]
        public int MunicipalityId { get; set; }
    }
}
