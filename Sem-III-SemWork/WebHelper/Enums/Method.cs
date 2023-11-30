using WebHelper.Attributes;

namespace WebHelper.Models;

[Flags]
public enum Method
{
    [Description("GET")]
    GET,
    
    [Description("POST")]
    POST,
    
    [Description("PATCH")]
    PATCH,
    
    [Description("DELETE")]
    DELETE
}