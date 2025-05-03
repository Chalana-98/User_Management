using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.Login;
using User.Management.API.Models.Authentication.SignUp;
using User.Management.Service.Models;
using User.Management.Service.Services;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,IEmailService emailService,
            SignInManager<IdentityUser>signInManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            
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
                UserName = registerUser.Username,
                TwoFactorEnabled=true
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
                await _userManager.AddToRoleAsync(user, role);

                // Add token to verifiy the email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLlink = $"https://localhost:7103{ Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email }) }";
                var message = new Message(new string[] { user.Email! }, "Confirmation email link", confirmationLlink!);
                _emailService.SendEmail(message);


                return StatusCode(StatusCodes.Status201Created,
                           new Response { Status = "Success", Message = $"User Created & Email sent to {user.Email} Successfullly" });

             

            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This Role Doesnot Exist" });
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task <IActionResult> ConfirmEmail(string token, string email)
        {   
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = "Email Verified Successfullly" });
                }

            }
           
            return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "This User Doesnot exist!" });
        }

        [HttpPost]
        [Route("login")]
         public async   Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            //checking the user
            var user = await _userManager.FindByNameAsync(loginModel.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user , loginModel.Password))
            {
                var authCliams = new List<Claim>
               {
                   new Claim(ClaimTypes.Name, user.UserName),
                   new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
               };
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles) 
                {
                    authCliams.Add(new Claim(ClaimTypes.Role, role));
                }

                if (user.TwoFactorEnabled)
                {   
                    await _signInManager.SignOutAsync();
                    await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    var message = new Message(new string[] { user.Email! }, "OTP Conffrimation", token!);
                    _emailService.SendEmail(message);


                    return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"We have sent an OTP to your Email {user.Email}" });
                }

                var jwtToken = GetToken(authCliams);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }
            return Unauthorized();

        }

        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> LoginWithOTP(string code, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var signIn = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
            if (signIn.Succeeded) 
            {
             if(user != null)
                {
                    var authCliams = new List<Claim>
                   {
                       new Claim(ClaimTypes.Name, user.UserName),
                       new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                   };
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        authCliams.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = GetToken(authCliams);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        expiration = jwtToken.ValidTo
                    });
                }
            }
            return Unauthorized();

        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"], 
                audience: _configuration["JWT:ValidAudience"], 
                expires: DateTime.Now.AddHours(3),
                claims: authClaims, 
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256) 
            );
            return token; 
        }


    };

 
     
 }
