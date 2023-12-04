using BotFramework;
using BotFramework.Attributes;
using BotFramework.Base;
using BotFramework.Enums;
using BotFramework.Extensions;
using BotFramework.Models;
using BotFramework.Other;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources;
using MysterySantaBot.Resources.Res;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.StartState;

[BotState(nameof(SetPhoto))]
public class SetPhoto : BaseMysterySantaState
{
    private SetPhotoRes r => R.SetPhotoState;
    
    public SetPhoto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Photo);
    }
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, SetPhotoRes r)
    {
        IReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Photo) == false ? r.SetCurrentPhoto.ToReplyMarkup<ReplyKeyboardMarkup>() : default;
        await botClient.SendTextMessageAsync(chatId, r.InputPhoto, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputPhoto);
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);

        // Сохраняем фото в локальной папке.
        string fileName = message.Photo.GetFileByQuality(PhotoQuality.High).FileUniqueId + ".jpeg";
        FilePath fp = new FilePath(Path.Combine(MediaDirectory, fileName));
        await BotMediaHelper.DownloadAndSaveTelegramFileAsync(BotClient, 
            message.Photo.GetFileByQuality(PhotoQuality.High).FileId, fp);
        
        existed.Photo = fileName;
        await Db.SaveChangesAsync();

        await ChangeState(nameof(SetAge));
        await SetAge.Introduce(BotClient, existed, Chat.ChatId, R.SetAgeState);
    }
}