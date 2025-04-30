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
    public async Task<IActionResult> GetClientTrips(int ClientId)
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
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO body)
    {
        var client = await service.CreateClientAsync(body);
        return Created($"/api/clients/{client.IdClient}",client);
    }
    
}