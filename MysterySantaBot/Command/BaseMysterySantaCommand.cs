using Microsoft.Extensions.Options;
using MultipleBotFramework.Base;
using MultipleBotFramework.Constants;
using MultipleBotFramework.Enums;
using MysterySantaBot.Database;
using MysterySantaBot.Resources;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace MysterySantaBot.Command;

public class BaseMysterySantaCommand : BaseBotHandler
{
    protected IServiceProvider ServiceProvider;
    protected readonly BotResources R;
    protected readonly SantaBotDbContext Db;

    public BaseMysterySantaCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
        R = serviceProvider.GetRequiredService<IOptions<BotResources>>().Value;
        Db = serviceProvider.GetRequiredService<SantaBotDbContext>();
    }
    

    protected async Task<Message> Answer(string text, string parseMode = ParseMode.Html, ReplyMarkup replyMarkup = default)
    {
        return await BotClient.SendMessageAsync(Chat.ChatId, text, parseMode:parseMode, replyMarkup: replyMarkup);
    }
    
    public async Task ChangeState(string stateName, ChatStateSetterType setterType = ChatStateSetterType.ChangeCurrent)
    {
        Chat.States.Set(stateName, setterType);
        await BotDbContext.SaveChangesAsync();
    }

    public override Task HandleBotRequest(Update update)
    {
        throw new NotImplementedException();
    }
}