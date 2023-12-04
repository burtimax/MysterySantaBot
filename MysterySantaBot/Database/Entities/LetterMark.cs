using BotFramework.Db.Entity;

namespace MysterySantaBot.Database.Entities;

public class ShowHistory : BaseBotEntity<long>
{
    public long UserTelegramId { get; set; }
    public long ShownUserTelegramId { get; set; }
    public DateTime LastShown { get; set; }
}