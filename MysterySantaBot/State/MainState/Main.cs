using System.Text;
using BotFramework;
using BotFramework.Attributes;
using BotFramework.Enums;
using BotFramework.Extensions;
using BotFramework.Models;
using BotFramework.Other;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database.Entities;
using MysterySantaBot.Options;
using MysterySantaBot.Resources.Res;
using MysterySantaBot.Services;
using MysterySantaBot.State.SetAgeState;
using MysterySantaBot.State.SetNameState;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State.SetDescriptionState;

[BotState(nameof(Main))]
public partial class Main : BaseMysterySantaState
{
    private readonly SearchLetterService _search;
    private readonly FileMediaService _mediaService;
    private readonly AppOptions _appOptions;
    private MainRes r => R.MainState;
    
    public Main(IServiceProvider serviceProvider) : base(serviceProvider)
    {
       _search = serviceProvider.GetRequiredService<SearchLetterService>();
       _mediaService = serviceProvider.GetRequiredService<FileMediaService>();
       _appOptions = serviceProvider.GetRequiredService<IOptions<AppOptions>>().Value;
       Expected(UpdateType.Message, UpdateType.CallbackQuery);
       ExpectedMessage(MessageType.Text);
    }

    public override Task UnexpectedUpdateHandler()
    {
        var keyboard = r.GetMainKeyboard(UserClaims);
        return Answer(r.Unexpected, replyMarkup:keyboard.Build());
    }

    public override async Task HandleMessage(Message message)
    {
        UserForm existed = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
        string text = message.Text;

        if (string.Equals(text, r.Heart)) 
            User.AdditionalProperties.Set(MainRes.PropKeyChoiceListWasLastButton, 1);
        else 
            User.AdditionalProperties.Remove(MainRes.PropKeyChoiceListWasLastButton);
        await BotDbContext.SaveChangesAsync();
        
        // Нажата кнопка поиска.
        if (string.Equals(text, r.Santa))
        {
            await SearchAndShowNextLetter(existed);
            return;
        }
        
        // Нажата кнопка редактирования письма.
        if (string.Equals(text, r.Letter))
        {
            await ShowMyLetter();
            return;
        }
        
        // Нажата кнопка избранных писем.
        if (string.Equals(text, r.Heart))
        {
            if (await GetChoiceListCount() < 1)
            {
                await Answer(r.ChoiceListIsEmpty);
                return;
            }
            
            List<UserChoice> choiceList = await GetChoiceList();
            UserForm firstFromChoiceList =
                await Db.UsersForm.FirstAsync(uf => uf.UserTelegramId == choiceList.First().ChosenUserTelegramId);
            await ShowLetterFromChoiceList(firstFromChoiceList);
            return;
        }

        if (string.Equals(text, r.CheckLetterModeratorButton) &&
            UserClaims.Any(uc => uc.Name == BotConstants.BaseBotClaims.BotUserGet))
        {
            // Получить следующее письмо для модерации.
            long? letterUserTelegramId = await GetNextUserTelegramIdForModeration();

            if (letterUserTelegramId is null || letterUserTelegramId == 0)
            {
                await Answer(r.NoLettersForModeration);
                return;
            }
            
            UserForm userForm =
                await Db.UsersForm.FirstAsync(uf => uf.UserTelegramId == letterUserTelegramId);
            await ShowLetter(userForm);
            return;
        }

        await UnexpectedUpdateHandler();
    }

    private async Task ShowLetterFromChoiceList(UserForm userForm, bool isAddingButton = false)
    {
        int choiceListCount = await GetChoiceListCount();
        if (choiceListCount == 0)
        {
            await Answer(r.ChoiceListIsEmpty);
            return;
        }
        
        var data = await GetDataForLetter(userForm, false);

        var inline = r.GetInlineUnderChoiceListForUser(User, UserClaims, userForm.UserTelegramId, 
            choiceListCount, 
            await HasChatExisted(BotClient, userForm.UserTelegramId),
            isAddingButton);
        
        await BotClient.SendPhotoAsync(Chat.ChatId, data.photo, data.caption,
            ParseMode.Html, 
            replyMarkup: inline.Build());
    }

    private async Task SearchAndShowNextLetter(UserForm existed)
    {
        UserForm? candidate = await _search.GetNextUserTelegramIdInSearch(existed);
            
        if (candidate == null)
        {
            await Answer(r.NotFoundLetters);
            return;
        }

        await ShowLetter(candidate);
    }
    
    private async Task ShowLetter(UserForm userForm, bool removeInline = false)
    {
        var data = await GetDataForLetter(userForm, removeInline);
        
        data.inline
            .NewRow()
            .Add(r.Like, MainRes.InlineLikeLetter + userForm.UserTelegramId)
            .Add(r.Dislike, MainRes.InlineDislikeLetter + userForm.UserTelegramId);

        
        
        await BotClient.SendPhotoAsync(Chat.ChatId, data.photo, data.caption,
            ParseMode.Html, 
            replyMarkup: removeInline == false ? data.inline.Build() : default);
        await UpdateLastShownTime(userForm.UserTelegramId);
    }

    private async Task<(InputOnlineFile photo, string caption, MarkupBuilder<InlineKeyboardMarkup> inline)>
        GetDataForLetter(UserForm userForm, bool removeInline)
    {
        int count = await CountHaveBeenChosen(userForm.UserTelegramId);
        bool hideChoiceButton = count >= AppConstants.MaxChosen; 
        
        InputOnlineFile iof = await _mediaService.GetUserFormPhoto(userForm) 
                              ?? throw new Exception($"Нет фото [{userForm.Photo}] для пользователя [{userForm.UserTelegramId}]"); // Получаем файл из диска.

        var marks = await CountLetterMarks(userForm.UserTelegramId);
        
        StringBuilder sb = new();

        if (HasUserClaim(BotConstants.BaseBotClaims.BotUserGet))
        {
            sb.Append($"<code>{userForm.UserTelegramId}</code> ");
        }
        
        sb.AppendLine(string.Format(r.LetterFormat, userForm.Name, userForm.Age.ToString(), userForm.Description));

        sb.AppendLine($"{r.Like} <b>{marks.Like}</b> {r.Dislike} <b>{marks.Dislike}</b>");
        
        MarkupBuilder<InlineKeyboardMarkup> inline = r.GetInlineUnderLetterForUser(User, UserClaims, userForm.UserTelegramId,
            await HasChatExisted(BotClient, userForm.UserTelegramId),true, hideChoiceButton);

        return (iof, sb.ToString(), inline);
    }

    private bool HasUserClaim(string claimName) => UserClaims.Any(uc => uc.Name == claimName);

    /// <summary>
    /// Обновить время последнего показа письма пользователя.
    /// </summary>
    /// <param name="shownUserTelegramId">ИД автора письма</param>
    private async Task UpdateLastShownTime(long shownUserTelegramId)
    {
        ShowHistory? sh = await Db.ShowHistory.FirstOrDefaultAsync(h =>
            h.UserTelegramId == User.TelegramId && h.ShownUserTelegramId == shownUserTelegramId);

        if (sh == null)
        {
            sh = new()
            {
                UserTelegramId = User.TelegramId,
                ShownUserTelegramId = shownUserTelegramId,
                LastShown = DateTime.Now,
                CreatedAt = DateTimeOffset.Now,
            };
            Db.ShowHistory.Add(sh);
            await Db.SaveChangesAsync();
            return;
        }
        
        sh.LastShown = DateTime.Now;
        sh.UpdatedAt = DateTimeOffset.Now;
        Db.ShowHistory.Update(sh);
        await Db.SaveChangesAsync();
    }

    /// <summary>
    /// Показать мое письмо.
    /// </summary>
    /// <param name="me"></param>
    private async Task ShowMyLetter()
    {
        UserForm me = await Db.UsersForm.SingleAsync(uf => uf.UserTelegramId == User.TelegramId);
        var data = await GetDataForLetter(me, true);

        var inline = r.GetMyLetterInline(me);

        await BotClient.SendPhotoAsync(Chat.ChatId, data.photo, data.caption,
            ParseMode.Html, 
            replyMarkup: inline.Build());
    }
    
    /// <summary>
    /// Перейти к редактированию письма.
    /// </summary>
    /// <param name="me"></param>
    private async Task GoToEditLetter(UserForm me)
    {
        await ChangeState(nameof(SetName));
        await Answer(r.EditLetter);
        await SetName.Introduce(BotClient, me, Chat.ChatId, R.SetNameState);
    }

    public static async Task<bool> HasChatExisted(ITelegramBotClient client, long chatId)
    {
        try
        {
            var chat = await client.GetChatAsync(chatId);
            if (chat != null) return true;
        }
        catch (Exception e)
        {
            return false;
        }

        return false;
    }
}