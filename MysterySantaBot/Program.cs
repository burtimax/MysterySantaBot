using BotFramework.Extensions;
using BotFramework.Options;
using Microsoft.EntityFrameworkCore;
using MysterySantaBot.Database;
using MysterySantaBot.Extensions;
using MysterySantaBot.Options;
using MysterySantaBot.Resources;
using MysterySantaBot.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);  
  
var builder = WebApplication.CreateBuilder(args);  
IServiceCollection services = builder.Services;  
  
// Initialize  
  
// Register options  
services.AddLogging();  
services.Configure<BotConfiguration>(builder.Configuration.GetSection("Bot"));  
services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));  
services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Key));  
var botConfig = builder.Configuration.GetSection("Bot").Get<BotConfiguration>();  
BotResources botResources = services.ConfigureBotResources(botConfig.ResourcesFilePath);  
services.AddBot(botConfig);

services.AddDbContext<SantaBotDbContext>(options =>
{
    options.UseNpgsql(botConfig.DbConnection);
});

using (SantaBotDbContext botDbContext = new SantaBotDbContext(botConfig.DbConnection))
{
    botDbContext.Database.Migrate();
}

// Add custom services
services.AddTransient<SearchLetterService>();
services.AddTransient<FileMediaService>();

// Add services to the container.  
services.AddControllers().AddNewtonsoftJson();  
services.AddHttpContextAccessor();  
  
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
services.AddEndpointsApiExplorer();  
services.AddSwaggerGen();  
  
var app = builder.Build();  
  
// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
//app.UseHttpsRedirection();  
app.UseRouting();  
app.UseAuthorization();  
app.MapControllers();  
app.Run();