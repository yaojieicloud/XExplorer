using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Serilog;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;
using XExplorer.Core.Service;

namespace XExplorer.Core.ViewModel.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
    }

    [RelayCommand]
    public async Task SetPasswordAsync(string password = null)
    {
        password ??= this.Password;
        var pwds = await this.dataService.PwdService.GetAsync();
        if (pwds?.Any(m => m.Pwd == password) ?? false)
            return;

        await this.dataService.PwdService.AddAsync(password);

        this.Notification("新增密码完成！");
    }

    /// <summary>
    /// 异步解压压缩文件的方法。
    /// 搜索指定文件夹中的压缩文件类型，并尝试解压它们。
    /// 支持的压缩文件类型包括：.zip、.7z、.rar、.tar、.gz、.bz2、.xz、.iso、.cab、.tgz、.lz、.z。
    /// </summary>
    /// <remarks>
    /// 该方法会弹出文件夹选择器以选择目标文件夹，获取其中支持类型的压缩文件，并利用已存储的密码或默认空密码进行解压操作。
    /// 解压完成后会生成通知，记录解压状态和所用时间。
    /// </remarks>
    /// <returns>表示异步操作的任务。</returns>
    [RelayCommand]
    public async Task UnzipAsync()
    {
        var st = Stopwatch.StartNew();
        Processing = true;

        try
        {
            var result = await FolderPicker.PickAsync(default);
            if (result != null)
            {
                if (result.Folder == null)
                    return;

                var dir = new DirRecord
                {
                    Name = result.Folder.Name,
                    FullName = result.Folder.Path,
                    ValidName = result.Folder.Path.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty)
                };

                var pwds = await this.dataService.PwdService.GetAsync();
                var pwdStrings = pwds.Select(m => m.Pwd).ToList();
                pwdStrings.Add(string.Empty);
                var reuqest = new UnzipRequest { Dir = dir.ValidName, Passwords = pwdStrings };

                start:
                try
                {
                    using var client = new HttpClient();
                    using var cont = new StringContent(JsonConvert.SerializeObject(reuqest), Encoding.UTF8,
                        "application/json");
                    client.Timeout = TimeSpan.FromHours(2);
                    var response = await client.PostAsync(AppSettingsUtils.Default.Current.ZipUrl, cont);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    client.Dispose();
                    Notification($"服务返回：{responseBody}");
                }
                catch (HttpRequestException hrex)
                {
                    Log.Error(hrex, hrex.Message);
                    goto start;
                }
 
                Notification($"文件夹 [{dir.FullName}] 全部解压完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            }
        }
        catch (Exception e)
        {
            Notification($"{e}");
            Log.Error(e, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
        finally
        {
            st.Stop();
            Log.Information($"全部处理完成，耗时[{st.Elapsed.TotalSeconds}]秒。");
            Processing = false;
        }
    }

    /// <summary>
    ///     打开日志目录。
    /// </summary>
    /// <remarks>
    ///     此方法首先获取当前应用程序域的基目录，然后构造日志目录的路径。最后，它使用Windows资源管理器打开日志目录。
    /// </remarks>
    [RelayCommand]
    public async Task OpenLogDirAsync()
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
            {
                // Adjust the path for macOS to get the app bundle root directory
                var path = Path.GetDirectoryName(AppSettingsUtils.Default.Current.LogFile);
                OpenFolder(path);
            }
            else
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                OpenFolder(path);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
            Notification($"{ex}");
        }

        await Task.CompletedTask;
    }
}