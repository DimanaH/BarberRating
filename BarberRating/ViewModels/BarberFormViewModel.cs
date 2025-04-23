using System.ComponentModel.DataAnnotations;

public class BarberFormViewModel
{

    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задължително.")]
    [StringLength(64, ErrorMessage = "Името не може да е повече от 64 символа.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Описанието е задължително.")]
    [StringLength(255, ErrorMessage = "Описанието не може да е повече от 255 символа.")]
    public string Description { get; set; }

    [Display(Name = "Снимка")]
    [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
    [MaxFileSize(2 * 1024 * 1024)] // 2MB
    public IFormFile? Image { get; set; }

    public string? ExistingImagePath { get; set; }


}
