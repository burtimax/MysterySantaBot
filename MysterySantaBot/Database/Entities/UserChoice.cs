using BotFramework.Db.Entity;

namespace MysterySantaBot.Database.Entities;

public class UserChoice : BaseBotEntity<long>
{
    public long UserTelegramId { get; set; }
    public long ChosenUserTelegramId { get; set; }
}