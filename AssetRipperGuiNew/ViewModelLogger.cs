using AssetRipper.Core.Logging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace AssetRipperGuiNew
{
	public sealed class ViewModelLogger : ILogger
	{
		private readonly MainWindowViewModel viewModel;

		public ViewModelLogger(MainWindowViewModel? viewModel)
		{
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}

		public void Log(LogType type, LogCategory category, string message)
		{
			if (Application.Current == null)
			{
				return;
			}
			if (Dispatcher.UIThread.CheckAccess())
			{
				LogInner(type, category, message);
			}
			else
			{
				Dispatcher.UIThread.Post(() => LogInner(type, category, message));
			}
		}

		private void LogInner(LogType type, LogCategory category, string message)
		{
			if(category != LogCategory.None)
				viewModel.LogText += $"{category.ToString()} ";
			
			switch (type)
			{
				case LogType.Warning:
				case LogType.Error:
					viewModel.LogText += $"[{type.ToString()}] ";
					break;
			}

			viewModel.LogText += ": ";
			viewModel.LogText += message;
			viewModel.LogText += Environment.NewLine;
		}

		public void BlankLine(int numLines)
		{
			for(int i = 0; i < numLines; i++)
				viewModel.LogText += Environment.NewLine;
		}
	}
}
