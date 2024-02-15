namespace PlantifyApp.Apis.Errors
{
    public class ApiValidationError:ApiErrorResponde
    {
        public List<string> Errors { get; set; }

        public ApiValidationError():base(400)
        {
            Errors = new List<string>();    
        }

    }
}
