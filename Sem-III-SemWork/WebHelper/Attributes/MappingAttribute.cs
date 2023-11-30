using WebHelper.Enums;
using WebHelper.Models;

namespace WebHelper.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MappingAttribute : Attribute
{
    public Method Method;
    public string Route;
    public Role Role;
    
    public MappingAttribute(Method method, string route, Role role = Role.Guest)
    {
        Route = route;
        Method = method;
        Role = role;
    }
}
