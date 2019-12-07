using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.ViewModels.User
{
    public class LoginRequestViewModel
    {
        [Required(ErrorMessage = "Enter UserName")]
        public string userName { get; set; }

        [Required(ErrorMessage = "Enter Password")]
        public string password { get; set; }

        public int userId { get; set; }
        public string token { get; set; }
        public int userTypeId { get; set; }
        public string organizationName { get; set; }

        public string LoginFailedMessage { get; set; }
    }
}
