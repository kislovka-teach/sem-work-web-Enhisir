using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Text.Json.Serialization;
using WebHelper.Auth;
using WebHelper.Enums;
using WebHelper.Tools;

namespace WebHelper.Models;

public abstract class AbstractUser
{
    [Required]
    public string Login { get; init; }
    
    [JsonIgnore]
    [Required]
    public string Password { get; private set; }
    
    [JsonIgnore]
    [Required]
    public virtual Role Role { get; set; }
    
    [JsonConstructor]
    protected AbstractUser() {}
    
    protected AbstractUser(DbDataReader reader)
    {
        Login = reader.GetString("login");
        Password = reader.GetString("password");
    }
    
    public void SetPassword(string pwd) 
        => Password = PasswordHasher.Hash(pwd);

    public Session CreateSession()
    {
        var session = new Session() { Value = this };
        session.Save();
        return session;
    }
    
    public bool ComparePassword(string maybePassword)
        => PasswordHasher.Validate(Password, maybePassword);
}