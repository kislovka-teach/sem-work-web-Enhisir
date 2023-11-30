using System.Net;
using System.Text.Json;
using Database.Entities;
using Database.Extensions;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Api;

public class ApiCityComponent : ApiComponent
{
    private readonly List<City> _allCities;
    
    public ApiCityComponent() : base("city")
    {
        _allCities = City.GetAll().ToListAsync().Result;
    }

    [Mapping(Method.GET, "/all")]
    public async Task GetAll(Context context) 
        => await JsonSerializer.SerializeAsync(
            context.Response.OutputStream, 
            _allCities, 
            JsonSerializing.StandardOptions);

    [Mapping(Method.GET, "/find")]
    public async Task Find(Context context)
    {
        var cityUuid = context.Request.QueryString.Get("uuid");
        var cityName = context.Request.QueryString.Get("name");

        if (cityName is null && cityUuid is null)
        {
            ShowBadRequest(context);
            return;
        }
        
        var city = cityName is null 
            ? await City.GetByIdAsync(Guid.Parse(cityUuid!)) 
            : await City.GetByNameAsync(cityName);

        if (city is null)
        {
            ShowNotFound(context);
            return;
        }
            
        await JsonSerializer.SerializeAsync(
            context.Response.OutputStream, city, JsonSerializing.StandardOptions);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
}