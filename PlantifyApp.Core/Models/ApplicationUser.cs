using Microsoft.AspNetCore.Identity;


namespace PlantifyApp.Core.Models
{
	public class ApplicationUser:IdentityUser
	{
		public string DisplayName { get; set; }
	}
}
