using Microsoft.AspNetCore.Mvc;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Apis.Helpers;
using PlantifyApp.Repository;
using PlantifyApp.Apis.Errors;

namespace PlantifyApp.Apis.Extension
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection Services)
        {







            Services.AddAutoMapper(typeof(MappingProfiles));


            Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (ActionContext) =>
                {
                    var errors = ActionContext.ModelState.Where(p => p.Value.Errors.Count > 0)
                                                        .SelectMany(E => E.Value.Errors)
                                                        .Select(e => e.ErrorMessage).ToList();


                    var VaidationError = new ApiValidationError()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(VaidationError);

                };
            });




           



            return Services;
        }


        }
}
