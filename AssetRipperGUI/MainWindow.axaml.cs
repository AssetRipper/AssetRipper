using AssetRipper.GUI.Components;
using AssetRipper.Library.Configuration;
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

	    private MainWindowViewModel VM;
	    
	    public MainWindow()
	    {
		    Instance = this;
		    DataContext = VM = new();

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

		//Called from UI
		private void ExportSelectedAssetToProjectClicked(object? sender, RoutedEventArgs e) => VM.ExportSelectedAssetToProject();

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