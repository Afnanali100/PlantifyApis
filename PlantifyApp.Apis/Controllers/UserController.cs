using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantifyApp.Apis.Errors;
using PlantifyApp.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlantifyApp.Apis.Controllers
{
    public class UserController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor,RoleManager<IdentityRole>roleManager)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.roleManager = roleManager;
        }

        [HttpGet("get-user-types")]
        public async Task<ActionResult> GetUserRoles()
        {
            var roles = roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("upload-user-image")]
        public async Task<ActionResult> UploadUserImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new ApiValidationError() { Errors = new List<string> { "Image is not provided" } });
            }

            try
            {
                string guid = Guid.NewGuid().ToString();
                string fileExtension = Path.GetExtension(image.FileName);
                string newFileName = $"{guid}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";

                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
                string imagePath = Path.Combine(rootPath, "User_images", newFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                string message = "Image uploaded successfully.";

                // Get the base URL dynamically
                var request = httpContextAccessor.HttpContext.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                string relativePath = "/Assest/User_images"; // This should match the path where images are stored on the server
                string fileUrl = $"{baseUrl}{relativePath}/{newFileName}";

                // Combine the base URL with the relative path to the uploaded file

                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);
                    user.Image_name = newFileName;
                    await userManager.UpdateAsync(user);
                }

                return Ok(new
                {
                    message = message,
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-user-image")]
        public async Task<ActionResult> DeleteUserImage()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            var image_name = user.Image_name;
            user.Image_name = null;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(500, "Failed to delete image"));
            }

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
            string imagePath = Path.Combine(rootPath, "User_images", image_name);

            if (System.IO.File.Exists(imagePath))
            {
                try
                {
                    System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));
                }
            }
            else
            {
                return NotFound(new ApiErrorResponde(404, "Image file not found."));
            }

            return Ok(new
            {
                message = "Image Deleted Successfully",
                statusCode = 200
            });

        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-user-address")]
        public async Task<ActionResult> EditUserAddress(string address)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user =await userManager.FindByEmailAsync(email);
                if(user != null)
                {
                    user.Address = address;
                    var result=await userManager.UpdateAsync(user);
                    if(result.Succeeded)
                    {
                        return Ok(new
                        {
                            message = "User Address Updated Successfully!",
                            statusCode = 200
                        });
                    }
                    return Ok(BadRequest(new ApiErrorResponde(500, "Failed Updated User Address !")));

                }
                return Ok(NotFound(new ApiErrorResponde(404,"User Not Exist!")));

            }
            return Ok(NotFound(new ApiErrorResponde(404, "User Not Exist!")));


        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-user-type")]
        public async Task<ActionResult> EditUserRole(string role)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return NotFound(new ApiErrorResponde(404, "User Not Found!"));
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ApiErrorResponde(404, "User Not Found!"));
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                return NotFound(new ApiErrorResponde(404, "The Role does not exist"));
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var currentrole = currentRoles.FirstOrDefault();
            if(currentrole != user.Role && user.Role!=null) { currentRoles = new List<string> { user.Role }; currentrole = user.Role; }
            if (userManager.IsInRoleAsync(user, currentrole).Result)
            {
                var result = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!result.Succeeded)
                {
                    return BadRequest(new ApiErrorResponde(500, "Failed to remove current roles from the user"));
                }

                result = await userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    return BadRequest(new ApiErrorResponde(500, "Failed to assign new role to the user"));
                }
                user.Role = role;
                await userManager.UpdateAsync(user);

                return Ok(new
                {
                    message = "User type updated successfully!",
                    statusCode = 200
                });
            }
            return BadRequest(new ApiErrorResponde(500, "Server Error please try later !"));

        }



    }
}

