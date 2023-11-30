using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using Database.Exceptions;
using Npgsql;
using NpgsqlTypes;

namespace Database.Entities;

public class Message
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public Guid ChatId { get; set; }
    
    [Required]
    public string SenderId { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    public Message() { Id = Guid.NewGuid(); }
    
    public Message(DbDataReader reader)
    {
        Id = reader.GetGuid("id");
        ChatId = reader.GetGuid("chat_id");
        SenderId = reader.GetString("sender_id");
        Content = reader.GetString("content");
    }
    
    public static async Task<Message?> GetByIdAsync(Guid id)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"Message\" where id = @Id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, id);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = new Message(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public static async IAsyncEnumerable<Message> GetByChatAsync(Chat chat, DateTime? filterDateTime = null)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        var cmd = new NpgsqlCommand();

        var sql = "SELECT * FROM \"Message\" WHERE chat_id=@ChatId";
        
        if (filterDateTime is not null)
        {
            sql += " AND creation_time > @FilterDateTime";
            cmd.Parameters.AddWithValue(
                "@FilterDateTime", 
                NpgsqlDbType.TimestampTz, filterDateTime.Value.ToUniversalTime());
        }
        sql += " ORDER BY creation_time ASC";
        cmd.Parameters.AddWithValue("@ChatId", NpgsqlDbType.Uuid, chat.Id);
        
        cmd.Connection = connection;
        cmd.CommandText = sql;
        
        var reader = await cmd.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return new Message(reader);
        }

        await connection.CloseAsync();
    }
    
    public async Task SaveAsync()
    {
        EntityValidator.Validate(this);
        if (await GetByIdAsync(Id) is not null)
            throw new InvalidOperationException(nameof(SaveAsync));
        
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();

        const string sql = "add_message";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@_id", NpgsqlDbType.Uuid, Id);
        cmd.Parameters.AddWithValue("@_chat_id", NpgsqlDbType.Uuid, ChatId);
        cmd.Parameters.AddWithValue("@_sender_id", NpgsqlDbType.Varchar, SenderId);
        cmd.Parameters.AddWithValue("@_content", NpgsqlDbType.Varchar, Content);
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
}