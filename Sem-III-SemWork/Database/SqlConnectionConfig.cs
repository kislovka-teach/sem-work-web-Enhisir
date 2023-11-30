using System.Text.Json;

namespace Database;

public record SqlConnectionConfig(
    string Host,
    string Username,
    string Password,
    string Database)
{
    // var cs = "Host=localhost;Username=postgres;Password=xdkess2004;Database=patients";

    public static  SqlConnectionConfig FromFile(string pathToFile)
    {
        var file = new FileInfo(pathToFile);
        if (!file.Exists) throw new FileNotFoundException(pathToFile);
        
        using var inputConfigStream = file.OpenRead();
        var config = JsonSerializer
            .Deserialize<SqlConnectionConfig>(inputConfigStream);
        if (config is null) throw new InvalidDataException();

        return config;
    }

    public override string ToString()
    {
        return $"Host={Host};Username={Username};Password={Password};Database={Database};Pooling=true";
    }
}