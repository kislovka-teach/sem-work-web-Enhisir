using System.Net;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace WebHelper;

public class HttpWebApp
{
    private readonly Lazy<HttpListener> _listener = new();
    private HttpListener Listener => _listener.Value;
    private readonly List<Component> _components = new() { new ShowStaticComponent() };
    
    public readonly string Address;
    public readonly int Port;

    public HttpWebApp(string address, int port)
    {
        Address = address;
        Port = port;
    }

    public void AddComponent(Component comp)
    {
        if (_components.Contains(comp, new ComponentComparer()))
        {
            throw new InvalidOperationException();
        }
        _components.Add(comp);
        _components.Sort(
            (l, r) 
                => -string.Compare(l.Domain, r.Domain, StringComparison.Ordinal));
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"Starting server on http://{Address}:{Port}");
        Listener.Prefixes.Add($"http://{Address}:{Port}/");
        Listener.Start();
        while (Listener.IsListening)
        {
            var context = await Listener.GetContextAsync();
            var session = Session.GetById(context.Request.Cookies["session-id"]?.Value);
            
            var myContext = new Context(context)
            {
                User = session?.Value ?? AnonymousUser.Instance
            };

            if (myContext.Path is not null)
            {
                var visited = false;
                foreach (var comp in _components.Where(comp => myContext.Path.StartsWith(comp.Domain)))
                {
                    await comp.Manage(myContext);
                    visited = true;
                    break;
                }

                if (!visited)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentLength64 = 0;
                }
            }
            try
            {
                context.Response.Close();
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                Console.WriteLine($"HTTP {context.Request.HttpMethod} {context.Request.RawUrl ?? "undefined"}" +
                                  $" - {context.Response.StatusCode}");
            }
        }
    }
}