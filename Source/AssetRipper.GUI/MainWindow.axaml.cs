using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AssetRipper.Import.Logging;
#if DEBUG
using Avalonia;
#endif

namespace AssetRipper.GUI
{
	public partial class MainWindow : Window
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static MainWindow Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		private MainWindowViewModel VM;
		public readonly LocalizationManager LocalizationManager;

		public MainWindow()
		{
			Instance = this;
			DataContext = VM = new();

			LocalizationManager = new();

			Logger.Verbose(LogCategory.System, $"Available languages: {string.Join(", ", LocalizationManager.SupportedLanguages.Select(l => l.LanguageCode))}");

			InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

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

		//Called from UI
		private void ExportSelectedAssetTypeToProjectClicked(object? sender, RoutedEventArgs e) => VM.ExportSelectedAssetTypeToProject();

		private void OnAssetSelected(object? sender, SelectionChangedEventArgs e)
		{
			NewUiFileListItem selectedItem = (NewUiFileListItem)e.AddedItems[0]!;
			if (selectedItem.AsObjectAsset == null)
			{
				//Ignore non-asset files
				return;
			}

			VM.OnAssetSelected(selectedItem, selectedItem.AsObjectAsset);
		}

		private void LanguageMenuItemClicked(object? sender, RoutedEventArgs e)
		{
			if (sender is not MenuItem { SelectedItem: LocalizationManager.SupportedLanguage language })
			{
				return;
			}

			Logger.Info(LogCategory.System, $"User selected language {language}");

			language.Apply();
		}
	}
}
