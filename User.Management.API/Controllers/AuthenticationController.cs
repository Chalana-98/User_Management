using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.SignUp;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<IdentityUser>userManager,
            RoleManager<IdentityRole> roleManager,IConfiguration configuration) 
        { 
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
        {
            //Check User Exist
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);

            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new Response { Status = "Error", Message = "User already exists!" });

            }
            //add the user in the database

            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username
            };

            if (await _roleManager.RoleExistsAsync(role)) 
            {

                var result = await _userManager.CreateAsync(user, registerUser.Password);

                if (!result.Succeeded)
                {
                   return StatusCode(StatusCodes.Status500InternalServerError,
                           new Response { Status = "Error", Message = "User Failed to Create" });
                }

                //Add role to the user
               await  _userManager.AddToRoleAsync(user, role);
                return StatusCode(StatusCodes.Status201Created,
                           new Response { Status = "Success", Message = "User Created Successfullly" });

            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This Role Doesnot Exist" });
            }
          }
        }
    }
