using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using uTinyRipper;

namespace uTinyRipperGUI
{
	public sealed class OutputLogger : ILogger
	{
		public OutputLogger(RichTextBox textBox)
		{
			if(textBox == null)
			{
				throw new ArgumentNullException(nameof(textBox));
			}
			m_textBox = textBox;
		}

		public void Log(LogType type, LogCategory category, string message)
		{
			Application.Current.Dispatcher.InvokeAsync(() => LogInner(type, category, message));
		}

		private void LogInner(LogType type, LogCategory category, string message)
		{
			TextRange rangeOfText = new TextRange(m_textBox.Document.ContentEnd, m_textBox.Document.ContentEnd);
			rangeOfText.Text = $"{category}: {message}\r";
			switch (type)
			{
				case LogType.Debug:
					rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Silver);
					rangeOfText.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
					break;
				case LogType.Info:
					rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
					rangeOfText.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
					break;
				case LogType.Warning:
					rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gold);
					rangeOfText.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
					break;
				case LogType.Error:
					rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
					rangeOfText.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
					break;

				default:
					throw new Exception($"Unsupported log type '{type}'");
			}
		}

		private readonly RichTextBox m_textBox;
	}
}
