using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using XExplorer.Core.ViewModel.Settings;

namespace XExplorer.Core.ViewModel.Browser;

public partial class BrowserViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the collection of URLs representing the navigation history.
    /// </summary>
    /// <remarks>The collection maintains the sequence of previously visited URLs and can be used to implement
    /// features such as back or forward navigation. Changes to the collection are observable, allowing UI elements to
    /// update automatically when the history changes.</remarks>
    [ObservableProperty]
    private ObservableCollection<string> historyUrls = new();

    /// <summary>
    /// Gets or sets the address of the target resource.
    /// </summary>
    [ObservableProperty]
    private string address = "https://www.xvideos.com";

    /// <summary>
    /// Gets or sets the URL of the video associated with this instance.
    /// </summary>
    [ObservableProperty]
    private string webUrl;

    /// <summary>
    /// 是否显示下载界面
    /// </summary>
    [ObservableProperty] 
    private bool showDownload;

    public BrowserViewModel()
    {
        this.InitDirs();
        this.InitHistUrls();
    }

    [RelayCommand]
    public async void Init()
    {
        this.InitDirs();
        this.InitHistUrls();
    }

    [RelayCommand]
    public async void Goto()
    {
        this.WebUrl = this.Address;

        if (!string.IsNullOrWhiteSpace(this.webUrl) && !this.HistoryUrls.Contains(this.webUrl))
            this.HistoryUrls.Add(this.webUrl);
    }

    [RelayCommand]
    private void UpdateUrl(object args)
    {
        // 如果使用了 Converter，args 直接就是 WebNavigatedEventArgs
        if (args is WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                // 更新属性，同步 UI
                this.Address = e.Url;
                this.webUrl = e.Url;
            }
        }
        else if (args is string url)
        {
            this.Address = url;
            this.webUrl = url;
        }
    }

    private void InitHistUrls()
    {
        this.HistoryUrls.Clear();
        this.HistoryUrls.Add("https://www.youtube.com/");
        this.HistoryUrls.Add("https://www.xvideos.com/");
        this.HistoryUrls.Add("https://cn.pornhub.com/");
    }
}
