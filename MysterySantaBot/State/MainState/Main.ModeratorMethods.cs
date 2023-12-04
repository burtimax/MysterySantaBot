using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MysterySantaBot.Database.Entities;

namespace MysterySantaBot.State.SetDescriptionState;

public partial class Main
{
    private async Task<long?> GetNextUserTelegramIdForModeration()
    {
        return await Db.ModeratorLetterQueue
            .OrderBy(q => q.CreatedAt)
            .Select(q => q.LetterUserTelegramId)
            .FirstOrDefaultAsync();
    }

    private async Task<ModeratorLetterQueue?> GetModeratorLetterQueueByUserTelegramId(long userTelegramId)
    {
        return await Db.ModeratorLetterQueue.FirstOrDefaultAsync(q => q.LetterUserTelegramId == userTelegramId);
    }
    
    private async Task RemoveFromModerationQueue(long letterUserTelegramId)
    {
        var item = await GetModeratorLetterQueueByUserTelegramId(letterUserTelegramId);
        if (item is null) return;
        
        Db.ModeratorLetterQueue.Remove(item);
        await Db.SaveChangesAsync();
    }
    
    private async Task AddToModerationQueue(long letterUserTelegramId)
    {
        var item = await GetModeratorLetterQueueByUserTelegramId(letterUserTelegramId);
        if (item is not null) return;

        item = new ModeratorLetterQueue()
        {
            CreatedAt = DateTimeOffset.Now,
            LetterUserTelegramId = letterUserTelegramId,
        };
        
        Db.ModeratorLetterQueue.Add(item);
        await Db.SaveChangesAsync();
    }
}