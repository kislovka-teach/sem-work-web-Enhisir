using System.Text;
using WebHelper.Tools;

namespace WebHelper.Components;

/// <summary>
/// Компонент для загрузки всех статических файлов сайта, не относящихся к логике.
/// Занимает весь поддомен /static
/// Крайне не рекомендуется получать с его помощью html-файлы 
/// </summary>
internal sealed class ShowStaticComponent : ShowComponent
{
    public ShowStaticComponent() : base("static") {}

    public override async Task Manage(Context context)
    {
        if (context.Request.HttpMethod == "POST") ShowForbidden(context);

        await ShowStatic(context);
    }
    
    private async Task ShowStatic(Context context)
    {
        var filePath = $"{context.Request.Url!.LocalPath.Trim('/')}";
        if (!new FileInfo(filePath).Exists)
        {
            ShowNotFound(context);
            return;
        }
        
        var extension = Path.GetExtension(filePath);
        context.Response.ContentType = extension switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            _ => "text/plain",
        };
        var file = await File.ReadAllBytesAsync(filePath);
        context.Response.ContentEncoding = Encoding.UTF8;
        await context.Response.OutputStream.WriteAsync(file);
    }
    
}