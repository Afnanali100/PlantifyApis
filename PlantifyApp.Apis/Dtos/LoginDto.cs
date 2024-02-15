using System.ComponentModel.DataAnnotations;

namespace PlantifyApp.Apis.Dtos
{
    public class LoginDto
    {

        [Required(ErrorMessage = " Email is required")]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = " password is required")]
        [DataType(DataType.Password)]

        public string Password { get; set; }
    }
}
