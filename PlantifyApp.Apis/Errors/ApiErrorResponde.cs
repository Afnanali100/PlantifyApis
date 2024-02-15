namespace PlantifyApp.Apis.Errors
{
    public class ApiErrorResponde
    {
        public int StatusCode { get; set; } 

        public string? Message { get; set; }


        public ApiErrorResponde( int StatusCode , string? Mesaage=null)
        {
            this.StatusCode = StatusCode;
            this.Message = Mesaage ?? GetMessage(StatusCode);
        }

        private string? GetMessage(int code)
        {

            return code switch
            {
                400 => "A Bad Request ,You Have Made",
                401 => "Athourized, you are not",
                404 => "Resources Not Founded",
                500 => "There is A server Error",
                _=>null
            };


        }
       



    }
}
