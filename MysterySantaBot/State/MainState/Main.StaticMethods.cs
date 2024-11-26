using System.Text;
using MultipleBotFramework.Dto;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.Services;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;

namespace MysterySantaBot.State.SetDescriptionState;

public partial class Main
{
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, long chatId, MainRes r, IEnumerable<ClaimValue> userClaims)
    {
        var keyboard = r.GetMainKeyboard(userClaims);
        
        await botClient.SendMessageAsync(chatId, r.Introduction, replyMarkup: keyboard.Build());
    }

    public static async Task SendMyLetter(IServiceProvider serviceProvider, UserForm me, long chatId, MainRes r)
    {
        //var mediaService = serviceProvider.GetRequiredService<FileMediaService>();
        ITelegramBotClient botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
        
        string photoFileId = me.Photo 
                              ?? throw new Exception($"Нет фото [{me.Photo}] для пользователя [{me.UserTelegramId}]"); // Получаем файл из диска.
        
        StringBuilder sb = new();
        sb.AppendLine(string.Format(r.LetterFormat, me.Name, me.Age.ToString(), me.Description));
        await botClient.SendPhotoAsync(chatId, photoFileId, caption: sb.ToString());
    }
}