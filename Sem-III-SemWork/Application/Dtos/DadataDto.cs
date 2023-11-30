using System.Text.Json.Serialization;

namespace Application.Dtos;

public class DadataDto
{
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("city_fias_id")]
    public Guid? CityId { get; set; }
    
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    
    [JsonPropertyName("region_fias_id")]
    public Guid? RegionId { get; set; }
    
    [JsonPropertyName("qc")]
    public bool Ok { get; set; }
}