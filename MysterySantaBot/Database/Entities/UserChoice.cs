
namespace MysterySantaBot.Database.Entities;

public class UserChoice : BaseEntity<long>
{
    public long UserTelegramId { get; set; }
    public long ChosenUserTelegramId { get; set; }
}