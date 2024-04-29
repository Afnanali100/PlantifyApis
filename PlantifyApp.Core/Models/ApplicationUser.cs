using Microsoft.AspNetCore.Identity;


namespace PlantifyApp.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? Role { get; set; }

        public string? Image_path { get; set; }
    }
}
