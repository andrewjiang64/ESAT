using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.User
{
    public class UserConnectionItem : BaseItem
    {
        public User User { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
    }
}
