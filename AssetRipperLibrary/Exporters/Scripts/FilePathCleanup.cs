using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal static class FilePathCleanup
	{
		const int maxSegmentLength = 255;

		/// <summary>
		/// Cleans up a node name for use as a file name.
		/// </summary>
		public static string CleanUpFileName(string text)
		{
			return CleanUpName(text, separateAtDots: false, treatAsFileName: false);
		}

		/// <summary>
		/// Removes invalid characters from file names and reduces their length,
		/// but keeps file extensions and path structure intact.
		/// </summary>
		public static string SanitizeFileName(string fileName)
		{
			return CleanUpName(fileName, separateAtDots: false, treatAsFileName: true);
		}

		/// <summary>
		/// Cleans up a node name for use as a file system name. If <paramref name="separateAtDots"/> is active,
		/// dots are seen as segment separators. Each segment is limited to maxSegmentLength characters.
		/// (see <see cref="GetLongPathSupport"/>) If <paramref name="treatAsFileName"/> is active,
		/// we check for file a extension and try to preserve it, if it's valid.
		/// </summary>
		static string CleanUpName(string text, bool separateAtDots, bool treatAsFileName)
		{
			int pos = text.IndexOf(':');
			if (pos > 0)
			{
				text = text.Substring(0, pos);
			}

			pos = text.IndexOf('`');
			if (pos > 0)
			{
				text = text.Substring(0, pos);
			}

			text = text.Trim();
			string? extension = null;
			int currentSegmentLength = 0;
			if (treatAsFileName)
			{
				// Check if input is a file name, i.e., has a valid extension
				// If yes, preserve extension and append it at the end.
				// But only, if the extension length does not exceed maxSegmentLength,
				// if that's the case we just give up and treat the extension no different
				// from the file name.
				int lastDot = text.LastIndexOf('.');
				if (lastDot >= 0 && text.Length - lastDot < maxSegmentLength)
				{
					string originalText = text;
					extension = text.Substring(lastDot);
					text = text.Remove(lastDot);
					foreach (char c in extension)
					{
						if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
						{
							// extension contains an invalid character, therefore cannot be a valid extension.
							extension = null;
							text = originalText;
							break;
						}
					}
				}
			}
			// Whitelist allowed characters, replace everything else:
			StringBuilder b = new StringBuilder(text.Length + (extension?.Length ?? 0));
			foreach (char c in text)
			{
				currentSegmentLength++;
				if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
				{
					// if the current segment exceeds maxSegmentLength characters,
					// skip until the end of the segment.
					if (currentSegmentLength <= maxSegmentLength)
					{
						b.Append(c);
					}
				}
				else if (c == '.' && b.Length > 0 && b[b.Length - 1] != '.')
				{
					// if the current segment exceeds maxSegmentLength characters,
					// skip until the end of the segment.
					if (separateAtDots || currentSegmentLength <= maxSegmentLength)
					{
						b.Append('.'); // allow dot, but never two in a row
					}

					// Reset length at end of segment.
					if (separateAtDots)
					{
						currentSegmentLength = 0;
					}
				}
				else if (treatAsFileName && (c == '/' || c == '\\') && currentSegmentLength > 0)
				{
					// if we treat this as a file name, we've started a new segment
					b.Append(c);
					currentSegmentLength = 0;
				}
				else
				{
					// if the current segment exceeds maxSegmentLength characters,
					// skip until the end of the segment.
					if (currentSegmentLength <= maxSegmentLength)
					{
						b.Append('-');
					}
				}
			}
			if (b.Length == 0)
			{
				b.Append('-');
			}

			string name = b.ToString();
			if (extension != null)
			{
				name += extension;
			}

			if (IsReservedFileSystemName(name))
			{
				return name + "_";
			}
			else if (name == ".")
			{
				return "_";
			}
			else
			{
				return name;
			}
		}

		/// <summary>
		/// Cleans up a node name for use as a directory name.
		/// </summary>
		public static string CleanUpDirectoryName(string text)
		{
			return CleanUpName(text, separateAtDots: false, treatAsFileName: false);
		}

		public static string CleanUpPath(string text)
		{
			return CleanUpName(text, separateAtDots: true, treatAsFileName: false)
				.Replace('.', Path.DirectorySeparatorChar);
		}

		private static bool IsReservedFileSystemName(string name)
		{
			switch (name.ToUpperInvariant())
			{
				case "AUX":
				case "COM1":
				case "COM2":
				case "COM3":
				case "COM4":
				case "COM5":
				case "COM6":
				case "COM7":
				case "COM8":
				case "COM9":
				case "CON":
				case "LPT1":
				case "LPT2":
				case "LPT3":
				case "LPT4":
				case "LPT5":
				case "LPT6":
				case "LPT7":
				case "LPT8":
				case "LPT9":
				case "NUL":
				case "PRN":
					return true;
				default:
					return false;
			}
		}
	}
}
