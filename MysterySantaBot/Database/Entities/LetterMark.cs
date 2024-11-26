namespace MysterySantaBot.Database.Entities;

public class LetterMark : BaseEntity<long>
{
    /// <summary>
    /// Кто поставил оценку
    /// </summary>
    public long UserTelegramId { get; set; }
    
    /// <summary>
    /// Кому поставили оценку
    /// </summary>
    public long LetterUserTelegramId { get; set; }
    
    /// <summary>
    /// Оценка
    /// </summary>
    public LetterMarkType Mark { get; set; }
}
