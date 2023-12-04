using System.Reflection;
using BotFramework.Dispatcher;
using BotFramework.Repository;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MysterySantaBot.Controller;

[ApiController]  
public class MainBotDispatcherController : BotDispatcherController  
{  
    private readonly ITelegramBotClient _botClient;
    private IBaseBotRepository _baseBotRepository;  
	  
    public MainBotDispatcherController(ITelegramBotClient botClient, IBaseBotRepository baseBotRepository,  
        IHttpContextAccessor contextAccessor)  
        : base(baseBotRepository, contextAccessor, Assembly.GetExecutingAssembly())  
    {  
        _botClient = botClient;  
    }  
	  
    [HttpPost("/")]  
    public override async Task<IActionResult> HandleBotRequest([FromBody] Update updateRequest)  
    {  
        return await base.HandleBotRequest(updateRequest);  
    }  
}
