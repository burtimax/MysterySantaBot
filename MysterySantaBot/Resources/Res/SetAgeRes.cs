using MultipleBotFramework.Utils.Keyboard;
using Telegram.BotAPI.AvailableTypes;

namespace MysterySantaBot.Resources.Res;

public class SetAgeRes
{
    public string InputAge { get; set; }

    public ReplyMarkup PreviousAge(int age) => new ReplyKeyboardBuilder().NewRow().Add(age.ToString()).Build();
}