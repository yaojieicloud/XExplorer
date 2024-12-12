using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using XExplorer.Core.Dictionaries;
using XExplorer.Core.Modes;
using XExplorer.Core.Service;
using XExplorer.Core.Utils;

namespace XExplorer.Core.ViewModel;

/// <summary>
///     The MainViewModel class serves as the primary ViewModel in the MVVM architecture
///     for the application, extending the functionality provided by the
///     ObservableObject from the CommunityToolkit.Mvvm.ComponentModel namespace.
///     This class is responsible for maintaining the state and logic required for
///     the UI, facilitating the data binding between the view and the model layers.
/// </summary>
partial class MainViewModel
{
    /// <summary>
    /// 初始化目录信息的方法。
    /// 此方法会从配置的根目录中获取所有子目录，并将其信息转化为 <see cref="DirInfo"/> 对象集合，
    /// 最终赋值给当前 ViewModel 的 <c>Dirs</c> 属性。
    /// </summary>
    /// <remarks>
    /// 具体步骤如下：
    /// 1. 调用 <see cref="Directory.GetDirectories(string)"/> 方法获取根目录中的所有子目录路径。
    /// 2. 遍历每一个子目录路径，将其转换为 <see cref="DirInfo"/> 对象实例，包含目录的名称、完整路径和有效名称信息。
    /// 3. 将所有的 <see cref="DirInfo"/> 对象存入 <see cref="ObservableCollection{T}"/> 集合中，并更新 <c>Dirs</c> 属性。
    /// </remarks>
    /// <example>
    /// 假设配置的根目录包括如下子目录：
    /// <list type="bullet">
    /// <item>C:\Root\Dir1</item>
    /// <item>C:\Root\Dir2</item>
    /// </list>
    /// 调用本方法后，<c>Dirs</c> 属性会包含以下内容：
    /// <list type="bullet">
    /// <item>目录名称：Dir1，有效名称：\Dir1，完整路径：C:\Root\Dir1</item>
    /// <item>目录名称：Dir2，有效名称：\Dir2，完整路径：C:\Root\Dir2</item>
    /// </list>
    /// </example>
    private void InitDirs()
    {
        var videoDirs = new List<DirInfo>();
        var allDirs = Directory.GetDirectories(AppSettingsUtils.Default.Current.RootDir);
        for (int i = 0; i < allDirs.Length; i++)
        {
            var dir = allDirs[i];
            var dirInfo = new DirectoryInfo(dir);
            var valid = dir.Replace(AppSettingsUtils.Default.Current.Volume, string.Empty);
            videoDirs.Add(new() { Name = dirInfo.Name, FullName = dir, ValidName = valid });
        } 
        
        this.Dirs = new ObservableCollection<DirInfo>(videoDirs);
    }

    private string AdjustPath(string path)
    {
        if (AppSettingsUtils.Default.OS == OS.MacCatalyst)
        {
            // 将 Windows 路径转换成 Mac 的路径
            path = path.Replace(AppSettingsUtils.Default.Windows.Volume, AppSettingsUtils.Default.Mac.Volume); 
            path = path.Replace('\\', '/');
        }
        else
        {
            // 将 Mac 路径转换成 Windows 的路径
            path = path.Replace(AppSettingsUtils.Default.Mac.Volume, AppSettingsUtils.Default.Windows.Volume); 
            path = path.Replace('/', '\\');
        }
        
        return path;
    }
}