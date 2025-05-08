using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class CountryCreateDTO {

    [MaxLength(120)]
    [Required]
    public string Name { get; set; }
}