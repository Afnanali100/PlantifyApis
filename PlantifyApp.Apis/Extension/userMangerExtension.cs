using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantifyApp.Core.Models;
using System.Security.Claims;

namespace PlantifyApp.Apis.Extension
{
    public static class userMangerExtension
    {
        public static async Task<ApplicationUser> GetUserWithAddressByEmailAsync(this UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var User = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            return User;
        }

    }
}
