using System.Text;
using BotFramework.Dto;
using BotFramework.Extensions;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.SetDescriptionState;

public partial class Main
{
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, MainRes r, IEnumerable<ClaimValue> userClaims)
    {
        var keyboard = r.GetMainKeyboard(userClaims);
        
        await botClient.SendTextMessageAsync(chatId, r.Introduction, replyMarkup: keyboard.Build());
    }

    public static async Task SendMyLetter(IServiceProvider serviceProvider, UserForm me, ChatId chatId, MainRes r)
    {
        var mediaService = serviceProvider.GetRequiredService<FileMediaService>();
        ITelegramBotClient botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
        
        InputOnlineFile iof = await mediaService.GetUserFormPhoto(me) 
                              ?? throw new Exception($"Нет фото [{me.Photo}] для пользователя [{me.UserTelegramId}]"); // Получаем файл из диска.
        
        StringBuilder sb = new();
        sb.AppendLine(string.Format(r.LetterFormat, me.Name, me.Age.ToString(), me.Description));
        await botClient.SendPhotoAsync(chatId, iof, sb.ToString());
    }
}