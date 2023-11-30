using System.Net.Http.Json;
using System.Text.Json;
using Application.Dtos;
using Database.Entities;

namespace Application.Services;

public class DadataValidator
{
    private const string Url = "https://cleaner.dadata.ru/api/v1/clean/address";
    private readonly HttpClient _client = new();
    

    public async Task<bool> ValidateFullAddressAsync(Guid cityId, string address)
    {
        var content = JsonContent.Create(new []{ address, });
        var response = await _client.PostAsync(Url, content);
        var dto = 
            await JsonSerializer.DeserializeAsync<DadataDto>(
                await response.Content.ReadAsStreamAsync());

        return dto is not null && dto.Ok && 
               (dto.Region is not null && dto.RegionId == cityId ||
                dto.City is not null && dto.CityId == cityId);
    }

    public async Task<bool> ValidateCityAsync(Guid id)
        => await City.GetByIdAsync(id) is not null;
}