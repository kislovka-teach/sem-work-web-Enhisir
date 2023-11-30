using System.Net;
using Database.Entities;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Api;

public class ApiImageComponent : ApiComponent
{
    public ApiImageComponent() : base("image") {}
    
    [Mapping(Method.GET, "/<id:uuid>")]
    public async Task GetImageByAnnouncementId(Context context)
        => await GetImageByIdWithParams(context);

    [Mapping(Method.GET, "/<id:uuid>/thumb")]
    public async Task GetThumbByAnnouncementId(Context context)
        => await GetImageByIdWithParams(context, true);

    private async Task GetImageByIdWithParams(Context context, bool isThumb = false)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }

        var guidId = Guid.Parse(id);
        
        if (await Announcement.GetByIdAsync(guidId) is null)
        {
            ShowNotFound(context);
            return;
        }
        
        var path = $"Cache/{guidId.ToString()}{(isThumb ? "_thumb" : string.Empty)}.jpg";
        if (!new FileInfo(path).Exists)
        {
            ShowInternalServerError(context);
            return;
        }
        
        var imageBytes = await File.ReadAllBytesAsync(path);
        await context.Response.OutputStream.WriteAsync(imageBytes);
        
        context.Response.ContentType = "image/jpg";
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
}