using MultipleBotFramework.Utils.Keyboard;

namespace MysterySantaBot.Resources.Res;

public class SetNameRes
{
    public string SetNameSuccess { get; set; }
    public string InputName { get; set; }
    public string TooLongName { get; set; }
    
    public ReplyKeyboardBuilder PreviousName(string name) => new ReplyKeyboardBuilder().NewRow().Add(name.ToString()).Build();
}