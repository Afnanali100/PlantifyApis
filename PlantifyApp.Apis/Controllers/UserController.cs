using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantifyApp.Apis.Errors;
using PlantifyApp.Core.Models;
using System.Security.Claims;

namespace PlantifyApp.Apis.Controllers
{

    public class UserController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {

            this.userManager = userManager;
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

                var email = User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);
                    user.Image_path = imagePath;
                    await userManager.UpdateAsync(user);

                }

                return Ok(new
                {
                    message = message,
                    path = imagePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
