using BotFramework.Db.Entity;

namespace MysterySantaBot.Database.Entities;

public class ModeratorLetterQueue : BaseBotEntity<long>
{
    /// <summary>
    /// Отправитель письма.
    /// </summary>
    public long LetterUserTelegramId { get; set; }
}
