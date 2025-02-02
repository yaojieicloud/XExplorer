using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Maui.Core.Hosting;
using UraniumUI;
using Xabe.FFmpeg;
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
            })
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit();

        builder.UseUraniumUI()
            .UseMauiCommunityToolkit()
            .UseUraniumUIMaterial();

        #region 初始化
        AppSettingsUtils.LoadJson(GetAppSettingsPath());
        
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().Enrich.WithThreadId().WriteTo.File(
                AppSettingsUtils.Default.Current.LogFile, rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {ThreadId}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger(); 
        Log.Information("The application has started.");

        SetFFmpegPath();

        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDAxNEAzMjM4MkUzMTJFMzlMeXJkaVJFV2Z5R3o5ZXNEVnNOQjFqUmx2MW0xZkR2TGdud2MrVGNJRlBzPQ==");
        
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

    private static void SetFFmpegPath()
    {
        var basePath = Environment.CurrentDirectory;
        var ffmpegDir = Path.Combine(AppContext.BaseDirectory, "ffmpeg", AppSettingsUtils.Default.OS);

#if MACCATALYST
        basePath = Path.Combine(basePath, "Contents", "Resources");
        ffmpegDir = Path.Combine(basePath, "ffmpeg", AppSettingsUtils.Default.OS);
#endif
        FFmpeg.SetExecutablesPath(ffmpegDir);
    }
}