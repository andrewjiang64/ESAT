using AgBMPTool.DBModel.Model.Boundary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.User
{
    public class UserParcels : UserConnectionItem
    {
        public Parcel Parcel { get; set; }

        [ForeignKey(nameof(Parcel))]
        public int ParcelId { get; set; }
    }
}
