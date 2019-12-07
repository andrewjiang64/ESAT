INSERT INTO public."User"(
	"Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp",
	"ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", 
	"AccessFailedCount", "FirstName", "LastName", "Active", "Address1", "Address2", "PostalCode", "Municipality",
	"City", "ProvinceId", "DateOfBirth", "TaxRollNumber", "DriverLicense", "LastFourDigitOfSIN", "Organization",
	"LastModified", "UserTypeId")
	VALUES (1, "demoManagerUser", "DEMOMANAGERUSER", "demo@esat.com", "DEMO@ESAT.COM", false, 
			"AQAAAAEAACcQAAAAEDHpshdeIRFX2/apvuh0vapubbBBB+F+eCqrGzFTYSFYv4AfIWipusCbqUsY7UPknA==",
			"52HRJQQOOAZ2SOK3ZVH7JB53356QT27E", "64c538d3-6b43-4515-83ed-16170438192b", null, false, false, null,
			true, 0, "Demo", "User", false,null,null,null,null,null, 1,null,null,null,null,null, "0001-01-01 00:00:00", 2);