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
    public Task PutClientToTripAsync(int ClientId, int TripId);
    public Task DeleteClientFromTripAsync(int ClientId, int TripId);
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

    public async Task PutClientToTripAsync(int ClientId, int TripId)
    {
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = @"INESRT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES
                   (@IdClient, @IdTrip, @RegisteredAt) SELECT scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        
        //checking if client exists
        var sqlClient = "SELECT 1 FROM CLIENT WHERE IdClient = @IdClient";
        await using var commandClient = new SqlCommand(sql, connection);
        commandClient.Parameters.AddWithValue("@IdClient", ClientId);
        await connection.OpenAsync();
        await using var reader2 = await commandClient.ExecuteReaderAsync();
        if (!await reader2.ReadAsync())
        {
            throw new NotFoundException($"No such client with id {ClientId}"); 
        }
        //checking if trip exists
        var sqlTrip = "SELECT maxPeople FROM Trip WHERE IdTrip = @IdTrip";
        await using var commandTrip = new SqlCommand(sql, connection);
        commandTrip.Parameters.AddWithValue("@IdTrip", TripId);
        await connection.OpenAsync();
        await using var readerTrip = await commandClient.ExecuteReaderAsync();
        if (!await readerTrip.ReadAsync())
        {
            throw new NotFoundException($"No such trip with id {TripId}"); 
        }
        var MaxClients = readerTrip.GetInt32(0);
        
        //checking if max isn't already fulfilled 
        var sqlTripClient = "SELECT Sum(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
        await using var commandTripClient = new SqlCommand(sql, connection);
        commandTrip.Parameters.AddWithValue("@IdTrip", TripId);
        await connection.OpenAsync();
        await using var readerTripClient = await commandClient.ExecuteReaderAsync();
       var ClientsCurrently = readerTripClient.GetInt32(0);
       if (ClientsCurrently >= MaxClients)
       {
           throw new MaxPeopleException($"There is already {MaxClients} clients on the trip");
       }
       
        command.Parameters.AddWithValue("@IdClient", ClientId);
        command.Parameters.AddWithValue("@IdTrip", TripId);
        command.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteClientFromTripAsync(int ClientId, int TripId)
    {
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = @"SELECT 1 FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdTrip", TripId);
        command.Parameters.AddWithValue("@IdClient", ClientId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Client {ClientId} isn't going to that trip. Check if you didn't misspell anything"); 
        }
        sql = "DELETE FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient";
        command.Parameters.AddWithValue("@IdTrip", TripId);
        command.Parameters.AddWithValue("@IdClient", ClientId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
    }
}