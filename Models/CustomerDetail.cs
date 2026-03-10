using System.ComponentModel.DataAnnotations;

namespace Gumani_Moila_ST10229429_CLDV7111w.Models
{
    public class CustomerDetail
    {

        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Customer last name is required")]
        public string CustomerLastName { get; set; }
        [Required(ErrorMessage = "Customer phone number is required")]
        public string CustomerPhone { get; set; }

        public string? CustomerEmail { get; set; }
        // Default value set to current date/time
        [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
