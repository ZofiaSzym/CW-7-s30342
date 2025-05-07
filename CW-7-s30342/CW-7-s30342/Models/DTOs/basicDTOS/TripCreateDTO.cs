using System.ComponentModel.DataAnnotations;

namespace CW_7_s30342.Models;

public class TripCreateDTO
{
    [Length(3, 25)]
    [Required]
    public string Name { get; set; }
    [Length(10, 100)]
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