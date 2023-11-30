using Database.Entities;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Enums;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Frontend;

public class ShowAnnouncementComponent : ShowComponent
{
    public ShowAnnouncementComponent() 
        : base("announcement") {}
    
    [Mapping(Method.GET, "new", Role.User)]
    public async Task AddAnnouncement(Context context)
        => await RenderPage(context, "static/html/addAnnouncement.html");

    [Mapping(Method.GET, "id/<id:uuid>/edit", Role.User)]
    public async Task EditAnnouncement(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
        {
            ShowInternalServerError(context);
            return;
        }
        var announcement = await Announcement.GetByIdAsync(Guid.Parse(id));
        if (announcement is null)
            ShowNotFound(context);
        else if (!context.User.Login.Equals(announcement.OwnerId))
            ShowForbidden(context); 
        else
            await RenderPage(context, "static/html/editAnnouncement.html");
    }

    [Mapping(Method.GET, "id/<id:uuid>")]
    public async Task GetById(Context context)
    {
        if (context.UrlParams?.TryGetValue("id", out var id) is null or false)
            ShowInternalServerError(context);
        else if (!Guid.TryParse(id, out var guidId))
            ShowBadRequest(context);
        else if (await Announcement.GetByIdAsync(guidId) is null)
            ShowNotFound(context);
        else
            await RenderPage(context, "static/html/announcementTemplate.html");
    }
}