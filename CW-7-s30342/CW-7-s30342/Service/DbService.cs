using CW_7_s30342.Exceptions;
using CW_7_s30342.Models;
using CW_7_s30342.Models.DTOs.TaskOrientedDTOs;
using Microsoft.Data.SqlClient;

namespace CW_7_s30342.Service;

public interface IDbService
{
    public Task<IEnumerable<TripCountryGetDTO>> getTripCountriesAsync();
    public Task<IEnumerable<ClientTripsGetDTO>> getClientTripsAsync(int idClient);
    public Task<ClientGetDTO> CreateClientAsync(ClientCreateDTO client);
}

public class DbService(IConfiguration config) : IDbService
{
    public async Task<IEnumerable<TripCountryGetDTO>> getTripCountriesAsync()
    {
        var results = new List<TripCountryGetDTO>();
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTO, t.MaxPeople, c.IdCountry, c.Name  FROM Trip
                        INNER JOIN Country_Trip ct ON t.IdTrip= ct.IdTrip
                        INNER JOIN Country c ON c.IdCountry= ct.IdCountry";
        await using var command = new SqlCommand(sql, connection);
        
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new TripCountryGetDTO()
            {
                IdTrip = reader.GetInt32(0),
                TripName = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                IdCountry = reader.GetInt32(6),
                CountryName = reader.GetString(7)
            });
        }
       
        return results;
    }

    public async Task<IEnumerable<ClientTripsGetDTO>> getClientTripsAsync(int idClient)
    {
        var results = new List<ClientTripsGetDTO>();
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTO, t.MaxPeople, ct.registeredAt, ct.PaymentDate  FROM Trip
                        INNER JOIN Client_Trip ct ON t.IdTrip= ct.IdTrip
                      WHERE ct.IdClient= @IdClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", idClient);
        
        await connection.OpenAsync();
        
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            var result = new List<ClientGetDTO>();
            sql = "SELECT 1 FROM CLIENT WHERE IdClient = @IdClient";
            await using var command2 = new SqlCommand(sql, connection);
            command2.Parameters.AddWithValue("@IdClient", idClient);
            await connection.OpenAsync();
            await using var reader2 = await command2.ExecuteReaderAsync();
            if (!await reader2.ReadAsync())
            {
               throw new NotFoundException($"No such client with id {idClient}"); 
            }
            throw new NotFoundException($"Client with id {idClient} has no trips");
        }
        
        while (await reader.ReadAsync())
        {
            results.Add(new ClientTripsGetDTO()
            {
                IdTrip = reader.GetInt32(0),
                TripName = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetInt32(6),
                PaymentDate = reader.GetInt32(7)
            });
        }

        return results;
    }

    public async Task<ClientGetDTO> CreateClientAsync(ClientCreateDTO client)
    {
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = @"INESRT INTO Client ( FirstName, LastName, Email, Telephone, Pesel) VALUES
                   (@FirstName, @LastName, @Email, @Telephone, @Pesel) SELECT scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        
        await connection.OpenAsync();
        var idClient = Convert.ToInt32(await command.ExecuteScalarAsync());
        return new ClientGetDTO()
        {
            IdClient = idClient,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel,
        };


    }
}