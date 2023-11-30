using System.Net;
using WebHelper.Models;

namespace WebHelper.Tools;

public class Context
{
    public string? Path { get; }
    public HttpListenerRequest Request { get; }
    public HttpListenerResponse Response { get; }
    public AbstractUser User { get; set; }
    public Dictionary<string, string>? UrlParams { get; set; }

    public Context(HttpListenerContext context)
    {
        Path = context.Request.Url?.LocalPath.TrimEnd('/');
        if (Path == "") Path = "/";
        
        Request = context.Request;
        Response = context.Response;
        User ??= AnonymousUser.Instance;
    }
}