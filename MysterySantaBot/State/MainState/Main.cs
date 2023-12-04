using BotFramework;
using BotFramework.Attributes;
using BotFramework.Enums;
using BotFramework.Extensions;
using BotFramework.Models;
using BotFramework.Other;
using Microsoft.EntityFrameworkCore;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.State.SetAgeState;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.SetDescriptionState;

[BotState(nameof(Main))]
public class Main : BaseMysterySantaState
{
    private SetDescriptionRes r => R.SetDescriptionState;
    
    public Main(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       Expected(UpdateType.Message);
       ExpectedMessage(MessageType.Text);
    }
    
    public static async Task Introduce(ITelegramBotClient botClient, UserForm userForm, ChatId chatId, SetDescriptionRes r)
    {
        IReplyMarkup? keyboard = string.IsNullOrEmpty(userForm.Photo) == false ? r.SetCurrentDescriptionKeyboard.ToReplyMarkup<ReplyKeyboardMarkup>() : default;
        await botClient.SendTextMessageAsync(chatId, r.InputDescription, replyMarkup: keyboard);
    }

    public override Task UnexpectedUpdateHandler()
    {
        return Answer(r.InputDescription);
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);

        if (string.Equals(message.Text, r.SetCurrentDescription))
        {
            await GoNext(existed);
            return;
        }
        
        // Сохраняем фото в локальной папке.
        if (message.Text!.Length > BotConstants.Constraints.MaxDocumentCaption)
        {
            await Answer(string.Format(r.DescriptionExceedTheLimit, BotConstants.Constraints.MaxDocumentCaption));
            return;
        }
        
        existed.Description = message.Text;
        await Db.SaveChangesAsync();

        await GoNext(existed);
    }

    private async Task GoNext(UserForm uf)
    {
        await ChangeState(nameof(SetAge));
        await SetAge.Introduce(BotClient, uf, Chat.ChatId, R.SetAgeState);
    }
}