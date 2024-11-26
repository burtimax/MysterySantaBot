using MultipleBotFramework.Utils;
using MysterySantaBot.Resources;

namespace MysterySantaBot.Extensions;

public static class IServiceCollectionExtensions
{
    public static BotResources ConfigureBotResources(this IServiceCollection services, string resourcesFilePath)  
    {  
        if (resourcesFilePath == null) throw new ArgumentNullException(nameof(resourcesFilePath));  
	  
        string json = File.ReadAllText(resourcesFilePath);  
        BotResourcesBuilder resourcesBuilder = new(json);  
        json = resourcesBuilder.Build();  
	  
        Stream jsonStream = StreamHelper.GenerateStreamFromString(json);  
        var resourcesConfigBuilder = new ConfigurationBuilder().AddJsonStream(jsonStream);  
        IConfiguration resourcesConfiguration = resourcesConfigBuilder.Build();  
	  
        services.Configure<BotResources>(resourcesConfiguration);  
        return resourcesConfiguration.Get<BotResources>();  
    }
}