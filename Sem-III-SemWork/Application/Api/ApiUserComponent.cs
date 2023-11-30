using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Application.Dtos;
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


public class ApiUserComponent : ApiComponent
{
    public ApiUserComponent() : base("user") {}
    
    [Mapping(Method.GET, "/me", Role.User)]
    public async Task GetMe(Context context)
    {
        var user = (context.User as User)!;
        var dto = new UserInfoDto(
            user.Login,
            user.Name,
            user.City);
            
        await JsonSerializer.SerializeAsync(
            context.Response.OutputStream, 
            dto, 
            JsonSerializing.StandardOptions);
        
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
    }

    [Mapping(Method.GET, "/me/announcements", Role.User)]
    public async Task GetMyAnnouncements(Context context)
    {
        var user = (context.User as User)!;
        var items = await Announcement.GetByUserAsync(user.Login).ToListAsync();
    }

    [Mapping(Method.PATCH, "/me/edit", Role.User)]
    public async Task ChangeUser(Context context)
    {
        try
        {
            var dto =
                await JsonSerializer.DeserializeAsync<UserChangeDto>(
                    context.Request.InputStream,
                    JsonSerializing.StandardOptions);
            
            var city = await City.GetByNameAsync(dto.City) ?? throw new ValidationException();
            
            var user = (context.User as User)!;
            user.Name = dto.Name;
            user.City = await City.GetByIdAsync(city.Id) ?? throw new ItemNotFoundException();
            await user.SaveAsync();
            
            //updating session data
            Session.GetById(context.Request.Cookies["session-id"]?.Value)!.Save();
            
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentLength64 = 0;
        }
        catch (Exception ex) when(ex is JsonException or ValidationException)
        {
            ShowBadRequest(context);
        }
        catch (Exception)
        {
            ShowInternalServerError(context);
        }
    }
    
    [Mapping(Method.GET, "/id/<login:string>")]
    public async Task GetUser(Context context)
    {
        if (context.UrlParams?.TryGetValue("login", out var login) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
        var user = await User.GetByLoginAsync(login.ToLower());
        if (user is null)
        {
            ShowNotFound(context);
            return;
        }
        
        var dto = new UserInfoDto(
            user.Login,
            user.Name,
            user.City);
            
        await JsonSerializer.SerializeAsync(
            context.Response.OutputStream, 
            dto, 
            JsonSerializing.StandardOptions);
        
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
    }
    
    [Mapping(Method.GET, "/id/<login:string>/announcements")]
    public async Task GetAnnouncementsByUser(Context context)
    {
        try
        {
            if (context.UrlParams?.TryGetValue("login", out var login) is null or false)
            {
                ShowInternalServerError(context);
                return;
            }
            var user = await User.GetByLoginAsync(login.ToLower()) ?? throw new NotFoundException();
            
            var items = await Announcement.GetByUserAsync(user.Login).ToListAsync();
        
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                items,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case ItemNotFoundException:
                    ShowNotFound(context);
                    break;
                case JsonException or ValidationException:
                    ShowBadRequest(context);
                    break;
                default:
                    ShowInternalServerError(context);
                    break;
            }
        }
    }
    
    [Mapping(Method.GET, "/me/chats", Role.User)]
    public async Task GetChatsByUser(Context context)
    {
        try
        {
            var user = (User)context.User;
            var chats = await Chat.GetByUserAsync(user.Login).ToListAsync();
        
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                chats,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case ItemNotFoundException:
                    ShowNotFound(context);
                    break;
                case JsonException or ValidationException:
                    ShowBadRequest(context);
                    break;
                default:
                    ShowInternalServerError(context);
                    break;
            }
        }
    }
}