using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.User
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Active { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PostalCode { get; set; }

        public string Municipality { get; set; }

        public string City { get; set; }

        [ForeignKey(nameof(Province))]
        public int ProvinceId { get; set; }

        public Boundary.Province Province { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string TaxRollNumber { get; set; }

        public string DriverLicense { get; set; }

        [MaxLength(4)]
        public string LastFourDigitOfSIN { get; set; }

        public string Organization { get; set; }

        public DateTime LastModified { get; set; }

        [ForeignKey(nameof(UserType))]
        public int UserTypeId { get; set; }

        public UserType UserType { get; set; }  

        public List<UserParcels> UserParcels { get; set; }

        public List<UserWatersheds> UserWatersheds { get; set; }

        public List<UserMunicipalities> UserMunicipalities { get; set; }

        public List<Project.Project> Projects { get; set; }
    }
}
