using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using MultipleBotFramework.Extensions;
using MultipleBotFramework.Options;
using MysterySantaBot;
using MysterySantaBot.Database;
using MysterySantaBot.Extensions;
using MysterySantaBot.Options;
using MysterySantaBot.Resources;
using MysterySantaBot.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwagger("Telegram Bot API");
builder.Services.AddFastEndpoints(o =>
    {
        o.Assemblies = new[]
        {
            typeof(GetBotsEndpoint).Assembly,
            typeof(Program).Assembly,
        };
    })
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Metal board bot API";
            s.Version = "v1";
        };
    });

var services = builder.Services;
//services.AddServices();
services.AddTransient<SearchLetterService>();
//services.AddTransient<FileMediaService>();

// Добавляем контексты
services.Configure<AppOptions>(builder.Configuration);
var config = builder.Configuration.Get<AppOptions>();

services.Configure<BotConfiguration>(builder.Configuration.GetSection("Bot"));
services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));
var botConfig = builder.Configuration.GetSection("Bot").Get<BotConfiguration>();

//
services.AddDbContext<SantaBotDbContext>(options =>
{
    options.UseNpgsql(botConfig.DbConnection);
});

// Регистрируем конфигурации.

BotOptions botOptions = builder.Configuration.GetSection("BotOptions").Get<BotOptions>();
BotResources botResources = services.ConfigureBotResources(botConfig.ResourcesFilePath);
services.AddBot(botConfig, botOptions: botOptions); // Подключаем бота
services.AddControllers();//.AddNewtonsoftJson(); //Обязательно подключаем NewtonsoftJson
services.AddHttpContextAccessor();
services.AddCors();
// services.AddMapster();

// Свои сервисы


// Регистрируем контексты к базам данных.

var app = builder.Build();
// app.UseSwagger();
// app.UseSwaggerUI(options =>
// {
//     options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
//     // options.RoutePrefix = string.Empty;
// });

app.UseFastEndpoints().UseSwaggerGen();

// using (var scope = app.Services.CreateScope())
// {
//     // Провести миграцию в БД.
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     await db.Database.MigrateAsync();
//         
//     var bootstrapDatabaseService = scope.ServiceProvider.GetRequiredService<BootstrapDatabaseService>();
//     await bootstrapDatabaseService.BootstrapDatabase();
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
//app.UseHttpsRedirection();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();