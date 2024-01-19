using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json.Serialization;
using Database.Exceptions;
using Npgsql;
using NpgsqlTypes;

namespace Database.Entities;

public class Announcement
{
    public Guid Id { get; private set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string Description { get; set; } = null!;
    [Required] public decimal Price { get; set; }
    [Required] public string OwnerId { get; set; } = null!;
    public User? Owner { get; set; }
    [Required] public City City { get; set; }
    [Required] public string Address { get; set; } = null!;
    public bool IsPrivileged { get; set; } = false;

    [JsonIgnore] public bool IsDisabled { get; set; } = false;
    
    public Announcement() { Id = Guid.NewGuid(); }

    /*private Announcement(DbDataReader reader)
    {
        Id = reader.GetGuid("id");
        Title = reader.GetString("title");
        Description = reader.GetString("description");
        Price = reader.GetDecimal("price");
        OwnerId = reader.GetString("owner_id");
        City = City.GetByIdAsync(reader.GetGuid("city_id")).Result
            ?? throw new ItemNotFoundException();
        Address = reader.GetString("address");
    }*/
    
    private static async Task<Announcement> FromDbDataReader(DbDataReader reader)
    {
        return new Announcement
        {
            Id = reader.GetGuid("id"),
            Title = reader.GetString("title"),
            Description = reader.GetString("description"),
            Price = reader.GetDecimal("price"),
            OwnerId = reader.GetString("owner_id"),
            Owner = await User.GetByLoginAsync(reader.GetString("owner_id"))
                    ?? throw new ItemNotFoundException(),
            City = await City.GetByIdAsync(reader.GetGuid("city_id"))
                   ?? throw new ItemNotFoundException(),
            Address = reader.GetString("address"),
            IsPrivileged = reader.GetBoolean("privileged"),
            IsDisabled = reader.GetBoolean("disabled")
        };

    }
    
    public static async Task<Announcement?> GetByIdAsync(Guid id)
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"Announcement\" where id = @Id and disabled=false";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, id);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = await FromDbDataReader(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public static async IAsyncEnumerable<Announcement> GetByUserAsync(string login)
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Announcement\" where owner_id=@Login and disabled=false" +
                           " ORDER BY privileged DESC";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Login", NpgsqlDbType.Varchar, login);

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReader(reader);
        }

        await connection.CloseAsync();
    }
    
    public static async IAsyncEnumerable<Announcement>? GetAll()
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Announcement\" where disabled=false ORDER BY privileged DESC";
        var cmd = new NpgsqlCommand(sql, connection);

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReader(reader);
        }

        await connection.CloseAsync();
    }
    
    public static async IAsyncEnumerable<Announcement>? GetByCity(City city)
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Announcement\" where city_id=@CityId and disabled=false" +
                           " ORDER BY privileged DESC";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CityId", NpgsqlDbType.Uuid, city.Id);

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReader(reader);
        }

        await connection.CloseAsync();
    }
    
    public static async IAsyncEnumerable<Announcement>? GetBy(User? user, City? city, string? title)
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        var cmd = new NpgsqlCommand();
        cmd.Connection = connection;

        const string sqlBase = "SELECT * FROM \"Announcement\"";
        
        var sqlParams = new List<string>();
        
        if (user is not null)
        {
            sqlParams.Add(" owner_id=@OwnerId");
            cmd.Parameters.AddWithValue("@OwnerId", NpgsqlDbType.Varchar, user.Login);
        }
        
        if (city is not null)
        {
            sqlParams.Add(" city_id=@CityId");
            cmd.Parameters.AddWithValue("@CityId", NpgsqlDbType.Uuid, city.Id);
        }

        if (title is not null)
        {
            sqlParams.Add(" lower(title) like ('%' || @Title || '%')");
            cmd.Parameters.AddWithValue("@Title", NpgsqlDbType.Varchar, title.Trim().ToLower());
        }

        var builder = new StringBuilder().Append(sqlBase);
        builder.Append(" where disabled=false");
        if (sqlParams.Count > 0)
        {
            for (var i = 0; i < sqlParams.Count; i++)
                builder.Append(" and ").Append(sqlParams[i]);

            cmd.CommandText = builder.ToString();
        }

        builder.Append(" ORDER BY privileged DESC");
        cmd.CommandText = builder.ToString();
        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReader(reader);
        }

        await connection.CloseAsync();
    }
    
    public static async IAsyncEnumerable<Announcement>? GetByName(string title)
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Announcement\" where disabled=false " +
                           "and lower(title) like ('%' || @Title || '%') ORDER BY privileged DESC";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Title", NpgsqlDbType.Uuid, title.Trim().ToLower());

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReader(reader);
        }

        await connection.CloseAsync();
    }
    
    public async Task SaveAsync()
    {
        EntityValidator.Validate(this);
        if (await GetByIdAsync(Id) is not null)
        {
            await UpdateAsync();
            return;
        }

        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "add_announcement";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@_id", NpgsqlDbType.Uuid, Id);
        cmd.Parameters.AddWithValue("@_title", NpgsqlDbType.Varchar, Title);
        cmd.Parameters.AddWithValue("@_description", NpgsqlDbType.Varchar, Description);
        cmd.Parameters.AddWithValue("@_price", NpgsqlDbType.Numeric, Price);
        cmd.Parameters.AddWithValue("@_owner_id", NpgsqlDbType.Varchar, OwnerId);
        cmd.Parameters.AddWithValue("@_city_id", NpgsqlDbType.Uuid, City.Id);
        cmd.Parameters.AddWithValue("@_address", NpgsqlDbType.Varchar, Address);
        var result = new NpgsqlParameter("@_result", NpgsqlDbType.Integer)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(result);
        
        try
        {
            await cmd.ExecuteNonQueryAsync();
            if ((int)(result.Value ?? -1) == -1) throw new CreationException();
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

    public async Task DisableAsync()
    {
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "update \"Announcement\"" +
                           "    set disabled = true where id = @Id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, Id);
        
        await cmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        IsDisabled = false;
    }

    private async Task UpdateAsync()
    {
        if (IsDisabled)
            throw new Exception();
        
        EntityValidator.Validate(this);
        await using var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "update \"Announcement\"" +
                           "    set description = @Description, " +
                           "    price = @Price, " +
                           "    privileged = @IsPrivileged " +
                           "where id = @Id";
        var cmd = new NpgsqlCommand(sql, connection);
        
        cmd.Parameters.AddWithValue("@Description", NpgsqlDbType.Varchar, Description);
        cmd.Parameters.AddWithValue("@Price", NpgsqlDbType.Numeric, Price);
        cmd.Parameters.AddWithValue("@IsPrivileged", NpgsqlDbType.Boolean, IsPrivileged);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, Id);
        
        await cmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }
}