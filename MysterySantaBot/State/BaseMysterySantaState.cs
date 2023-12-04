using System.Net.Http.Headers;
using BotFramework.Base;
using BotFramework.Enums;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database;
using MysterySantaBot.Resources;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.State;

public class BaseMysterySantaState : BaseBotState
{
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
        
        if (update.Type == UpdateType.Message &&
            IsExpectedMessageType(update.Message) == false)
        {
            await UnexpectedUpdateHandler();
            return;
        }

        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessage(update.Message!);
                break;
            case UpdateType.CallbackQuery:
                await HandleCallbackQuery(update.CallbackQuery!);
                break;
        }
    }

    public virtual async Task UnexpectedUpdateHandler()
    {
        await BotClient.SendTextMessageAsync(Chat.ChatId, R.Default.NotExpectedUpdate);
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
    protected void Expected(params UpdateType[] types)
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
        return ExpectedUpdates.Any() == false || ExpectedUpdates.Contains(update.Type);
    }
    
    private bool IsExpectedMessageType(Message message)
    {
        return ExpectedMessageTypes.Any() == false || ExpectedMessageTypes.Contains(message.Type);
    }

    protected Task Answer(string text, ParseMode parseMode = ParseMode.Html, IReplyMarkup replyMarkup = default)
    {
        return BotClient.SendTextMessageAsync(Chat.ChatId, text, parseMode, replyMarkup: replyMarkup);
    }
    
    public async Task ChangeState(string stateName, ChatStateSetterType setterType = ChatStateSetterType.ChangeCurrent)
    {
        Chat.States.Set(stateName, setterType);
        await BotDbContext.SaveChangesAsync();
    }
}