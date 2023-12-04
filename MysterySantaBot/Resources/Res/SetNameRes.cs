using BotFramework.Other;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.Resources.Res;

public class SetNameRes
{
    public string SetNameSuccess { get; set; }
    public string InputName { get; set; }
    public string TooLongName { get; set; }
    
    public IReplyMarkup PreviousName(string name) => new MarkupBuilder<ReplyKeyboardMarkup>().NewRow().Add(name.ToString()).Build();
}