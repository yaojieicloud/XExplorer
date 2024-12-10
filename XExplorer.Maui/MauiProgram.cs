using Microsoft.Extensions.Logging;
using Serilog;
using XExplorer.Core.Modes;
using CommunityToolkit.Maui;

namespace XExplorer.Maui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().Enrich.WithThreadId().WriteTo.File("logs\\XExplorer.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {ThreadId}] {Message:lj}{NewLine}{Exception}").CreateLogger();
        AppSettingsUtils.LoadJson(GetAppSettingsPath());
        Log.Information("The application has started.");
         
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
    
    private static string GetAppSettingsPath()
    {
        string basePath = Environment.CurrentDirectory;

#if MACCATALYST
        basePath = Path.Combine(basePath, "Contents", "Resources");
#endif

        return Path.Combine(basePath, "appsettings.json");
    }
}