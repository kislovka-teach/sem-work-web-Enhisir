using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace WebHelper.Tools;

public static class JsonSerializing
{
    private class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
                return name;

            return name.ToLower();
        }
    }
    
    public static readonly JsonSerializerOptions StandardOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true,
    };
}