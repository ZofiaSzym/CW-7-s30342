using CW_7_s30342.Models.DTOs.TaskOrientedDTOs;
using CW_7_s30342.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CW_7_s30342.Controllers;

[ApiController]
[Route("api/trips")]
public class TripController(IDbService _service) :ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTripsWithCountries()
    {
        return Ok(await _service.GetTripCountriesAsync() );
    }
}
