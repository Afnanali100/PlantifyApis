using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Services;
using PlantifyApp.Repository.Identity;
using PlantifyApp.Core.Models;

namespace PlantifyApp.Apis.Extension
{
    public static class UserAppExtentions
    {
        public static IServiceCollection UserAppExtension(this IServiceCollection Services, IConfiguration configuration)
        {

            Services.AddScoped<ITokenService, TokenService>();

            Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;


            }).AddEntityFrameworkStores<IdentityConnection>();


            Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:ValidIssuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:ValidAudience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
            });


            return Services;
        }


    }
}
