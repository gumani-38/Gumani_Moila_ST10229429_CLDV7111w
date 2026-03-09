using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gumani_Moila_ST10229429_CLDV7111w.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [DisplayName("Event")]
        [ForeignKey("Event")]
        public int EventId { get; set; }
        [DisplayName("Venue")]
        [ForeignKey("Venue")]

        public int VenueId { get; set; }
        [DisplayName("Customer")]
        [ForeignKey("CustomerDetail")]
        public int CustomerId { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
        public CustomerDetail? CustomerDetail { get; set; }


    }
}
