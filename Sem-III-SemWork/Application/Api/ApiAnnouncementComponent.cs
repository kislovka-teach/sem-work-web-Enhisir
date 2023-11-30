using System.Net;
using System.Text.Json;
using Application.Dtos;
using Application.Models;
using Application.Services;
using Database;
using Database.Entities;
using Database.Exceptions;
using Database.Extensions;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Enums;
using WebHelper.Exceptions;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Api;

public class ApiAnnouncementComponent : ApiComponent
{
    private readonly ImageHelper _imageHelper = new();
    private readonly DadataValidator _validator = new();
    
    public ApiAnnouncementComponent() : base("announcement") {}

    [Mapping(Method.GET, "/id/<id:uuid>")]
    public async Task GetById(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
            
        var announcement = await Announcement.GetByIdAsync(Guid.Parse(id));
        if (announcement is null)
        {
            ShowNotFound(context);
            return;
        }
            
        await JsonSerializer.SerializeAsync(
            context.Response.OutputStream, 
            announcement, 
            JsonSerializing.StandardOptions);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
    }
    
    [Mapping(Method.POST, "/id/<id:uuid>/change_privilege", Role.User)]
    public async Task ChangePrivilege(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
            
        var announcement = await Announcement.GetByIdAsync(Guid.Parse(id));
        if (announcement is null)
        {
            ShowNotFound(context);
            return;
        }
        
        if (!context.User.Login.Equals(announcement.OwnerId))
        {
            ShowForbidden(context);
            return;
        }

        try
        {
            announcement.IsPrivileged = !announcement.IsPrivileged;
            await announcement.SaveAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
        }
        catch (Exception)
        {
            ShowInternalServerError(context);
        }
    }

    [Mapping(Method.POST, "/new", Role.User)]
    public async Task AddAnnouncement(Context context)
    {
        try
        {
            var model = await JsonSerializer
                .DeserializeAsync<AddAnnouncementModel>(
                    context.Request.InputStream,
                    JsonSerializing.StandardOptions);
            
            if (model is null) throw new Exception();
            
            var city = await City.GetByIdAsync(model.CityId);
            
            if (city is null) throw new NotFoundException();
            
            EntityValidator.Validate(model);
            var announcement = new Announcement()
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Address = model.Address,
                City = city,
                OwnerId = context.User.Login
            };

            if (await _validator.ValidateFullAddressAsync(announcement.City.Id, model.Address))
            {
                ShowBadRequest(context);
                return;
            }
            
            if (await _imageHelper.SaveImageAsync(announcement.Id, model.Image))
            {
                await announcement.SaveAsync();
                await JsonSerializer.SerializeAsync(
                    context.Response.OutputStream,
                    new GuidDto(announcement.Id),
                    JsonSerializing.StandardOptions);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                ShowInternalServerError(context);
            }
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
    
    [Mapping(Method.POST, "/id/<id:uuid>/edit", Role.User)]
    public async Task EditAnnouncement(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
        
        var announcement = await Announcement.GetByIdAsync(Guid.Parse(id));
        if (announcement is null)
        {
            ShowNotFound(context);
            return;
        }
        
        if (!context.User.Login.Equals(announcement.OwnerId))
        {
            ShowForbidden(context);
            return;
        }
        
        try
        {
            var model = await JsonSerializer
                .DeserializeAsync<EditAnnouncementDto>(
                    context.Request.InputStream,
                    JsonSerializing.StandardOptions);
            if (model is null) throw new Exception();

            
            announcement.Description = model.Description ?? announcement.Description;
            announcement.Price = model.Price ?? announcement.Price;
            await announcement.SaveAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
    
    [Mapping(Method.POST, "/id/<id:uuid>/remove", Role.User)]
    public async Task DisableAnnouncement(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
        
        var announcement = await Announcement.GetByIdAsync(Guid.Parse(id));
        if (announcement is null)
        {
            ShowNotFound(context);
            return;
        }
        
        if (!context.User.Login.Equals(announcement.OwnerId))
        {
            ShowForbidden(context);
            return;
        }
        
        try
        {
            await announcement.DisableAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
    
    [Mapping(Method.GET, "/catalog")]
    public async Task GetCatalogue(Context context)
    {
        try
        {
            if (context.Request.Cookies["current-city"] is null)
                throw new Exception();
            
            var str = Uri.UnescapeDataString(context.Request.Cookies["current-city"]!.Value);
            var city = JsonSerializer.Deserialize<City>(str, JsonSerializing.StandardOptions);

            if (!(city is not null && await City.GetByIdAsync(city.Id) is not null))
                throw new ItemNotFoundException();
            
            var items = await Announcement.GetBy(null, city, null)!.ToListAsync();
        
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                items,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }

    [Mapping(Method.GET, "/search")]
    public async Task Search(Context context)
    {
        try
        {
            var title = context.Request.QueryString.Get("title");
            if (title is null || context.Request.Cookies["current-city"] is null)
                throw new Exception();
            
            var str = Uri.UnescapeDataString(context.Request.Cookies["current-city"]!.Value);
            var city = JsonSerializer.Deserialize<City>(str, JsonSerializing.StandardOptions);

            if (!(city is not null && await City.GetByIdAsync(city.Id) is not null))
                throw new ItemNotFoundException();
            
            var items = await Announcement.GetBy(null, city, title)!.ToListAsync();
        
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                items,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
}