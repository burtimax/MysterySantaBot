
namespace MysterySantaBot.Database.Entities;

public class ShowHistory : BaseEntity<long>
{
    public long UserTelegramId { get; set; }
    public long ShownUserTelegramId { get; set; }
    public DateTime LastShown { get; set; }
}