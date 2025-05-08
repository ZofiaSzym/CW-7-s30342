using CW_7_s30342.Exceptions;
using CW_7_s30342.Models;
using CW_7_s30342.Models.DTOs.TaskOrientedDTOs;
using Microsoft.Data.SqlClient;

namespace CW_7_s30342.Service;

public interface IDbService
{
    public Task<IEnumerable<TripCountryGetDTO>> GetTripCountriesAsync();
    public Task<IEnumerable<ClientTripsGetDTO>> GetClientTripsAsync(int idClient);
    public Task<Client> CreateClientAsync(ClientCreateDTO client);
    public Task PutClientToTripAsync(int clientId, int tripId);
    public Task DeleteClientFromTripAsync(int clientId, int tripId);
}

public class DbService(IConfiguration config) : IDbService
{
    //getTripCountiesAsync pomaga nam w wyszukaniu danych wycieczki + dodatkowo daje nazwę kraju.
    public async Task<IEnumerable<TripCountryGetDTO>> GetTripCountriesAsync()
    {
        var results = new List<TripCountryGetDTO>();
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        //łączenie tabel Trip -> country_trip_> country pozwala nam wypisać dane wycieczki + połączonego z nią kraju
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name  FROM Trip t
                        INNER JOIN Country_Trip ct ON t.IdTrip= ct.IdTrip
                        INNER JOIN Country c ON c.IdCountry= ct.IdCountry";
        await using var command = new SqlCommand(sql, connection);
        
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        //aby zwrócić te wycieczki
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

    // GetClientTripsAsync daj nam wszystkie wycieczki jednego klienta
    public async Task<IEnumerable<ClientTripsGetDTO>> GetClientTripsAsync(int idClient)
    {
        
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString); ;
        // sprawdzamy czy taki klient istnieje
        var sql2 = "SELECT 1 FROM CLIENT WHERE IdClient = @IdClient";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@IdClient", idClient);
        await connection.OpenAsync();
        await using (var reader2 = await command2.ExecuteReaderAsync())
        {
            if (!reader2.HasRows)
            {
                throw new NotFoundException($"No such client with id {idClient}");
            }
        }

        //zapytanie to zapamiętuje dane wycieczki jeżeli powiązany jest z nią nasz klient
        var results = new List<ClientTripsGetDTO>();
        var sql = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.registeredAt, ct.PaymentDate  FROM Trip t
                        INNER JOIN Client_Trip ct ON t.IdTrip= ct.IdTrip
                      WHERE ct.IdClient= @IdClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", idClient);
        await using var reader = await command.ExecuteReaderAsync();
        //aby zwrócić te wycieczki
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
                PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            });
        }

        return results;
    }

    //tworzy nowego klienta. 
    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        
        //tworzenie nowego klienta z wartościami podanymi (oprócz id)
        var sql = @"INSERT INTO Client ( FirstName, LastName, Email, Telephone, Pesel) VALUES
                   (  @FirstName, @LastName, @Email, @Telephone, @Pesel); SELECT scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);        
        await connection.OpenAsync();
        var idClient = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        //zwracanie klienta:)
        return new Client
        {
            IdClient = idClient,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel,
        };
        
    }

    //endpoint ten pozwala na włożenie ooby na wyciueczkę
   public async Task PutClientToTripAsync(int clientId, int tripId)
{
    var connectionString = config.GetConnectionString("Default");
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    int maxClients = 0;

    // Sprawdza czy kilent istnieje
    var sqlClient = "SELECT 1 FROM CLIENT WHERE IdClient = @IdClient";
    await using (var commandClient = new SqlCommand(sqlClient, connection))
    {
        commandClient.Parameters.AddWithValue("@IdClient", clientId);
        await using var reader = await commandClient.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new NotFoundException($"No such client with id {clientId}");
    }

    // Sprawdza czy wycieczka istnieje
    var sqlTrip = "SELECT maxPeople FROM Trip WHERE IdTrip = @IdTrip";
    await using (var commandTrip = new SqlCommand(sqlTrip, connection))
    {
        commandTrip.Parameters.AddWithValue("@IdTrip", tripId);
        await using var reader = await commandTrip.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new NotFoundException($"No such trip with id {tripId}");
        maxClients = reader.GetInt32(0);
    }

    //liczy klientów na wycieczce porównuje do wcześniej zapamiętanego limitu osób
    var sqlTripClient = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
    await using (var commandTripClient = new SqlCommand(sqlTripClient, connection))
    {
        commandTripClient.Parameters.AddWithValue("@IdTrip", tripId);
        var clientsCurrently = (int)await commandTripClient.ExecuteScalarAsync();
        if (clientsCurrently >= maxClients)
            throw new MaxPeopleException($"There are already {clientsCurrently} clients on the trip (max: {maxClients})");
    }

    //dodajemy go na wycieczke :)
    var sql = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) 
                VALUES (@IdClient, @IdTrip, @RegisteredAt)";
    await using (var command = new SqlCommand(sql, connection))
    {
        command.Parameters.AddWithValue("@IdClient", clientId);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        command.Parameters.AddWithValue("@RegisteredAt", 0); // W poleceniu napisane jest, aby podać aktualną datę, jednak w danych wykresu jest int. Założyłam więc, że wkładamy 0 i traktujemy to "ile dni minęło od zarejestrowania"  
        await command.ExecuteNonQueryAsync();
    }
}

    //usuwamy klienta z wycieczki podając dane jego i wycieczki
    public async Task DeleteClientFromTripAsync(int clientId, int tripId)
    {
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        //sprawdzamy czy klient ten w ogóle jest przypisany na tą wycieczkę
        var sql = @"SELECT 1 FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        command.Parameters.AddWithValue("@IdClient", clientId);
        await connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new NotFoundException(
                    $"Client {clientId} isn't going to that trip. Check if you didn't misspell anything");
            }
        }

        //usuwamy go
        var sql2 = "DELETE FROM Client_Trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient";
        var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@IdTrip", tripId);
        command2.Parameters.AddWithValue("@IdClient", clientId);
        await command2.ExecuteNonQueryAsync();
    }
}