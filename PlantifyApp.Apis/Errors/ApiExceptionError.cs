namespace PlantifyApp.Apis.Errors
{ 

    public class ApiExceptionError : ApiErrorResponde
    {
        public string? Details { get; set; }

        public ApiExceptionError(int StatusCode, string? Message = null, string? Details = null) : base(StatusCode, Message)
        {
            this.Details = Details;
        }

    }
}
