using MultipleBotFramework.Constants;
using MultipleBotFramework.Dispatcher.HandlerResolvers;
using MultipleBotFramework.Utils.Keyboard;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace MysterySantaBot.Command;

[BotHandler(command: "/start", version: 2.0f)]
public class StartCommand : BaseMysterySantaCommand
{
    public StartCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task HandleBotRequest(Update update)
    {
        ReplyKeyboardBuilder markupBuilder = new();
        markupBuilder.NewRow().Add(R.StartState.Go);
        var markup = (ReplyMarkup) markupBuilder.Build();
        // markup.OneTimeKeyboard = true;
        await Answer(R.Default.BotIntroduction, parseMode:ParseMode.Markdown, markup);
    }
}