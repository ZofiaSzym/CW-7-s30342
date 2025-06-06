﻿namespace CW_7_s30342.Models.DTOs.TaskOrientedDTOs;

public class TripCountryGetDTO
{
    public int IdTrip { get; set; }
    public string TripName { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public int IdCountry { get; set; }
    public string CountryName { get; set; }
}