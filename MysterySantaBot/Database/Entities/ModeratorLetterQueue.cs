
namespace MysterySantaBot.Database.Entities;

public class ModeratorLetterQueue : BaseEntity<long>
{
    /// <summary>
    /// Отправитель письма.
    /// </summary>
    public long LetterUserTelegramId { get; set; }
}
