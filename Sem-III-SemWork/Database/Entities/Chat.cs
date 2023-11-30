using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Text.Json.Serialization;
using Database.Exceptions;
using Database.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace Database.Entities;

public class Chat
{
    [Required] public Guid Id { get; private init; }

    [Required] public Announcement Announcement { get; init; } = null!;

    [Required] public User Consumer { get; init; } = null!;

    [JsonConstructor]
    private Chat() { Id = Guid.NewGuid(); }
    
    public Chat(Announcement announcement, User consumer) : this()
    {
        Announcement = announcement;
        Consumer = consumer;
    }
    
    public static async Task<Chat?> GetByIdAsync(Guid id)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "select * from \"Chat\" where id = @Id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, id);

        var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
            
        await reader.ReadAsync();
        var result = await FromDbDataReaderAsync(reader);

        await connection.CloseAsync();
        
        return result;
    }
    
    public static async IAsyncEnumerable<Chat> GetByUserAsync(string login)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Chat\" " + 
                             "where announcement_id in (SELECT id FROM \"Announcement\" where owner_id=@Login) " +
                           "UNION " +
                           "SELECT * FROM \"Chat\" " +
                             "where consumer_id=@Login";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Login", NpgsqlDbType.Varchar, login);

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReaderAsync(reader);
        }

        await connection.CloseAsync();
    }
    
    public static async IAsyncEnumerable<Chat> GetByAnnouncementAsync(Guid id)
    {
        var connection = DatabaseSettings.GetConnection();
        await connection.OpenAsync();
        
        const string sql = "SELECT * FROM \"Chat\" where announcement_id=@Id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, id);

        var reader = await cmd.ExecuteReaderAsync();
        
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                yield return await FromDbDataReaderAsync(reader);
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

        if ((await GetByAnnouncementAsync(Announcement.Id).ToListAsync())
            .Any(chat => chat.Consumer.Login.Equals(Consumer.Login)))
            throw new CreationException();
        
        const string sql = "insert into \"Chat\"(id, announcement_id, consumer_id) " +
                           "values (@Id, @AnnouncementId, @ConsumerId) RETURNING id";
        var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", NpgsqlDbType.Uuid, Id);
        cmd.Parameters.AddWithValue("@AnnouncementId", NpgsqlDbType.Uuid, Announcement.Id);
        cmd.Parameters.AddWithValue("@ConsumerId", NpgsqlDbType.Varchar, Consumer.Login);
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
    
    private static async Task<Chat> FromDbDataReaderAsync(DbDataReader reader)
    {
        return new Chat
        {
            Id = reader.GetGuid("id"),
            Announcement = await Announcement.GetByIdAsync(reader.GetGuid("announcement_id"))
                           ?? throw new ItemNotFoundException(),
            Consumer = await User.GetByLoginAsync(reader.GetString("consumer_id"))
                       ?? throw new ItemNotFoundException()
        };
    }
}