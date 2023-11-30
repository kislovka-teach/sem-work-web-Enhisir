using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Database.Entities;

public class City
{
    [Required] public Guid Id { get; init; }
    [Required] public string Name { get; init; } = null!;
    
    public City() {}

    private City(DbDataReader reader) 
    {
        Id = reader.GetGuid("id"); 
        Name = reader.GetString("name");
    }
    
    public static async Task<City?> GetByIdAsync(Guid id)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"City\" where id = @Id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, id);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = new City(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public static async Task<City?> GetByNameAsync(string name)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"City\" where name = @Name";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Name", NpgsqlDbType.Varchar, name);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = new City(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public async Task SaveAsync()
    {
        EntityValidator.Validate(this);
        if (await GetByIdAsync(Id) is not null)
            throw new InvalidOperationException(nameof(SaveAsync));
        
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "insert into \"City\"(id, name) values (@Id, @Name)";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, Id);
        cmd.Parameters.AddWithValue("@Name", NpgsqlDbType.Varchar, Name);
        
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    public static async IAsyncEnumerable<City> GetAll()
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"City\"";
        var cmd = new NpgsqlCommand(sql, connection);

        var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            yield return new City(reader);
    }
}