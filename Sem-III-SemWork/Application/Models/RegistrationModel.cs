using System.ComponentModel.DataAnnotations;

namespace Application.Models;

public class RegistrationModel
{
    [Required]
    public string Name { get; set; }
    
    [RegularExpression(@"^([a-zA-Z\d_-]).{6,}$")]
    public string Login { get; set; }
    
    [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[/\\.^*+?!()\[\]{}|]).{8,}$")]
    public string Password { get; set; }
    
    [Required]
    public Guid CityId { get; set; }
}