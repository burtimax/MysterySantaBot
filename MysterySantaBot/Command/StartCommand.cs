using BotFramework.Attributes;
using BotFramework.Base;
using BotFramework.Other;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.Command;

[BotCommand("/start")]
public class StartCommand : BaseMysterySantaCommand
{
    public StartCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task HandleBotRequest(Update update)
    {
        MarkupBuilder<ReplyKeyboardMarkup> markupBuilder = new();
        markupBuilder.NewRow().Add(R.StartState.Go);
        var markup = (ReplyKeyboardMarkup) markupBuilder.Build();
        markup.OneTimeKeyboard = true;
        await Answer(R.Default.BotIntroduction, parseMode:ParseMode.Markdown, markup);
    }
}