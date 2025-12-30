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

    private Stack<string> backStack = new();
    private Stack<string> forwardStack = new();

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

    private bool isGoBack = false;

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
        this.isGoBack = false;

        if (!string.IsNullOrWhiteSpace(this.webUrl) && !this.HistoryUrls.Contains(this.webUrl))
            this.HistoryUrls.Add(this.webUrl);
    }

    [RelayCommand]
    private void UpdateUrl(object args)
    { 
        if (args is string url)
        { 
            if (!string.IsNullOrWhiteSpace(this.webUrl) && !this.HistoryUrls.Contains(this.webUrl))
                this.HistoryUrls.Insert(0, this.webUrl);

            if (isGoBack)
            {
                if (!this.forwardStack.Any())
                    this.forwardStack.Push(this.Address);
                else if (this.forwardStack.TryPeek(out var tmpUrl))
                    if (url != tmpUrl)
                        this.forwardStack.Push(this.Address);
                this.isGoBack = false;
            }
            else
            {
                var currUrl = new Uri(url);
                var addressUri = new Uri(this.Address);
                if (currUrl == addressUri)
                    return;

                if (!this.backStack.Any())
                    this.backStack.Push(this.Address);
                else if (this.backStack.TryPeek(out var tmpUrl))
                    if (url != tmpUrl)
                        this.backStack.Push(this.Address);

                this.forwardStack.Clear();
            }

            this.Address = url;
            this.webUrl = url;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (this.backStack.TryPop(out string url))
        {
            this.isGoBack = true;
            this.WebUrl = url;            
        }
    }

    [RelayCommand]
    private void GoForward()
    {
        if (this.forwardStack.TryPop(out string url))
        {
            this.isGoBack = false;
            this.WebUrl = url;
            
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
