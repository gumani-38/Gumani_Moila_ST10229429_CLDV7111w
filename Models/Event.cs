using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gumani_Moila_ST10229429_CLDV7111w.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Event description is required")]
        public string EventDescription { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Venue")]
        [DisplayName("Which Venue?")]
        public int VenueId { get; set; }
        [NotMapped]
        public int RemainingSeats { get; set; }
        // Default value set to current date/time
        [DisplayFormat(DataFormatString = "{0:dd MMM yy}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for the foreign key
        public Venue? Venue { get; set; }
        public User? User { get; set; }

    }
}
