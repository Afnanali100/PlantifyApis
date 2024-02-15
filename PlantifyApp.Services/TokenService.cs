using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlantifyApp.Services
{
    public class TokenService:ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> CreateToken(ApplicationUser userApp, UserManager<ApplicationUser> userManager)
        {
            // private Claims
            var AuthClaims = new List<Claim>()
         {
             new Claim(ClaimTypes.GivenName,userApp.DisplayName),
             new Claim(ClaimTypes.Email,userApp.Email),
         };
            var UserRole = await userManager.GetRolesAsync(userApp);
            foreach (var role in UserRole)
                AuthClaims.Add(new Claim(ClaimTypes.Role, role));

            // Key

            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

            //Reqister Claims

            var Token = new JwtSecurityToken(
                issuer: configuration["Jwt:ValidIssuer"],
                audience: configuration["Jwt:ValidAudience"],
                expires: DateTime.Now.AddDays(double.Parse(configuration["Jwt:DurationDays"])),
                claims: AuthClaims,
                signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(Token);
        }

    }
}