using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AssetRipper.GUI
{
	public partial class MainWindow : Window
    {
	    public static MainWindow Instance;
	    public TextBox LogText;

	    private MainWindowViewModel VM => (MainWindowViewModel) DataContext!;
	    
	    public MainWindow()
	    {
		    Instance = this;

		    InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

		    LogText = this.Get<TextBox>("LogText");

		    AddHandler(DragDrop.DropEvent, (sender, args) => VM.OnFileDropped(args));
	    }

	    protected override void OnDataContextChanged(EventArgs e)
	    {
		    base.OnDataContextChanged(e);
		    
		    VM.HasFile = false;
		    VM.HasLoaded = false;
	    }

	    private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

	    //Called from UI
	    private void ExitClicked(object? sender, RoutedEventArgs e) => Close();

	    //Called from UI
	    private void ExportAllClicked(object? sender, RoutedEventArgs e) => VM.ExportAll();

	    private void OnAssetSelected(object? sender, SelectionChangedEventArgs e)
	    {
		    NewUiFileListItem selectedItem = (NewUiFileListItem) e.AddedItems[0]!;
		    if (selectedItem.AsObjectAsset == null)
		    {
			    //Ignore non-asset files
			    return;
		    }

		    VM.OnAssetSelected(selectedItem, selectedItem.AsObjectAsset);
	    }

	    private void CheckforUpdateClicked(object? sender, RoutedEventArgs e)
	    {
		    
	    }

	    private void GithubClicked(object? sender, RoutedEventArgs e) =>
		    OpenUrl("https://github.com/ds5678/AssetRipper");

	    private void WebsiteClicked(object? sender, RoutedEventArgs e) =>
		    OpenUrl("https://ds5678.github.io/AssetRipper/");

	    private static void OpenUrl(string url)
	    {
		    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		    {
			    Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
		    }
		    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		    {
			    Process.Start("xdg-open", url);
		    }
		    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		    {
			    Process.Start("open", url);
		    }
	    }
    }
}