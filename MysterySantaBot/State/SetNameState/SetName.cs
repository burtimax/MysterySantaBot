using Microsoft.EntityFrameworkCore;
using MultipleBotFramework.Attributes;
using MultipleBotFramework.Dispatcher.HandlerResolvers;
using MultipleBotFramework.Enums;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetAgeState;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace MysterySantaBot.State.SetNameState;

[BotHandler(stateName:Name)]
public class SetName : BaseMysterySantaState
{
    public const string Name = "SetNameState";
    private SetNameRes r => R.SetNameState;
    
    public SetName(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Text);
    }

    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, long chatId, SetNameRes r)
    {
        ReplyMarkup keyboard = userForm.Age != null ? r.PreviousName(userForm.Name) : default;
        await botClient.SendMessageAsync(chatId, r.InputName, replyMarkup: keyboard);
    }
    
    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputName);
    }

    public override async Task HandleMessage(Message message)
    {
        string name = message.Text;

        if (name.Length > 50)
        {
            await Answer(r.TooLongName);
            return;
        }

        UserForm? existed = await Db.UsersForm.SingleOrDefaultAsync(uf => uf.UserTelegramId == User.TelegramId);
        if (existed == null)
        {
            existed = new UserForm()
            {
                UserTelegramId = User.TelegramId,
                Name = name!
            };
            
            Db.UsersForm.Add(existed);
            await Db.SaveChangesAsync();
        }
        else
        {
            existed.Name = name!;
            await Db.SaveChangesAsync();
        }
        
        await ChangeState(nameof(SetAge));
        await SetAge.Introduce(BotClient, existed, Chat.ChatId, R.SetAgeState);
    }
}