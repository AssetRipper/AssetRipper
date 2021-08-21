using Avalonia.Threading;
using MessageBox.Avalonia;

namespace AssetRipper.GUI.Extensions
{
	public static class UIExtentions
	{
		public static void ShowPopup(this BaseViewModel _, string body, string title = "Message") => Dispatcher.UIThread.Post(() => ShowPopup(body, title));

		private static void ShowPopup(string body, string title)
		{
			MessageBoxManager.GetMessageBoxStandardWindow(title, body)
				.Show();
		}
	}
}