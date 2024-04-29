namespace PlantifyApp.Apis.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public string? Password { get; set; }
        public string? Token { get; set; }

        public DateTime LogineTime { get; set; } = DateTime.Now;

        public string? Phone_number { get; set; }

        public string? Image_path { get; set; }


    }
}
