using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlantifyApp.Apis.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage =" name is required")]
        [DisplayName("Name")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = " Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = " Phonenumber is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = " password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}
