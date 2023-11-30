using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Frontend;

public class ShowAuthComponent : ShowComponent
{
    public ShowAuthComponent() 
        : base("auth") {}

    [Mapping(Method.GET, "login")]
    public async Task Login(Context context)
        => await RenderPage(context, "static/html/login.html");
    
    [Mapping(Method.GET, "register")]
    public async Task Register(Context context)
        => await RenderPage(context, "static/html/register.html");
    
    
}