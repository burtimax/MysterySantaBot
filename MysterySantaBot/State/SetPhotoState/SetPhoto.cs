using BotFramework.Attributes;
using BotFramework.Enums;
using BotFramework.Extensions;
using BotFramework.Models;
using BotFramework.Other;
using Microsoft.EntityFrameworkCore;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.Services;
using MysterySantaBot.State.SetAgeState;
using MysterySantaBot.State.SetDescriptionState;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.SetPhotoState;

[BotState(nameof(SetPhoto))]
public class SetPhoto : BaseMysterySantaState
{
    private SetPhotoRes r => R.SetPhotoState;
    private FileMediaService _mediaService;
    
    public SetPhoto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _mediaService = serviceProvider.GetRequiredService<FileMediaService>();
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Photo, MessageType.Text);
    }
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, SetPhotoRes r)
    {
        IReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Photo) == false ? r.SetCurrentPhotoKeyboard.ToReplyMarkup<ReplyKeyboardMarkup>() : default;
        await botClient.SendTextMessageAsync(chatId, r.InputPhoto, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputPhoto);
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
        
        // Оставить текущее фото
        if (message.Type == MessageType.Text )
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
        FilePath fp = await _mediaService.LoadPhoto(message.Photo);
        
        existed.Photo = fp.FileName;
        await Db.SaveChangesAsync();

        await GoNext(existed);
    }

    private async Task GoNext(UserForm uf)
    {
        await ChangeState(nameof(SetDescription));
        await SetDescription.Introduce(BotClient, uf, Chat.ChatId, R.SetDescriptionState);
    }
}