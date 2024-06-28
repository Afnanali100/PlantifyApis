using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantifyApp.Apis.Dtos;
using PlantifyApp.Apis.Errors;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Core.Models;

namespace PlantifyApp.Apis.Controllers
{
   
    public class WebsiteController : ApiBaseController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        private readonly IGenericRepository<Contactus> contactusRepo;

        public WebsiteController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper,IGenericRepository<Contactus>contactusRepo)
        {
            UserManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
            this.contactusRepo = contactusRepo;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        [HttpPost("contactus")]
        public async Task<ActionResult> Contactus(ContactusDto contact)
        {
            if (contact != null)
            {
                var data = new Contactus()
                {
                    name = contact.name,
                    email = contact.email,
                    message = contact.message,
                    is_replied = false,
                    created_at = DateTime.Now

                };

                 await contactusRepo.Add(data);

                return Ok(new
                {
                    message = "your message has created successfully and we will reach u soon :) !",
                    statusCode = 200
                });

            }
            return BadRequest(new ApiErrorResponde(400,"you should send vaild message"));

        }

       

    }
}
