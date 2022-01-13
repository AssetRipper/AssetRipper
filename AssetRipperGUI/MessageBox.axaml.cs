using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
#if DEBUG
using Avalonia;
#endif

namespace AssetRipper.GUI
{
	public partial class MessageBox : Window
	{
		public MessageBox()
		{
			InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public static void Popup(string title, string body,
			MessageBoxViewModel.Buttons buttons = MessageBoxViewModel.Buttons.Okay,
			Action<MessageBoxViewModel.Result>? callback = null)
		{
			MessageBox messageBox = new()
			{
				DataContext = new MessageBoxViewModel(body, buttons, callback),
				Title = title
			};
			messageBox.Show();
		}
	}
}
