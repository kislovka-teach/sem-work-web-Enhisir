using System.ComponentModel.DataAnnotations;

namespace Application.Models;
    
public class AddAnnouncementModel
{
    [Required] public string Title { get; set; } = null!;
    [Required] public string Description { get; set; } = null!;
    [Required] public decimal Price { get; set; }
    [Required] public Guid CityId { get; set; }
    [Required] public string Address { get; set; } = null!;
    [Required] public string Image { get; set; }
}