namespace Application.Dtos;

public class MessageDto
{
    public Guid? ChatId { get; set; }
    public Guid AnnouncementId { get; set; }
    public string Text { get; set; }
}