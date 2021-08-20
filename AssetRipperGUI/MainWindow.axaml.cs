using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

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

	    private void ExitClicked(object? sender, RoutedEventArgs e) => Close();

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
    }
}