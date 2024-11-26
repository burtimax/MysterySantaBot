using Microsoft.EntityFrameworkCore;
using MultipleBotFramework.Dispatcher.HandlerResolvers;
using MultipleBotFramework.Enums;
using MultipleBotFramework.Extensions;
using MultipleBotFramework.Models;
using MultipleBotFramework.Utils.Keyboard;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.Services;
using MysterySantaBot.State.SetAgeState;
using MysterySantaBot.State.SetDescriptionState;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace MysterySantaBot.State.SetPhotoState;

[BotHandler(stateName: Name, version: 2.0f)]
public class SetPhoto : BaseMysterySantaState
{
    public const string Name = "SetPhotoState";
    
    private SetPhotoRes r => R.SetPhotoState;
    //private FileMediaService _mediaService;
    
    public SetPhoto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       // _mediaService = serviceProvider.GetRequiredService<FileMediaService>();
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Photo, MessageType.Text);
    }
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, long chatId, SetPhotoRes r)
    {
        ReplyKeyboardBuilder keyboardBuilder = new();
        keyboardBuilder.NewRow().Add("Оставить текущее");
        ReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Photo) == false ?keyboardBuilder : default;
        await botClient.SendMessageAsync(chatId, r.InputPhoto, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputPhoto);
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
        
        // Оставить текущее фото
        if (message.Type() == MessageType.Text )
        {
            if (string.Equals(message.Text, r.SetCurrentPhoto))
            {
                await GoNext(existed);
                return;
            }

            await UnexpectedUpdateHandler();
            return;
        }

        // Сохраняем фото в локальной папке.
        // FilePath fp = await _mediaService.LoadPhoto(message.Photo);
        string photoFileId = message.Photo.Last().FileId;
        
        existed.Photo = photoFileId;
        await Db.SaveChangesAsync();

        await GoNext(existed);
    }

    private async Task GoNext(UserForm uf)
    {
        await ChangeState(nameof(SetDescription));
        await SetDescription.Introduce(BotClient, uf, Chat.ChatId, R.SetDescriptionState);
    }
}