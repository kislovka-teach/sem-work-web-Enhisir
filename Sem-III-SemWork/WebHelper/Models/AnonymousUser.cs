using WebHelper.Enums;

namespace WebHelper.Models;

public class AnonymousUser : AbstractUser
{
    public static readonly AnonymousUser Instance = new();
    
    private AnonymousUser() { Role = Role.Guest; }
}