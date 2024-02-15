namespace PlantifyApp.Apis.Extension
{
    public static class SwaggerServiceExtention
    {

        public static IServiceCollection SwaggerServices(this IServiceCollection Services)
        {
            Services.AddEndpointsApiExplorer();
            Services.AddSwaggerGen();
            return Services;
        }


        public static void UseSwaggerMiddlware(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

    }
}
