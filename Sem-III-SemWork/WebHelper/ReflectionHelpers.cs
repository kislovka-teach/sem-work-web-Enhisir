using WebHelper.Attributes;

namespace WebHelper;

public static class ReflectionHelpers
{
    private static string GetCustomDescription(object objEnum)
    {
        var fi = objEnum.GetType().GetField(objEnum.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return (attributes.Length > 0 ? attributes[0] : objEnum).ToString()!;
    }

    public static string Description(this Enum value) 
        => GetCustomDescription(value);
}