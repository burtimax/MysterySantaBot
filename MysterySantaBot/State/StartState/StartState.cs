using BotFramework;
using BotFramework.Attributes;
using BotFramework.Base;
using BotFramework.Enums;
using Microsoft.Extensions.Options;
using MysterySantaBot.Resources;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetNameState;
using Telegram.Bot;
using Telegram.Bot.Types;

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
        await BotClient.SendTextMessageAsync(Chat.ChatId, r.Introduction);
        await ChangeState(nameof(SetName));
    }
}