using CW_7_s30342.Exceptions;
using CW_7_s30342.Models;
using CW_7_s30342.Service;
using Microsoft.AspNetCore.Mvc;

namespace CW_7_s30342.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController(IDbService service) : ControllerBase
{
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        try
        {
            var trips = await service.GetClientTripsAsync(id);
            return Ok(trips);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] ClientCreateDTO body)
    {
        var client = await service.CreateClientAsync(body);
        return Created($"/api/clients/{client.IdClient}",client);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> PutClientTrip(
        [FromRoute] int id, 
        [FromRoute] int tripId)
    {
        try
        {
            await service.PutClientToTripAsync(id, tripId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (MaxPeopleException mpe)
        {
            return BadRequest(mpe.Message);
        }



    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientTrip(
    [FromRoute] int id,
    [FromRoute] int tripId)
    {
        
        try
        {
            await service.DeleteClientFromTripAsync(id, tripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}