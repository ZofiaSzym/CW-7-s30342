using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class TripCreateDTO
{
    [MaxLength(120)]
    [Required]
    public string Name { get; set; }
    [MaxLength(120)]
    [Required]
    public string Description { get; set; }
    [DataType(DataType.Date)]
    [Required]
    public DateTime DateFrom { get; set; }
    [DataType(DataType.Date)]
    [Required]
    public DateTime DateTo { get; set; }
    [Required]
    public int MaxPeople { get; set; }
  
}