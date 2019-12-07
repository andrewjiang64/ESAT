using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgBMPTool.DBModel.Model.User;

namespace AgBMPTool.BLL.Services.Users
{
    public class UserDetailsDTO
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Active { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PostalCode { get; set; }

        public string Municipality { get; set; }

        public string City { get; set; }

        public int ProvinceId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string TaxRollNumber { get; set; }

        public string DriverLicense { get; set; }

        public string LastFourDigitOfSIN { get; set; }

        public int AccessFailedCount { get; set; }

        public string Organization { get; set; }

        public DateTime LastModified { get; set; }

        public int UserTypeId { get; set; }


        public static UserDetailsDTO Map(User user)
        {
            var userDetailsDTO = new UserDetailsDTO();

            userDetailsDTO.UserId = user.Id;
            userDetailsDTO.FirstName = user.FirstName;
            userDetailsDTO.LastName = user.LastName;
            userDetailsDTO.Active = user.Active;
            userDetailsDTO.Address1 = user.Address1;
            userDetailsDTO.Address2 = user.Address2;
            userDetailsDTO.PostalCode = user.PostalCode;
            userDetailsDTO.Municipality = user.Municipality;
            userDetailsDTO.City = user.City;
            userDetailsDTO.PasswordHash = user.PasswordHash;
            userDetailsDTO.SecurityStamp = user.SecurityStamp;
            userDetailsDTO.ProvinceId = user.ProvinceId;
            userDetailsDTO.AccessFailedCount = user.AccessFailedCount;


            return userDetailsDTO;
        }
    }
}
