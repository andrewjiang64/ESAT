using AgBMPTool.DBModel.Model.ModelComponent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.User
{
    public class UserWatersheds : UserConnectionItem
    {
        public Watershed Watershed { get; set; }

        [ForeignKey(nameof(Watershed))]
        public int WatershedId { get; set; }
    }
}
