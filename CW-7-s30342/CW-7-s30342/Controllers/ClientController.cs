using CW_7_s30342.Exceptions;
using CW_7_s30342.Models;
using CW_7_s30342.Service;
using Microsoft.AspNetCore.Mvc;

namespace CW_7_s30342.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController(IDbService service) : ControllerBase
{
    [HttpGet("{ClientId}/trips")]
    public async Task<IActionResult> GetClientTrips(
        [FromRoute] int ClientId)
    {
        try
        {
            return Ok(await service.getClientTripsAsync(ClientId));
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

    [HttpPut("{ClientId}/trips/{TripId}")]
    public async Task<IActionResult> PutClientTrip(
        [FromRoute] int ClientId, 
        [FromRoute] int TripId)
    {
        try
        {
            await service.PutClientToTripAsync(ClientId, TripId);
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

    [HttpDelete("{ClientId}/trips/{TripId}")]
    public async Task<IActionResult> DeleteClientTrip(
    [FromRoute] int ClientId,
    [FromRoute] int TripId)
    {
        
        try
        {
            await service.DeleteClientFromTripAsync(ClientId, TripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}