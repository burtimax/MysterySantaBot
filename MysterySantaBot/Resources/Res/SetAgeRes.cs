using BotFramework.Other;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.Resources.Res;

public class SetAgeRes
{
    public string InputAge { get; set; }

    public IReplyMarkup PreviousAge(int age) => new MarkupBuilder<ReplyKeyboardMarkup>().NewRow().Add(age.ToString()).Build();
}