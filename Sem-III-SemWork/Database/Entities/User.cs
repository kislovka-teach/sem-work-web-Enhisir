using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using Database.Exceptions;
using Npgsql;
using NpgsqlTypes;
using WebHelper.Enums;
using WebHelper.Models;

namespace Database.Entities;

public class User : AbstractUser
{
    public override Role Role => Role.User;

    [Required] public string Name { get; set; } = null!;
    [Required] public City City { get; set; } = null!;
    
    public User() {}

    private User(DbDataReader reader) : base(reader) 
    {
        Name = reader.GetString("name");
        City = City.GetByIdAsync(reader.GetGuid("city_id")).Result
               ?? throw new ItemNotFoundException();
    }
    
    public static async Task<User?> GetByLoginAsync(string login)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"User\" where login = @Login";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Login", NpgsqlDbType.Varchar, login);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = new User(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public async Task SaveAsync()
    {
        EntityValidator.Validate(this);
        if (await GetByLoginAsync(Login) is not null)
        {
            await UpdateAsync();
            return;
        }
            
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "add_user";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.CommandType = CommandType.StoredProcedure;
            
        cmd.Parameters.AddWithValue("@_login", NpgsqlDbType.Varchar, Login);
        
        cmd.Parameters.AddWithValue("@_name", NpgsqlDbType.Varchar, Name);
        cmd.Parameters.AddWithValue("@_password", NpgsqlDbType.Varchar, Password);
        cmd.Parameters.AddWithValue("@_city_id", NpgsqlDbType.Uuid, City.Id);
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

    private async Task UpdateAsync()
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "update \"User\" " +
                           "    set name = @Name, " +
                           "    city_id = @CityId " +
                           "where login = @Login";
        var cmd = new NpgsqlCommand(sql, connection);
        
        cmd.Parameters.AddWithValue("@Name", NpgsqlDbType.Varchar, Name);
        cmd.Parameters.AddWithValue("@CityId", NpgsqlDbType.Uuid, City.Id);
        cmd.Parameters.AddWithValue("@Login", NpgsqlDbType.Varchar, Login);
        await cmd.ExecuteNonQueryAsync();
        
        await connection.CloseAsync();
    }
}