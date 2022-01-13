using Avalonia.Controls;
using System;

namespace AssetRipper.GUI
{
	public class MessageBoxViewModel : BaseViewModel
	{

		public enum Buttons
		{
			Okay,
			YesNoCancel,
			YesNo
		}

		public enum Result
		{
			Okay,
			Yes,
			No,
			Cancel
		}

		public string BodyText
		{
			get => _bodyText;
			set
			{
				_bodyText = value;
				OnPropertyChanged();
			}
		}

		public bool OkayVisible
		{
			get => _currentButtons == Buttons.Okay;
		}

		public bool YesVisible
		{
			get => _currentButtons == Buttons.YesNo | _currentButtons == Buttons.YesNoCancel;
		}

		public bool NoVisible
		{
			get => _currentButtons == Buttons.YesNo | _currentButtons == Buttons.YesNoCancel;
		}

		public bool CancelVisible
		{
			get => _currentButtons == Buttons.YesNoCancel;
		}


		private string _bodyText = "<null>";
		private Buttons _currentButtons;
		private readonly Action<Result>? _callback;

		public MessageBoxViewModel() : this("<null>") { }
		public MessageBoxViewModel(string body, Buttons buttons = Buttons.Okay, Action<Result>? callback = null)
		{
			BodyText = body;
			_currentButtons = buttons;
			_callback = callback;
		}

		// Called by UI
		private void CancelClicked(Window window)
		{
			_callback?.Invoke(Result.Cancel);
			window.Close();
		}

		// Called by UI
		private void NoClicked(Window window)
		{
			_callback?.Invoke(Result.No);
			window.Close();
		}

		// Called by UI
		private void YesClicked(Window window)
		{
			_callback?.Invoke(Result.Yes);
			window.Close();
		}

		// Called by UI
		private void OkayClicked(Window window)
		{
			_callback?.Invoke(Result.Okay);
			window.Close();
		}
	}
}