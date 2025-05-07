using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class CountryCreateDTO {

[Length(3, 25)]
[Required]
    public string Name { get; set; }
}