using System;
using System.Text;
using System.Threading;
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
			// skip this log type cause RichTextBox is insanely slow
			if (category == LogCategory.Export && type == LogType.Info)
			{
				return;
			}

			if (Application.Current == null)
			{
				return;
			}
			if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
			{
				LogInner(type, category, message);
			}
			else
			{
				Application.Current.Dispatcher.Invoke(() => LogInner(type, category, message));
			}
		}

		private void LogInner(LogType type, LogCategory category, string message)
		{
			TextRange rangeOfText = new TextRange(m_textBox.Document.ContentEnd, m_textBox.Document.ContentEnd);
			m_sb.Append(category.ToString());
			m_sb.Append(':').Append(' ');
			m_sb.Append(message);
			m_sb.Append('\r');
			m_sb.Replace("\n", string.Empty);
			rangeOfText.Text = m_sb.ToString();
			m_sb.Clear();

			if (type != m_lastLogType)
			{
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
				m_lastLogType = type;
			}
		}

		private readonly StringBuilder m_sb = new StringBuilder();
		private readonly RichTextBox m_textBox;

		private LogType m_lastLogType = LogType.Info;
	}
}
