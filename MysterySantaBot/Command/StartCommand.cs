using BotFramework.Attributes;
using BotFramework.Base;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MysterySantaBot.Command;

[BotCommand("/start")]
public class StartCommand : BaseMysterySantaCommand
{
    public StartCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task HandleBotRequest(Update update)
    {
        await Answer(R.Default.BotIntroduction);
    }
}