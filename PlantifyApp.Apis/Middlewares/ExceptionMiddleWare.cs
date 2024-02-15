using System.Net;
using System.Text.Json;
using PlantifyApp.Apis.Errors;

namespace PlantifyApp.Apis.Middlewares
{
    public class ExceptionMiddleWare
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleWare> logger;
        private readonly IHostEnvironment env;

        public ExceptionMiddleWare(RequestDelegate next , ILogger<ExceptionMiddleWare> logger,IHostEnvironment env)
        {
            this.next = next;
            this.logger = logger;
            this.env = env;
        }



        public async Task InvokeAsync(HttpContext context)
        {
            try
            {

               await next.Invoke(context);

            }catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode=(int) HttpStatusCode.InternalServerError;

                var error = env.IsDevelopment() ?
                     new ApiExceptionError((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace)
                    : new ApiExceptionError((int)HttpStatusCode.InternalServerError);


                var option = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json=JsonSerializer.Serialize(error,option);

                await context.Response.WriteAsync(json);

            }

        }


    }
}
