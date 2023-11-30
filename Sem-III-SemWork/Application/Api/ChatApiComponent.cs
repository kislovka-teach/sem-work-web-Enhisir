using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Application.Dtos;
using Database;
using Database.Entities;
using Database.Extensions;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Enums;
using WebHelper.Exceptions;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Api;

public class ChatApiComponent : ApiComponent
{
    public ChatApiComponent() : base("chat") {}

    [Mapping(Method.POST, "/messages/new", Role.User)]
    public async Task SendMessage(Context context)
    {
        try
        {
            var user = (User)context.User;

            var dto =
                await JsonSerializer.DeserializeAsync<MessageDto>(
                    context.Request.InputStream,
                    JsonSerializing.StandardOptions)
                ?? throw new ValidationException();
            
            var announcement = 
                await Announcement.GetByIdAsync(dto.AnnouncementId) 
                ?? throw new ItemNotFoundException();

            if (dto.ChatId is null && announcement.OwnerId.Equals(user.Login))
                throw new BadRequestException();
            
            var chat = (await Chat.GetByAnnouncementAsync(dto.AnnouncementId).ToListAsync()) 
                .FirstOrDefault(ant => dto.ChatId is null 
                    ? ant.Consumer.Login.Equals(user.Login) || announcement.OwnerId.Equals(user.Login)
                    : ant.Id == dto.ChatId);

            if (chat is null)
            {
                if (announcement.OwnerId.Equals(user.Login))
                    throw new ForbiddenException();
                
                chat = new Chat(announcement, user);
                await chat.SaveAsync();
            }

            var message = new Message
            {
                ChatId = chat.Id,
                SenderId = user.Login,
                Content = dto.Text,
            };
            await message.SaveAsync();
            context.Response.StatusCode = (int)HttpStatusCode.Created;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case ForbiddenException:
                    ShowForbidden(context);
                    return;
                case BadRequestException:
                    ShowBadRequest(context);
                    return;
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
    
    [Mapping(Method.GET, "/id/<chatId:uuid>", Role.User)]
    public async Task GetChat(Context context)
    {
        try
        {
            var user = (User)context.User;
            
            if (context.UrlParams?.TryGetValue("chatId", out var chatIdString) is null or false)
                throw new Exception();

            var chatId = Guid.Parse(chatIdString);
            var chat = await Chat.GetByIdAsync(chatId) ?? throw new ItemNotFoundException();

            if (!chat.Announcement.OwnerId.Equals(user.Login) && !chat.Consumer.Login.Equals(user.Login))
                throw new ForbiddenException();
            
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                chat,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case ForbiddenException:
                    ShowForbidden(context);
                    return;
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
    
    [Mapping(Method.GET, "/id/<chatId:uuid>/messages", Role.User)]
    public async Task GetMessages(Context context)
    {
        var user = (User)context.User;
        try
        {
            if (context.UrlParams?.TryGetValue("chatId", out var chatIdString) is null or false)
                throw new Exception();
            
            var chat = await Chat.GetByIdAsync(Guid.Parse(chatIdString)) 
                       ?? throw new ItemNotFoundException();
            if (!chat.Announcement.OwnerId.Equals(user.Login) 
                && !chat.Consumer.Login.Equals(user.Login))
                throw new ForbiddenException();
            
            var filterDateTimeString = context.Request.QueryString.Get("filterDateTime");
            var filterIsValid = DateTime.TryParse(filterDateTimeString, out var filterDateTime);
            
            var messages = 
                await Message.GetByChatAsync(
                    chat, 
                    filterIsValid ? filterDateTime : null).ToListAsync();
            
            await JsonSerializer.SerializeAsync(
                context.Response.OutputStream,
                messages,
                JsonSerializing.StandardOptions);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case ForbiddenException:
                    ShowForbidden(context);
                    return;
                case ItemNotFoundException:
                    ShowNotFound(context);
                    return;
                case JsonException or ValidationException:
                    ShowBadRequest(context);
                    return;
                default:
                    ShowInternalServerError(context);
                    return;
            }
        }
    }
}