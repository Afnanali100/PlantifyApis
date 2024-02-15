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

namespace PlantifyApp.Apis.Controllers
{

    public class AccountController : ApiBaseController
    {

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            UserManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto LoginUser)
        {
            var user = await UserManager.FindByEmailAsync(LoginUser.Email);
            if (user is null) return Unauthorized(new ApiErrorResponde(401));

            var result = await signInManager.CheckPasswordSignInAsync(user, LoginUser.Password, false);
            if (!result.Succeeded) { return Unauthorized(new ApiErrorResponde(401)); }
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await tokenService.CreateToken(user, UserManager)
            });

        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            if (CheckEmailExists(model.Email).Result.Value)

               return BadRequest(new ApiValidationError() { Errors = new List<string> { "This Email Is Takon" } });

           if (CheckNameExists(model.DisplayName).Result.Value)

              return BadRequest(new ApiValidationError() { Errors = new List<string> { "This Name Is Takon" } });

            var user = new ApplicationUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
                PhoneNumber = model.PhoneNumber
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(401));
            }

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await tokenService.CreateToken(user, UserManager)
            });

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("currentuser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await UserManager.FindByEmailAsync(email);
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await tokenService.CreateToken(user, UserManager)
            });

        }

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

