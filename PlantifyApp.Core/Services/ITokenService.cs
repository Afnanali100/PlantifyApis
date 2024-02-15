using Microsoft.AspNetCore.Identity;
using PlantifyApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantifyApp.Core.Services
{
    public interface ITokenService
    {
        public Task<string> CreateToken(ApplicationUser userApp, UserManager<ApplicationUser> userManager);
    }
}
