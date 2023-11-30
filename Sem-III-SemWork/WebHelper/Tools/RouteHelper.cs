using System.Text.RegularExpressions;

namespace WebHelper.Tools;

public static partial class RouteHelper
{
    // http://localhost:5500/users/<id:int>/
    private static Dictionary<string, string> _types = new()
    {
        {"int", "[0-9]+"},
        {"string", @"[\w\d_\-]+"},
        {"uuid", "[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?"}
    };
    
    public static string RouteToRegexString(string route)
    {
        return ParamFinder()
            .Replace(
                route,
                match => $"(?<{match.Groups["name"].Value}>{_types[match.Groups["type"].Value]})");
    }

    public static Dictionary<string, string>? GetUrlParams(Regex regexRoute, string currentRoute)
    {
        var match = regexRoute.Match(currentRoute);

        return !match.Success 
            ? null 
            : match.Groups.Values
                .Skip(1)
                .ToDictionary(g => g.Name, g => g.Value.Trim('/'));
    }

    [GeneratedRegex("<(?<name>[a-zA-Z]+):(?<type>[a-zA-Z]+)>")]
    private static partial Regex ParamFinder();
}