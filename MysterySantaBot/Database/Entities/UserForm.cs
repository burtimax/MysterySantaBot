
namespace MysterySantaBot.Database.Entities;

public class UserForm : BaseEntity<long>
{
    public long UserTelegramId { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Photo { get; set; }
    public int? ChosenByOthersCount { get; set; }
    public string? Description { get; set; }
    public string? Contact { get; set; }
    public bool IsHidden { get; set; }
}