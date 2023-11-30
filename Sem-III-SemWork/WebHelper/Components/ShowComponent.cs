using System.Net;
using WebHelper.Tools;

namespace WebHelper.Components;

public abstract class ShowComponent : Component
{
    protected ShowComponent(string domain) : base(domain) {}

    public override async Task Manage(Context context)
    {
        await base.Manage(context);
        context.Response.ContentType = "text/html; charset=utf-8";
    }
    
    protected async Task RenderPage(Context context, string pagePath)
    {
        if (context.Request.HttpMethod == "POST") ShowForbidden(context);

        try
        {
            var file = await File.ReadAllBytesAsync(pagePath); 
            await context.Response.OutputStream.WriteAsync(file);
            
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html";
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }
}