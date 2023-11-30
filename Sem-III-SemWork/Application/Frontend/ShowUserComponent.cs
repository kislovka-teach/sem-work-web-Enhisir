using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Frontend;

public class ShowUserComponent : ShowComponent
{
    public ShowUserComponent() 
        : base("user") {}
    
    [Mapping(Method.GET, "<login:string>")]
    public async Task Register(Context context)
        => await RenderPage(context, "static/html/userPage.html");
}