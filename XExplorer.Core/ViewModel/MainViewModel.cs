using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
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
public partial class MainViewModel : ObservableObject
{
    /// <summary>
    ///     MainViewModel 类是应用程序中 MVVM 架构的主要 ViewModel。
    ///     此类扩展了 CommunityToolkit.Mvvm.ComponentModel 命名空间中的
    ///     ObservableObject，实现了应用程序 UI 所需的状态和逻辑管理，
    ///     促进了视图层和模型层之间的数据绑定。
    /// </summary>
    public MainViewModel()
    { 
        this.dataService = new DataService();
        this.InitDirs();
    }

    #region Command

    /// <summary>
    ///     异步加载选定目录中的视频数据。
    /// </summary>
    /// <returns>返回一个执行加载操作的任务。</returns>
    [RelayCommand]
    public async Task LoadDirAsync()
    {
        try
        {
            Processing = true;
            Videos.Clear();
            var enties = await dataService.VideosService.QueryAsync(this.SelectedDir.ValidName);
            var modes = enties.ToModes();
            Videos = new ObservableCollection<VideoMode>(modes);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{MethodBase.GetCurrentMethod().Name} Is Error");
        }
        finally
        {
            Processing = false;
        }
    }

    #endregion
}