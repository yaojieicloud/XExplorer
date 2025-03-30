using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
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

    [RelayCommand]
    public async Task UnzipAsync()
    {
        var zipExts = new HashSet<string>()
            { ".zip", ".7z", ".rar", ".tar", ".gz", ".bz2", ".xz", ".iso", ".cab", ".tgz", ".lz", ".z" };
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

                var dirInfo = new DirectoryInfo(dir.FullName);
                var compressedFiles = dirInfo.GetFiles()
                    .Where(file => zipExts.Contains(file.Extension.ToLower()))
                    .ToList();
                var pwds = await this.dataService.PwdService.GetAsync();
                var pwdStrings = pwds.Select(m => m.Pwd).ToList();
                pwdStrings.Add(string.Empty);

                await Task.Run(() =>
                {
                    foreach (var file in compressedFiles)
                    {
                        this.ExtractArchive(file.FullName, pwdStrings, dirInfo.FullName);
                    }
                });
                
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
}