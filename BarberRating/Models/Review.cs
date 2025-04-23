namespace BarberRating.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Review
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Връзка към потребител
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Връзка към бръснар
        public int BarberId { get; set; }
        public Barber Barber { get; set; }
    }

}
