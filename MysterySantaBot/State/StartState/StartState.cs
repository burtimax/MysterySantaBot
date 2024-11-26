using Microsoft.Extensions.Options;
using MultipleBotFramework.Attributes;
using MultipleBotFramework.Constants;
using MysterySantaBot.Resources;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetNameState;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;

namespace MysterySantaBot.State.StartState;

[BotState(nameof(StartState))]
public class StartState : BaseMysterySantaState
{
    private StartRes r => R.StartState;
    
    public StartState(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        
    }

    public override async Task HandleBotRequest(Update update)
    {
        await BotClient.SendMessageAsync(Chat.ChatId, r.Introduction, parseMode: ParseMode.Markdown);
        await ChangeState(nameof(SetName));
    }
}