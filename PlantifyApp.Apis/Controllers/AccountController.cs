using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PlantifyApp.Apis.Dtos;
using PlantifyApp.Core.Models;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Apis.Errors;
using Microsoft.AspNetCore.Http.Extensions;

namespace PlantifyApp.Apis.Controllers
{

    public class AccountController : ApiBaseController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            UserManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginDto LoginUser)
        {
            var user = await UserManager.FindByEmailAsync(LoginUser.Email);
            if (user is null) return Unauthorized(new ApiErrorResponde(401));

            var result = await signInManager.CheckPasswordSignInAsync(user, LoginUser.Password, false);
            if (!result.Succeeded) { return Unauthorized(new ApiErrorResponde(401)); }
            return Ok(new
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await tokenService.CreateToken(user, UserManager)
            });

        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegisterDto model)
        {
            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationError() { Errors = new List<string> { "This Email Is Taken" } });

            if (CheckNameExists(model.DisplayName).Result.Value)
                return BadRequest(new ApiValidationError() { Errors = new List<string> { "This Name Is Taken" } });

            // Check if the role exists
            if (!await roleManager.RoleExistsAsync(model.Role)|| model.Role=="Admin")
            {
                
                    return NotFound(new ApiErrorResponde(500, "The Role is not Exist"));
            }

            var user = new ApplicationUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                created_date=DateTime.Now
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // Log the errors or return them as part of the response
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new ApiErrorResponde(401, errors[0].ToString()));
            }

            // Assign the role to the user
            var roleAssignResult = await UserManager.AddToRoleAsync(user, model.Role);
            if (!roleAssignResult.Succeeded)
                return BadRequest(new ApiErrorResponde(500, "Failed to assign role to the user"));

            return Ok(new
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await tokenService.CreateToken(user, UserManager)
            });
        }




        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-current-user")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await UserManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var request = HttpContext.Request;
            var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
            string image = null;
            if (!string.IsNullOrEmpty(user.Image_name))
                 image = $"{requestUrl}/{user.Image_name}";

            return Ok(new 
            {
                Id=user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Image_path = image,
                Role=user.Role,
                Address=user.Address,
                Token = await tokenService.CreateToken(user, UserManager)
            });
        }






        //[HttpPost("SmsVerfication")]
        //public async Task<ActionResult<SMS>> SendSms(RegisterDto model)
        //{

        //    Random random = new Random();
        //    int verfiction_num = random.Next(10000, 99999);

        //    var user = new ApplicationUser()
        //    {
        //        DisplayName = model.DisplayName,
        //        Email = model.Email,
        //        UserName = model.Email.Split('@')[0],
        //        PhoneNumber = model.PhoneNumber
        //    };

        //    var sms = new SMS()
        //    {
        //        PhoneNumber = user.PhoneNumber,
        //        Body = $"Your Verfication Code is {verfiction_num}",
        //        VerficationCode = verfiction_num.ToString(),

        //    };
        //    smsMessage.sendSms(sms);

        //    return Ok(new SMS
        //    {
        //        PhoneNumber = user.PhoneNumber,
        //        Body = $" Verfication Code is {verfiction_num}",
        //        VerficationCode = verfiction_num.ToString(),
        //        Token = await tokenService.CreateToken(user, UserManager)
        //    });



        //}




        [HttpGet("Checkemail")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await UserManager.FindByEmailAsync(email) is not null;
        }

        [HttpGet("Checkname")]
        public async Task<ActionResult<bool>> CheckNameExists(string name)
        {

            return await UserManager.FindByNameAsync(name) is not null;
        }






    }
}

