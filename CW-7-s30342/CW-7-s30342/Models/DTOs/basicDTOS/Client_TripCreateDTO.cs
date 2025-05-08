using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class Client_TripCreateDTO
{
    [Required]
    public int IdClient { get; set; }
    [Required]
    public int IdCountry { get; set; }
    [Required]
    public int RegisteredAt { get; set; }
    public int PaymentDate { get; set; }
}