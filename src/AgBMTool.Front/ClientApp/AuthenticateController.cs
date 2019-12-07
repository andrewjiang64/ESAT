using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgBMPTool.BLL.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgBMTool.Front.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using AgBMPTool.DBModel.Model.User;
using AgBMTool.Front.ViewModels.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AgBMTool.Front.Controllers
{
    [Route("api/Authenticate")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private UserManager<User> _userManager;

        public AuthenticateController(IOptions<AppSettings> appSettings, UserManager<User> userManager)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
        }

        // POST: api/Authenticate
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginRequestViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.userName);
                    if (user != null && await _userManager.CheckPasswordAsync(user, model.password))
                    {
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                        new Claim("UserID",user.Id.ToString())
                            }),
                            Expires = DateTime.UtcNow.AddDays(1),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret)), SecurityAlgorithms.HmacSha256Signature)
                        };
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                        model.token = tokenHandler.WriteToken(securityToken);
                        model.password = null;
                        model.userTypeId = user.UserTypeId;
                        model.userId = user.Id;
                        model.organizationName = user.Organization;
                        return Ok(model);
                    }
                    else
                    {
                        model.LoginFailedMessage = "Username or password is incorrect";
                        model.token = null;
                        model.password = null;
                        model.userTypeId = 0;
                        model.userId = 0;
                        return Ok(model);
                        //return BadRequest(new { message = "Username or password is incorrect." });
                    }
                }
                model.LoginFailedMessage = "Please enter correct information.";
                model.token = null;
                model.password = null;
                model.userTypeId = 0;
                model.userId = 0;
                return Ok(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}