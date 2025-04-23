using System.ComponentModel.DataAnnotations;

public class ReviewFormViewModel
{
    [Required]
    public int BarberId { get; set; }

    [Required(ErrorMessage = "Моля, въведете текст на ревюто.")]
    [StringLength(1000)]
    public string Content { get; set; }
}
