using BotFramework.Base;
using BotFramework.Enums;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database;
using MysterySantaBot.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MysterySantaBot.Command;

public class BaseMysterySantaCommand : BaseBotCommand
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
    

    protected Task Answer(string text, ParseMode parseMode = ParseMode.Html, IReplyMarkup replyMarkup = default)
    {
        return BotClient.SendTextMessageAsync(Chat.ChatId, text, parseMode, replyMarkup: replyMarkup);
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