using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PlantifyApp.Apis.Extension;
using PlantifyApp.Apis.Middlewares;
using PlantifyApp.Repository.Identity;

namespace PlantifyApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            //------------Contection To DataBase----------------------
            builder.Services.AddDbContext<IdentityConnection>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            //------------Add CORS Services----------------------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });



            ///----------------Extiensions------------------------
            builder.Services.AddApplicationService();
            builder.Services.UserAppExtension(builder.Configuration);
            builder.Services.SwaggerServices();








            var app = builder.Build();
            #region Update Database
            var scoope = app.Services.CreateScope();
            var services = scoope.ServiceProvider;
            var LogerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var dbcontext = services.GetRequiredService<IdentityConnection>();
                await dbcontext.Database.MigrateAsync();
             

                var IdentityContext = services.GetRequiredService<IdentityConnection>();
                await IdentityContext.Database.MigrateAsync();

                
              
            }
            catch (Exception ex)
            {
                var log = LogerFactory.CreateLogger<Program>();
                log.LogError(ex, "Error Occured During Update Database");
            }

            #endregion


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerMiddlware();
            }
            app.UseMiddleware<ExceptionMiddleWare>();
            app.UseStatusCodePagesWithRedirects("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins");

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Assest", "User_images")),
                RequestPath = "/Assest/User_images"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityImages")),
                RequestPath = "/Assest/CommunityImages"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityVideos")),
                RequestPath = "/Assest/CommunityVideos"
            });
            app.MapControllers();

            app.Run();
        }
    }
}