using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class Country_TripCreateDTO
{
    [Required]
    public int IdCountry { get; set; }
    [Required]
    public int IdTrip { get; set; }
}