using System.ComponentModel.DataAnnotations;

namespace Gumani_Moila_ST10229429_CLDV7111w.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "User last name is required")]
        public string UserLastName { get; set; }
        [Required(ErrorMessage = "User email is required")]
        public string UserEmail { get; set; }
        [Required(ErrorMessage = "User password is required")]
        public string UserPassword { get; set; }
        [Required(ErrorMessage = "User phone number is required")]
        public string UserPhone { get; set; }
        public Boolean IsAdmin { get; set; } = false; // Default value set to false
                                                      // Default value set to current date/time
        [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;




    }
}
