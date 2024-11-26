// using BotFramework.Enums;
// using BotFramework.Extensions;
// using BotFramework.Models;
// using BotFramework.Options;
// using BotFramework.Other;
// using Microsoft.Extensions.Options;
// using MultipleBotFramework.Enums;
// using MultipleBotFramework.Models;
// using MultipleBotFramework.Options;
// using MultipleBotFramework.Utils;
// using MysterySantaBot.Database.Entities;
// using Telegram.Bot;
// using Telegram.Bot.Types;
// using Telegram.Bot.Types.InputFiles;
// using Telegram.BotAPI;
// using Telegram.BotAPI.AvailableTypes;
// using File = System.IO.File;
//
// namespace MysterySantaBot.Services;
//
// public class FileMediaService
// {
//     private readonly ITelegramBotClient _botClient;
//     private readonly string _mediaDir;
//     
//     public FileMediaService(IOptions<BotConfiguration> botConfig, ITelegramBotClient botClient)
//     {
//         _botClient = botClient;
//         _mediaDir = botConfig.Value.MediaDirectory;
//     }
//
//     // public async Task<InputOnlineFile?> GetUserFormPhoto(UserForm userForm)
//     // {
//     //     if (string.IsNullOrEmpty(userForm.Photo)) return null;
//     //
//     //     FilePath path = new FilePath(Path.Combine(_mediaDir, userForm.Photo));
//     //
//     //     if (File.Exists(path) == false)
//     //     {
//     //         string fileId = userForm.Photo;
//     //         MemoryStream ms =
//     //             (await BotMediaHelper.GetFileFromTelegramAsync(_botClient, fileId))
//     //             .fileData;
//     //         return new InputOnlineFile(ms, userForm.Photo);
//     //     }
//     //     
//     //     return new InputOnlineFile(await BotMediaHelper.GetFileByPathAsync(path), userForm.Photo);
//     // }
//
//     public async Task<FilePath> LoadPhoto(PhotoSize[] photo)
//     {
//         string fileName = GetFileNameFromFileId(photo.GetFileByQuality(PhotoQuality.High).FileId);
//         FilePath fp = new FilePath(Path.Combine(_mediaDir, fileName));
//         await BotMediaHelper.DownloadAndSaveTelegramFileAsync(_botClient, 
//             photo.GetFileByQuality(PhotoQuality.High).FileId, fp);
//         return fp;
//     }
//
//     private string GetFileIdFromFileName(string fileName) => fileName.Replace(".jpeg", "");
//     private string GetFileNameFromFileId(string fileId) => fileId + ".jpeg";
// }