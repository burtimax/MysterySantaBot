using Microsoft.EntityFrameworkCore;
using MultipleBotFramework;
using MultipleBotFramework.Constants;
using MultipleBotFramework.Utils;
using MultipleBotFramework.Utils.Keyboard;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.UpdatingMessages;

namespace MysterySantaBot.State.SetDescriptionState;

public partial class Main
{
    public override async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {

        // Inline кнопка добавить письмо в избранные
        if (callbackQuery.Data.StartsWith(MainRes.InlineKeyAddToChoiceList))
        {
            long chosenUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineKeyAddToChoiceList, ""));

            if (await GetChoiceListCount() >= AppConstants.MaxChosen)
            {
                await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                    string.Format(r.ExceedLimitInChoiceList, AppConstants.MaxChosen.ToString()), true);
                return;
            }

            await AddToChoiceList(chosenUserTelegramId);

            InlineKeyboardBuilder inlineForUpdate = User.AdditionalProperties.Contains(MainRes.PropKeyChoiceListWasLastButton) 
                ? r.GetInlineUnderChoiceListForUser(User, UserClaims, chosenUserTelegramId, 
                    await GetChoiceListCount(), 
                    await HasChatExisted(BotClient, chosenUserTelegramId), 
                    false)
                : r.GetInlineUnderLetterForUser(User, UserClaims, chosenUserTelegramId, await HasChatExisted(BotClient, chosenUserTelegramId), false);
            
            await BotClient.EditMessageReplyMarkupAsync(Chat.ChatId, callbackQuery.Message.MessageId,
                replyMarkup: inlineForUpdate.Build());
            
            UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
            await SearchAndShowNextLetter(existed);
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await NotifyUserAddedToChoiceList(chosenUserTelegramId);
            return;
        }
        
        // Inline кнопка убрать из избранных.
        if (callbackQuery.Data.StartsWith(MainRes.InlineRemoveFromChoiceList))
        {
            long chosenUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineRemoveFromChoiceList, ""));

            if (await GetChoiceListCount() == 0)
            {
                await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                    r.ChoiceListIsEmpty, true);
                return;
            }

            await RemoveFromChoiceList(chosenUserTelegramId);

           InlineKeyboardBuilder inlineForUpdate = User.AdditionalProperties.Contains(MainRes.PropKeyChoiceListWasLastButton) 
                ? r.GetInlineUnderChoiceListForUser(User, UserClaims, chosenUserTelegramId, await GetChoiceListCount(), 
                    await HasChatExisted(BotClient, chosenUserTelegramId),
                    true)
                : r.GetInlineUnderLetterForUser(User, UserClaims, chosenUserTelegramId, await HasChatExisted(BotClient, chosenUserTelegramId), true);
            
            await BotClient.EditMessageReplyMarkupAsync(Chat.ChatId, callbackQuery.Message.MessageId,
                replyMarkup: inlineForUpdate.Build());
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }
        
        if (callbackQuery.Data.StartsWith(MainRes.InlineShowPrevious))
        {
            long userTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineShowPrevious, ""));

            List<UserChoice> choiceList = await GetChoiceList();
            
            int prev = 0;
            for (int i = 0; i < choiceList.Count; i++)
            {
                if (choiceList[i].ChosenUserTelegramId == userTelegramId)
                {
                    prev = i == 0 ? choiceList.Count - 1 : i - 1;
                    break;
                }
            }
            UserChoice previous = choiceList[prev];
            UserForm prevUserForm = Db.UsersForm.First(uf => uf.UserTelegramId == previous.ChosenUserTelegramId);
            //await BotClient.DeleteMessageAsync(Chat.ChatId, callbackQuery.Message.MessageId);
            //await ShowLetterFromChoiceList(prevUserForm);
            var data = await GetDataForLetter(prevUserForm, false);
            var inline = r.GetInlineUnderChoiceListForUser(User, UserClaims, prevUserForm.UserTelegramId, await GetChoiceListCount(), 
                await HasChatExisted(BotClient, prevUserForm.UserTelegramId),false);
            await BotClient.EditMessageMediaAsync(Chat.ChatId, callbackQuery.Message.MessageId, new InputMediaPhoto(prevUserForm.Photo!) );
            await BotClient.EditMessageCaptionAsync(Chat.ChatId, callbackQuery.Message.MessageId, data.caption, ParseMode.Html);
            await BotClient.EditMessageReplyMarkupAsync(Chat.ChatId, callbackQuery.Message.MessageId,
                replyMarkup: inline.Build());
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }
        
        if (callbackQuery.Data.StartsWith(MainRes.InlineShowNext))
        {
            long userTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineShowNext, ""));

            List<UserChoice> choiceList = await GetChoiceList();
            
            int next = 0;
            for (int i = 0; i < choiceList.Count; i++)
            {
                if (choiceList[i].ChosenUserTelegramId == userTelegramId)
                {
                    next = i == choiceList.Count - 1 ? 0 : i + 1;
                    break;
                }
            }
            UserChoice previous = choiceList[next];
            UserForm nextUserForm = Db.UsersForm.First(uf => uf.UserTelegramId == previous.ChosenUserTelegramId);
            // await BotClient.DeleteMessageAsync(Chat.ChatId, callbackQuery.Message.MessageId);
            // await ShowLetterFromChoiceList(nextUserForm);
            var data = await GetDataForLetter(nextUserForm, false);
            var inline = r.GetInlineUnderChoiceListForUser(User, UserClaims, nextUserForm.UserTelegramId, await GetChoiceListCount(),
                await HasChatExisted(BotClient, nextUserForm.UserTelegramId),false);
            await BotClient.EditMessageMediaAsync(Chat.ChatId, callbackQuery.Message.MessageId, new InputMediaPhoto(nextUserForm.Photo));
            await BotClient.EditMessageCaptionAsync(Chat.ChatId, callbackQuery.Message.MessageId, data.caption, ParseMode.Html);
            await BotClient.EditMessageReplyMarkupAsync(Chat.ChatId, callbackQuery.Message.MessageId, 
                replyMarkup: inline.Build());
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }
        
        // Inline кнопка поставить лайк письму
        if (callbackQuery.Data.StartsWith(MainRes.InlineLikeLetter))
        {
            long letterUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineLikeLetter, ""));

            await SetMarkToUserLetter(letterUserTelegramId, LetterMarkType.Like);
            UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
            await SearchAndShowNextLetter(existed);
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }
        
        // Inline кнопка поставить дизлайк письму
        if (callbackQuery.Data.StartsWith(MainRes.InlineDislikeLetter))
        {
            long letterUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineDislikeLetter, ""));

            await SetMarkToUserLetter(letterUserTelegramId, LetterMarkType.Dislike);
            UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
            await SearchAndShowNextLetter(existed);
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }
        
        // Inline кнопка поставить дизлайк письму
        if (callbackQuery.Data.StartsWith(MainRes.InlineEditMyLetter))
        {
            UserForm me = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
            await GoToEditLetter(me);
            await BotClient.DeleteMessageAsync(Chat.ChatId, callbackQuery.Message.MessageId);
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        
        // Inline кнопка поставить дизлайк письму
        if (callbackQuery.Data.StartsWith(MainRes.InlineDeleteMessage))
        {
            await BotClient.DeleteMessageAsync(Chat.ChatId, callbackQuery.Message.MessageId);
            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        
        // Inline кнопка модератора подтвердить письмо
        if (callbackQuery.Data.StartsWith(MainRes.InlineAllowLetter) &&
            UserClaims.Any(uc => uc.Name == BotConstants.BaseBotClaims.BotUserGet))
        {
            long letterUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineAllowLetter, ""));
            UserForm? userForm = await Db.UsersForm.FirstOrDefaultAsync(uf => uf.UserTelegramId == letterUserTelegramId);
            if (userForm is not null && userForm.IsHidden)
            {
                userForm.IsHidden = false;
                Db.UsersForm.Update(userForm);
                await Db.SaveChangesAsync();
            }
            
            ModeratorLetterQueue? moderatorQueueItem = await GetModeratorLetterQueueByUserTelegramId(letterUserTelegramId);
            if (moderatorQueueItem is not null)
            {
                await RemoveFromModerationQueue(letterUserTelegramId);
            }

            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await Answer(r.SuccessLetterAllowed);
            return;
        }
        
        // Inline кнопка модератора отклонить письмо
        if (callbackQuery.Data.StartsWith(MainRes.InlineForbidLetter) &&
            UserClaims.Any(uc => uc.Name == BotConstants.BaseBotClaims.BotUserGet))
        {
            long letterUserTelegramId = long.Parse(callbackQuery.Data.Replace(MainRes.InlineForbidLetter, ""));
            UserForm? userForm = await Db.UsersForm.FirstOrDefaultAsync(uf => uf.UserTelegramId == letterUserTelegramId);
            if (userForm is not null)
            {
                userForm.IsHidden = true;
                Db.UsersForm.Update(userForm);
                await Db.SaveChangesAsync();
            }
            
            ModeratorLetterQueue? moderatorQueueItem = await GetModeratorLetterQueueByUserTelegramId(letterUserTelegramId);
            if (moderatorQueueItem is not null)
            {
                await RemoveFromModerationQueue(letterUserTelegramId);
            }

            await BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            // Уведомление для пользователя.
            await BotHelper.ExecuteFor(BotDbContext, AppConstants.BotId, letterUserTelegramId, async tuple =>
            {
                await BotClient.SendMessageAsync(tuple.chat.ChatId, r.YourLetterWasForbiddenByModeratorNotification);
            });
            await Answer(r.SuccessLetterForbid);
            return;
        }
    }

    /// <summary>
    /// Уведомляем пользователя, что его письмо было добавлено в избранные кем-то.
    /// </summary>
    /// <param name="letterUserTelegramId"></param>
    private async Task NotifyUserAddedToChoiceList(long letterUserTelegramId)
    {
        await BotHelper.ExecuteFor(BotDbContext, AppConstants.BotId, letterUserTelegramId, async tuple =>
        {
            await BotClient.SendMessageAsync(tuple.chat.ChatId, R.Notification.YourLetterWasAddedToChoiceList);
        });
    }
    
    private Task<int> GetChoiceListCount()
    {
        return Db.UserChoices.Where(uc => uc.UserTelegramId == User.TelegramId).CountAsync();
    }

    private async Task<List<UserChoice>> GetChoiceList()
    {
        return (await Db.UserChoices.Where(uc => uc.UserTelegramId == User.TelegramId).ToListAsync())
            .OrderBy(uc => uc.CreatedAt)
            .ToList();
    }

    private async Task AddToChoiceList(long chosenUserTelegramId)
    {
        UserChoice? existed = await Db.UserChoices.FirstOrDefaultAsync(uc => uc.UserTelegramId == User.TelegramId && uc.ChosenUserTelegramId == chosenUserTelegramId);

        if (existed != null) return;
        
        UserChoice newChoice = new()
        {
            UserTelegramId = User.TelegramId,
            ChosenUserTelegramId = chosenUserTelegramId,
            CreatedAt = DateTimeOffset.Now,
        };

        Db.UserChoices.Add(newChoice);
        await Db.SaveChangesAsync();
    }
    
    private async Task RemoveFromChoiceList(long chosenUserTelegramId)
    {
        UserChoice? existed = await Db.UserChoices.FirstOrDefaultAsync(uc => uc.UserTelegramId == User.TelegramId && uc.ChosenUserTelegramId == chosenUserTelegramId);

        if (existed == null) return;
        
        Db.UserChoices.Remove(existed);
        await Db.SaveChangesAsync();
    }

    private async Task<LetterMark?> GetMarkForLetter(long letterUserTelegramId)
    {
        return await Db.LetterMarks.FirstOrDefaultAsync(lm =>
            lm.UserTelegramId == User.TelegramId && lm.LetterUserTelegramId == letterUserTelegramId);
    }
    
    private async Task SetMarkToUserLetter(long letterUserTelegramId, LetterMarkType markType)
    {
        LetterMark? existed = await GetMarkForLetter(letterUserTelegramId);

        if (existed == null)
        {
            LetterMark newMark = new()
            {
                UserTelegramId = User.TelegramId,
                LetterUserTelegramId = letterUserTelegramId,
                Mark = markType,
                CreatedAt = DateTimeOffset.Now,
            };
            Db.LetterMarks.Add(newMark);
            await Db.SaveChangesAsync();
            return;
        }

        existed.Mark = markType;
        existed.UpdatedAt = DateTimeOffset.Now;
        Db.LetterMarks.Update(existed);
        await Db.SaveChangesAsync();
    }

    private async Task<(int Like, int Dislike)> CountLetterMarks(long letterUserTelegramId)
    {
        var marks = await Db.LetterMarks
            .Where(lm => lm.LetterUserTelegramId == letterUserTelegramId)
            .Select(lm => new
            {
                Mark = lm.Mark
            })
            .ToListAsync();

        int likesCount = marks.Count(lm => lm.Mark == LetterMarkType.Like);
        int dislikesCount = marks.Count(lm => lm.Mark == LetterMarkType.Dislike);
        return (likesCount, dislikesCount);
    }

    /// <summary>
    /// Сколько раз письмо пользователя было выбрано другими пользователями.
    /// </summary>
    /// <param name="letterUserTelegramId">ID владельца письма.</param>
    /// <returns></returns>
    private async Task<int> CountHaveBeenChosen(long letterUserTelegramId)
    {
        return await Db.UserChoices.CountAsync(uc => uc.ChosenUserTelegramId == letterUserTelegramId);
    }
}