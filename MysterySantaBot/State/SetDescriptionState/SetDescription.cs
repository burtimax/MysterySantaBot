using Microsoft.EntityFrameworkCore;
using MultipleBotFramework;
using MultipleBotFramework.Attributes;
using MultipleBotFramework.Enums;
using MultipleBotFramework.Utils;
using MultipleBotFramework.Utils.Keyboard;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

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
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, long chatId, SetDescriptionRes r)
    {
        ReplyKeyboardBuilder keyboardBuilder = new();
        keyboardBuilder.NewRow().Add("Оставить текущее");
        ReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Description) == false ? keyboardBuilder : default;
        await botClient.SendMessageAsync(chatId, r.InputDescription, replyMarkup: keyboard);
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
        await BotHelper.ExecuteFor(BotDbContext, AppConstants.BotId, BotConstants.BaseBotClaims.BotUserGet, async tuple =>
        {
            await BotClient.SendMessageAsync(tuple.chat.ChatId, R.Notification.NewLetterForModeration);
        }).ConfigureAwait(false);
    }

    private async Task GoNext(UserForm uf)
    {
        await ChangeState(nameof(Main));
        await Main.SendMyLetter(this.ServiceProvider, uf, Chat.ChatId, R.MainState);
        await Main.Introduce(BotClient, uf, Chat.ChatId, R.MainState, UserClaims);
    }
}