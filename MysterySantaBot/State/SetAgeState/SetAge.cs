using Microsoft.EntityFrameworkCore;
using MultipleBotFramework.Attributes;
using MultipleBotFramework.Enums;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetPhotoState;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace MysterySantaBot.State.SetAgeState;

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

    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, long chatId, SetAgeRes r)
    {
        ReplyMarkup keyboard = userForm.Age != null ? r.PreviousAge((int) userForm.Age) : default;
        await botClient.SendMessageAsync(chatId, r.InputAge, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return base.UnexpectedUpdateHandler();
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