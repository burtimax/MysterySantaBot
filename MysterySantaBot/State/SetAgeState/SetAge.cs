using BotFramework;
using BotFramework.Attributes;
using BotFramework.Base;
using BotFramework.Db.Entity;
using BotFramework.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources;
using MysterySantaBot.Resources.Res;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.StartState;

[BotState(nameof(SetAge))]
public class SetAge : BaseMysterySantaState
{
    private const int MinAge = 0;
    private const int MaxAge = 100;
    
    private SetAgeRes r => R.SetAgeState;
    
    public SetAge(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Text);
    }

    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, SetAgeRes r)
    {
        IReplyMarkup keyboard = userForm.Age != null ? r.PreviousAge((int) userForm.Age) : default;
        await botClient.SendTextMessageAsync(chatId, r.InputAge, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputAge);
    }

    public override async Task HandleMessage(Message message)
    {
        if (int.TryParse(message.Text, out int age) == false ||
            age < MinAge || age > MaxAge)
        {
            await Answer(r.InputAge);
            return;
        }
        
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
       
        existed.Age = age;
        await Db.SaveChangesAsync();
        
        await ChangeState(nameof(SetPhoto));
        await SetPhoto.Introduce(BotClient, existed, Chat.ChatId, R.SetPhotoState);
    }
}