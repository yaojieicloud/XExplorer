using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using UraniumUI;
using XExplorer.Core.Modes;

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
            fonts.AddMaterialIconFonts();
        }).UseMauiCommunityToolkit();

        builder.UseUraniumUI()
            .UseMauiCommunityToolkit()
            .UseUraniumUIMaterial();
        
        #region 初始化

        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().Enrich.WithThreadId().WriteTo.File(
                "logs/XExplorer.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {ThreadId}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        AppSettingsUtils.LoadJson(GetAppSettingsPath());
        Log.Information("The application has started.");

        #endregion
       
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }

    private static string GetAppSettingsPath()
    {
        var basePath = Environment.CurrentDirectory;

#if MACCATALYST
        basePath = Path.Combine(basePath, "Contents", "Resources");
#endif

        return Path.Combine(basePath, "appsettings.json");
    }
}