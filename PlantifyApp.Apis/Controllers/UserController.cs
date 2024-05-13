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

namespace PlantifyApp.Apis.Controllers
{
    public class UserController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserController(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("uploaduserimage")]
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
                    user.Image_path = fileUrl;
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




    }
}

