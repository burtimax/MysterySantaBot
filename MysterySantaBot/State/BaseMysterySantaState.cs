using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MultipleBotFramework.Base;
using MultipleBotFramework.Constants;
using MultipleBotFramework.Enums;
using MultipleBotFramework.Extensions;
using MysterySantaBot.Database;
using MysterySantaBot.Resources;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace MysterySantaBot.State;

public class BaseMysterySantaState : BaseBotHandler
{
    protected string NotExpectedMessage { get; set; } 
    protected IServiceProvider ServiceProvider;
    protected readonly BotResources R;
    protected readonly SantaBotDbContext Db;
    private readonly List<UpdateType> ExpectedUpdates = new ();
    private readonly List<MessageType> ExpectedMessageTypes = new ();
    

    public BaseMysterySantaState(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
        R = serviceProvider.GetRequiredService<IOptions<BotResources>>().Value;
        Db = serviceProvider.GetRequiredService<SantaBotDbContext>();
    }

   public override async Task HandleBotRequest(Update update)
    {
        if (IsExpectedUpdate(update) == false)
        {
            await UnexpectedUpdateHandler();
            return;
        }
        
        if (update.Type() == MultipleBotFramework.Enums.UpdateType.Message &&
            IsExpectedMessageType(update.Message) == false)
        {
            await UnexpectedUpdateHandler();
            return;
        }

        switch (update.Type())
        {
            case MultipleBotFramework.Enums.UpdateType.Message:
                await HandleMessage(update.Message!);
                break;
            case MultipleBotFramework.Enums.UpdateType.CallbackQuery:
                await HandleCallbackQuery(update.CallbackQuery!);
                break;
        }
    }

    public virtual async Task UnexpectedUpdateHandler()
    {
        try
        {
            await Answer(NotExpectedMessage);
        }
        catch (Exception e)
        {
        }
    }
    
    public virtual async Task HandleMessage(Message message)
    {
        throw new NotImplementedException();
    }
    
    public virtual async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        throw new NotImplementedException();
        return;
    }

    /// <summary>
    /// Заполняем ожидаемые типы запросов для состояния.
    /// </summary>
    /// <param name="types"></param>
    protected void Expected(params MultipleBotFramework.Enums.UpdateType[] types)
    {
        foreach (var type in types)
        {
            ExpectedUpdates.Add(type);
        }
    }
    
    /// <summary>
    /// Заполняем ожидаемые типы сообщений для состояния.
    /// </summary>
    /// <param name="types"></param>
    protected void ExpectedMessage(params MessageType[] types)
    {
        foreach (var type in types)
        {
            ExpectedMessageTypes.Add(type);
        }
    }

    private bool IsExpectedUpdate(Update update)
    {
        return ExpectedUpdates.Any() == false || ExpectedUpdates.Contains(update.Type());
    }
    
    private bool IsExpectedMessageType(Message message)
    {
        return ExpectedMessageTypes.Any() == false || ExpectedMessageTypes.Contains(message.Type());
    }

    protected Task<Message> Answer(string text, string parseMode = ParseMode.Html, ReplyMarkup replyMarkup = default)
    {
        return BotClient.SendMessageAsync(Chat.ChatId, text:text, parseMode:parseMode, replyMarkup: replyMarkup);
    }
    
    protected async Task AnswerCallback()
    {
        if (this.Update.Type() == MultipleBotFramework.Enums.UpdateType.CallbackQuery)
        {
            await BotClient.AnswerCallbackQueryAsync(this.Update.CallbackQuery.Id);
        }
    }
    
    public async Task ChangeState(string stateName, ChatStateSetterType setterType = ChatStateSetterType.ChangeCurrent)
    {
        Chat.States.Set(stateName, setterType);
        await BotDbContext.SaveChangesAsync();
    }
    
}