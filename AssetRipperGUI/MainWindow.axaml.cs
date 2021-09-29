using AssetRipper.Core.Logging;
using AssetRipper.GUI.Components;
using AssetRipper.Library.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace AssetRipper.GUI
{
	public partial class MainWindow : Window
    {
	    public static MainWindow Instance;
	    public TextBox LogText;

	    private MainWindowViewModel VM;
	    public readonly LocalizationManager LocalizationManager = new();
	    
	    public MainWindow()
	    {
		    Instance = this;
		    DataContext = VM = new();

		    LocalizationManager.Init();

		    Logger.Info(LogCategory.System, $"Available languages: {string.Join(", ", LocalizationManager.SupportedLanguages.Select(l => l.LanguageCode))}");

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

		private void LanguageMenuItemClicked(object? sender, RoutedEventArgs e)
		{
			if(sender is not MenuItem {SelectedItem: LocalizationManager.SupportedLanguage language})
				return;

			Logger.Info(LogCategory.System, $"User selected language {language}");
			
			language.Apply();
		}
    }
}