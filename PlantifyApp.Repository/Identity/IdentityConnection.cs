using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlantifyApp.Core.Models;


namespace PlantifyApp.Repository.Identity
{
	public class IdentityConnection:IdentityDbContext<ApplicationUser>
	{
        public IdentityConnection(DbContextOptions<IdentityConnection> options):base(options) 
        {
            
        }

    }
}
