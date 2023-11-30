using System.Net;
using System.Text.RegularExpressions;
using WebHelper.Attributes;
using WebHelper.Tools;

namespace WebHelper.Components;

public abstract class Component
{
    private readonly List<(Regex route, MappingAttribute mapping, Func<Context, Task>)> _methodsPointers = new();
    public readonly string Domain;

    protected Component(string domain)
    {
        Domain = $"/{domain.Trim('/')}";
        
        var methods = GetType()
                                            .GetMethods()
                                            .Where(info => info.GetCustomAttributes(false)
                                                                .Any(attr => attr is MappingAttribute));
        foreach (var method in methods)
        {
            var attr = (MappingAttribute)method.GetCustomAttributes(false)
                                        .First(attr => attr is MappingAttribute);
            
            // converting route to regex string
            var rgxRoute = $"{Domain}/{RouteHelper.RouteToRegexString(attr.Route.Trim('/'))}".Trim('/');
            _methodsPointers.Add(
                (new Regex($"^/{rgxRoute}$"), attr, async context => 
                {
                    var result = (Task)method.Invoke(this, new object?[] { context })!;
                    await result;
                }));
        }
    }
    
    public virtual async Task Manage(Context context)
    {
        if (context.Path is null) return;
        
        foreach (var (availableRoute, mapping, action) in _methodsPointers
                     .Where(x => x.mapping.Method.Description() == context.Request.HttpMethod))
        {
            // check for route
            var match = availableRoute.Match(context.Path!);
            if (!match.Success 
                || context.Request.HttpMethod != mapping.Method.ToString()) continue;

            // check for actor role
            if ((mapping.Role & context.User.Role) != mapping.Role)
            {
                ShowUnauthorized(context);
                return;
            }
            
            // get url params
            context.UrlParams = RouteHelper.GetUrlParams(availableRoute, context.Path);
            
            await action(context);
            return;
        }
        
        ShowNotFound(context);
    }
    
    protected static void ShowNotFound(Context context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentLength64 = 0;
    }
    
    protected static void ShowForbidden(Context context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentLength64 = 0;
    }
    
    protected static void ShowUnauthorized(Context context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentLength64 = 0;
    }

    protected static void ShowBadRequest(Context context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentLength64 = 0;
    }

    protected static void ShowInternalServerError(Context context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentLength64 = 0;
    }
}