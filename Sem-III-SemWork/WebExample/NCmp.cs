using System.Text;
using WebHelper;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace WebExample;

public class NCmp : Component
{
    public NCmp(string domain) : base(domain) {}

    [Mapping(Method.GET, "/")]
    public async Task GetMain(Context context)
    {
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Hello World!"));
    }
    
    [Mapping(Method.GET, "/user/<login:string>/users")]
    public async Task GetUser(Context context)
    {
        if (context.UrlParams is null)
        {
            ShowNotFound(context);
            return; 
        }

        try
        {
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes($"{context.UrlParams["id"]}"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}