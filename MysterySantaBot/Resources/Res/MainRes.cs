using BotFramework;
using BotFramework.Db.Entity;
using BotFramework.Dto;
using BotFramework.Extensions;
using BotFramework.Other;
using MysterySantaBot.Database.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.Resources.Res;

public class MainRes
{
    public string Santa { get; set; }
    public string Heart { get; set; }
    public string Letter { get; set; }
    public string Pine { get; set; }
    public string Left { get; set; }
    public string Right { get; set; }
    public string Like { get; set; }
    public string Dislike { get; set; }
    public string CheckLetterModeratorButton { get; set; }
    public string AllowLetterByModerator { get; set; }
    public string ForbidLetterByModerator { get; set; }
    public string ButtonLinkToPrivateMessage { get; set; }
    public string EditLetter { get; set; }
    public string Introduction { get; set; }
    
    /// <summary>
    /// 0 - Имя,
    /// 1 - Возраст,
    /// 2 - Описание.
    /// </summary>
    public string LetterFormat { get; set; }
    public string Unexpected { get; set; }
    public string ButtonAddToChoiceList { get; set; }
    public string ButtonRemoveFromChoiceList { get; set; }
    
    /// <summary>
    /// 0 - Максимальное кол-во в списке избранных.
    /// </summary>
    public string ExceedLimitInChoiceList { get; set; }
    public string SuccessAddToChoiceList { get; set; }
    public string SuccessRemoveFromChoiceList { get; set; }
    public string SuccessLetterAllowed { get; set; }
    public string SuccessLetterForbid { get; set; }
    public string NoLettersForModeration { get; set; }
    public string ChoiceListIsEmpty { get; set; }
    public string NotFoundLetters { get; set; }
    public string EditLetterInlineButton { get; set; }
    public string HideMessage { get; set; }
    public string YourLetterWasForbiddenByModeratorNotification { get; set; }
    


    #region Methods

    /// <summary>
    /// Получить клавиатуру.
    /// </summary>
    /// <param name="userClaims"></param>
    /// <returns></returns>
    public MarkupBuilder<ReplyKeyboardMarkup> GetMainKeyboard(IEnumerable<ClaimValue> userClaims)
    {
        MarkupBuilder<ReplyKeyboardMarkup> markupBuilder = new();
        markupBuilder.NewRow()
            .Add(Santa)
            .Add(Heart)
            .Add(Letter);

        // Модераторам добавляем спец кнопку.
        if (userClaims.Any(uc => uc.Name == BotConstants.BaseBotClaims.BotUserGet))
        {
            markupBuilder.Add(CheckLetterModeratorButton);
        }

        return markupBuilder;
    }

    /// <summary>
    /// Клавиатура под моим письмом.
    /// </summary>
    /// <param name="me"></param>
    /// <returns></returns>
    public MarkupBuilder<InlineKeyboardMarkup> GetMyLetterInline(UserForm me)
    {
        MarkupBuilder<InlineKeyboardMarkup> inline = new();
        inline.NewRow()
            .Add(EditLetterInlineButton, InlineEditMyLetter)
            .NewRow()
            .Add(HideMessage, InlineDeleteMessage);
        return inline;
    }

    /// <summary>
    /// Получить клавиатуру под письмом для пользователя.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="letterUserForm">Письмо.</param>
    /// <returns></returns>
    public MarkupBuilder<InlineKeyboardMarkup> GetInlineUnderLetterForUser(BotUser user,
        IEnumerable<ClaimValue> userClaims, long letterUserTelegramId, bool chatHasExisted, bool isAddingButton = true,
        bool hideChoiceButton = false)
    {
        MarkupBuilder<InlineKeyboardMarkup> inline = new();
        
        if (userClaims.Any(uc => uc.Name == BotConstants.BaseBotClaims.BotUserGet))
        {
            inline.NewRow()
                .Add(AllowLetterByModerator, InlineAllowLetter + letterUserTelegramId)
                .Add(ForbidLetterByModerator, InlineForbidLetter + letterUserTelegramId);

            if (chatHasExisted)
            {
                inline.NewRow()
                    .Add(ButtonLinkToPrivateMessage,
                        url: string.Format(AppConstants.UserPrivateMessageLinkFormat, letterUserTelegramId.ToString()));
            }
        }
        
        if (hideChoiceButton == false)
        {
            if (isAddingButton)
            {
                inline.NewRow().Add(ButtonAddToChoiceList, MainRes.InlineKeyAddToChoiceList + letterUserTelegramId);
            }
            else
            {
                inline.NewRow().Add(ButtonRemoveFromChoiceList, MainRes.InlineRemoveFromChoiceList + letterUserTelegramId);
            }
        }
        
        return inline;
    }

    public MarkupBuilder<InlineKeyboardMarkup> GetInlineUnderChoiceListForUser(BotUser user,
        IEnumerable<ClaimValue> userClaims, long letterUserTelegramId, int choiceListCount, bool chatHasExisted, bool isAddingButton = true)
    {
        var inline = GetInlineUnderLetterForUser(user, userClaims, letterUserTelegramId, chatHasExisted, isAddingButton);

        if (choiceListCount > 0)
        {
            inline.NewRow()
                .Add(Left, MainRes.InlineShowPrevious + letterUserTelegramId)
                .Add(Right, MainRes.InlineShowNext + letterUserTelegramId);
        }

        return inline;
    }

    #endregion
    
    
    
    #region InlineData

    public const string InlineKeyAddToChoiceList = "add_to_choice_list:";
    public const string InlineRemoveFromChoiceList = "remove_from_choice_list:";
    public const string InlineShowPrevious = "show_previous_item:";
    public const string InlineShowNext = "show_next_item:";
    public const string InlineLikeLetter = "like_letter:";
    public const string InlineDislikeLetter = "dislike_letter:";
    public const string InlineEditMyLetter = "edit_letter:";
    public const string InlineDeleteMessage = "delete_message:";
    public const string InlineAllowLetter = "allow_letter:";
    public const string InlineForbidLetter = "forbid_letter:";

    #endregion
    
    #region UserPropertyKeys

    public const string PropKeyChoiceListWasLastButton = "choice_list_now";

    #endregion

}