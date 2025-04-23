namespace BarberRating.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Barber
    {
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public string PhotoPath { get; set; }

        public ICollection<Review> Reviews { get; set; }
    }

}
