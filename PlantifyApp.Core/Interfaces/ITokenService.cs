using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlantifyApp.Core.Models;

namespace PlantifyApp.Core.Interfaces
{
    public interface ITokenService
    {
        public  Task<string> CreateToken(ApplicationUser userApp, UserManager<ApplicationUser> userManager);
        
    }
}
