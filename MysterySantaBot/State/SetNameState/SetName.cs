using BotFramework;
using BotFramework.Attributes;
using BotFramework.Base;
using BotFramework.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources;
using MysterySantaBot.Resources.Res;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MysterySantaBot.State.StartState;

[BotState(nameof(SetName))]
public class SetName : BaseMysterySantaState
{
    private SetNameRes r => R.SetNameState;
    
    public SetName(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Text);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputName);
    }

    public override async Task HandleMessage(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            
            return;
        }
        
        UserForm? existed = await Db.UsersForm.SingleOrDefaultAsync(uf => uf.UserTelegramId == User.TelegramId);
        if (existed == null)
        {
            existed = new UserForm()
            {
                UserTelegramId = User.TelegramId,
                Name = message.Text!
            };
            
            Db.UsersForm.Add(existed);
            await Db.SaveChangesAsync();
        }
        else
        {
            existed.Name = message.Text!;
            await Db.SaveChangesAsync();
        }
        
        await ChangeState(nameof(SetAge));
        await SetAge.Introduce(BotClient, existed, Chat.ChatId, R.SetAgeState);
    }
}