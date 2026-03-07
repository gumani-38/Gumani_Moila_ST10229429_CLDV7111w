using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gumani_Moila_ST10229429_CLDV7111w.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        public string VenueName { get; set; }
        [Required(ErrorMessage = "Venue location is required")]
        public string VenueLocation { get; set; }
        [Required(ErrorMessage = "Venue capacity is required")]
        public int VenueCapacity { get; set; }
        [Required(ErrorMessage = "Venue image URL is required")]
        public string VenueImageUrl { get; set; }

        // Default value set to current date/time
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        public int UserId { get; set; }

        // Navigation property for the foreign key
        public User User { get; set; }

    }
}
