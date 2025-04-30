using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class ClientCreateDTO
{
    [Length(3, 25)]
    [Required]
    public string FirstName { get; set; }
    [Required]
    [Length(3, 25)]
    public string LastName { get; set; }
    [EmailAddress]
    [Required]
    public string Email { get; set; }
    [Length(9,9)]
    [Required]
    public string Telephone { get; set; }
    [Length(11,11)]
    [Required]
    public string Pesel { get; set; }
}