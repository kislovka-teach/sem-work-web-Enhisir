using System.Globalization;
using CsvHelper;
using Database;
using Database.Entities;

namespace DatabasePreparer;

public static class CityParser
{
    public static async Task SaveData()
    {
        using var textReader = new StreamReader("./city.csv");
        var reader = new CsvReader(textReader, CultureInfo.InvariantCulture);

        await foreach (var city in reader.GetRecordsAsync<City>())
        {
            await city.SaveAsync();
        }
    }
}