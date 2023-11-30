using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Enums;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Frontend;

public class ShowMainComponent : ShowComponent
{
    public ShowMainComponent() 
        : base("") {}

    [Mapping(Method.GET, "/")]
    public async Task ShowHome(Context context)
        => await RenderPage(context, "static/html/catalog.html");
    
    [Mapping(Method.GET, "/search")]
    public async Task ShowSearch(Context context)
        => await RenderPage(context, "static/html/search.html");

    [Mapping(Method.GET, "/chats", Role.User)]
    public async Task ShowChats(Context context)
        => await RenderPage(context, "static/html/chats.html");
}