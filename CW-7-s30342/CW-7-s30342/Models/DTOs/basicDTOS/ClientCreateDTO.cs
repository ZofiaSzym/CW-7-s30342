using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class ClientCreateDTO
{
    [MaxLength(120)]
    [Required]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(120)]
    public string LastName { get; set; }
    [EmailAddress]
    [Required]
    public string Email { get; set; }
    [MaxLength(120)]
    [Required]
    public string Telephone { get; set; }
    [MaxLength(120)]
    [Required]
    public string Pesel { get; set; }
}