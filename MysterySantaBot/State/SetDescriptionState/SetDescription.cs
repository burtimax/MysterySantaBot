using BotFramework;
using BotFramework.Attributes;
using BotFramework.Enums;
using BotFramework.Extensions;
using BotFramework.Models;
using BotFramework.Other;
using Microsoft.EntityFrameworkCore;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetAgeState;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.SetDescriptionState;

[BotState(nameof(SetDescription))]
public class SetDescription : BaseMysterySantaState
{
    private const int MaxDescriptionLength = 800;
    
    private SetDescriptionRes r => R.SetDescriptionState;
    
    public SetDescription(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Text);
    }
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, SetDescriptionRes r)
    {
        IReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Description) == false ? r.SetCurrentDescriptionKeyboard.ToReplyMarkup<ReplyKeyboardMarkup>() : default;
        await botClient.SendTextMessageAsync(chatId, r.InputDescription, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputDescription);
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);

        if (string.Equals(message.Text, r.SetCurrentDescription))
        {
            // Добавить письмо в очередь на модерацию.
            await AddLetterToModerationQueue(User.TelegramId);
            await GoNext(existed);
            return;
        }
        
        if (message.Text!.Length > MaxDescriptionLength)
        {
            await Answer(string.Format(r.DescriptionExceedTheLimit, MaxDescriptionLength));
            return;
        }
        
        existed.Description = message.Text;
        await Db.SaveChangesAsync();

        // Добавить письмо в очередь на модерацию.
        await AddLetterToModerationQueue(User.TelegramId);
        
        await GoNext(existed);
    }

    private async Task AddLetterToModerationQueue(long letterUserTelegramId)
    {
        // Если уже есть в очереди, не добавлять.
        ModeratorLetterQueue? existed =
            await Db.ModeratorLetterQueue.FirstOrDefaultAsync(q => q.LetterUserTelegramId == letterUserTelegramId);

        if (existed != null)
        {
            return;
        }
        
        ModeratorLetterQueue newItem = new()
        {
            LetterUserTelegramId = letterUserTelegramId,
            CreatedAt = DateTimeOffset.Now,
        };
        Db.ModeratorLetterQueue.Add(newItem);
        await Db.SaveChangesAsync();
        await BotHelper.ExecuteFor(BotDbContext, BotConstants.BaseBotClaims.BotUserGet, async tuple =>
        {
            await BotClient.SendTextMessageAsync(tuple.chat.ChatId, R.Notification.NewLetterForModeration);
        });
    }

    private async Task GoNext(UserForm uf)
    {
        await ChangeState(nameof(Main));
        await Main.SendMyLetter(this.ServiceProvider, uf, Chat.ChatId, R.MainState);
        await Main.Introduce(BotClient, uf, Chat.ChatId, R.MainState, UserClaims);
    }
}