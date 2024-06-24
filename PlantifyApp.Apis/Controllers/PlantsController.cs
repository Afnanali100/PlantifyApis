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
using System.Security.Claims;

namespace PlantifyApp.Apis.Controllers
{

    public class PlantsController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IGenericRepository<Plants> plantRepo;
        private readonly IMapper mapper;
     

        public PlantsController(UserManager<ApplicationUser> userManager, IGenericRepository<Plants> plantRepo, IMapper mapper)
        {
            this.userManager = userManager;
            this.plantRepo = plantRepo;
            this.mapper = mapper;
           
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-new-plant-details")]
        public async Task<ActionResult> Createplant(
        [FromQuery] string? common_name,
        [FromQuery] string?[] scientific_name,
        [FromQuery] string?[] other_name,
        [FromQuery] string? medium_url,
        [FromQuery] string? small_url)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user != null)
                    {
                        var plant = new Plants()
                        {
                            user_id = user.Id,
                            common_name = common_name,
                            scientific_name = scientific_name != null ? string.Join(",", scientific_name) : null,
                            other_name = other_name != null ? string.Join(",", other_name) : null,
                            medium_url = medium_url,
                            small_url = small_url
                        };

                      //  var plant = mapper.Map<PlantDto, Plants>(plantDto);
                        await plantRepo.Add(plant);

                        return Ok(new
                        {
                            message = "Plant details created successfully",
                            statusCode = 200
                        });
                    }
                    return NotFound(new ApiErrorResponde(400, "User does not exist"));
                }

                return BadRequest(new ApiErrorResponde(400, "Invalid user email"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponde(500, "Error occurred while creating the plant"));
            }
            
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-all-user-saved-plant-details")]
        public async Task<ActionResult> GetAllPosts()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var plants = await plantRepo.GetAllPlantsDetailsForSpecifcUser(user.Id); // Await the result here

                    if (plants != null && plants.Any()) // Check if there are any posts
                    {
                        // var plantDtos = mapper.Map<IReadOnlyList<Plants>, IReadOnlyList<PlantDto>>(plants);
                        var plantDtos = plants.Select(plant => new PlantDto
                        {
                            Id = plant.Id,
                            user_id = plant.user_id,
                            common_name = plant.common_name,
                            scientific_name = plant.scientific_name?.Split(','),
                            other_name = plant.other_name?.Split(','),
                            default_image =new DefaultImageDto {
                            medium_url = plant.medium_url,
                            small_url = plant.small_url
                            }
                           
                        }).ToList();

                        return Ok(plantDtos);
                        
                      
                    }
                    return NotFound(new ApiErrorResponde(404, "There are no plants available"));
                }
                return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
            }
            return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
        }



    }
}
