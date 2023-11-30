using Npgsql;

namespace Database;

public static class DatabaseSettings
{
    private const string DefaultConfigPath = "config/default.json";
    private static readonly SqlConnectionConfig Config = SqlConnectionConfig.FromFile(DefaultConfigPath);

    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(Config.ToString());
    }
}