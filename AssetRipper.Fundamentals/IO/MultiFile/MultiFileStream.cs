using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.IO.MultiFile
{
	public sealed class MultiFileStream : Stream
	{
		public MultiFileStream(IEnumerable<Stream> streams)
		{
			if (streams == null)
			{
				throw new ArgumentNullException(nameof(streams));
			}
			foreach (Stream stream in streams)
			{
				if (stream == null)
				{
					throw new ArgumentNullException();
				}
				if (!stream.CanSeek)
				{
					throw new Exception($"Stream {stream} isn't seekable");
				}
			}

			m_streams = streams.ToArray();
			if (m_streams.Count == 0)
			{
				throw new ArgumentException(nameof(streams));
			}

			Length = streams.Sum(t => t.Length);
			CanRead = m_streams.All(t => t.CanRead);
			CanWrite = m_streams.All(t => t.CanWrite);
			UpdateCurrentStream();
		}

		~MultiFileStream()
		{
			Dispose(false);
		}

		/// <summary>
		/// Determines if the path could be part of a multi file
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>True if the path matches the multi file regex</returns>
		public static bool IsMultiFile(string path)
		{
			return s_splitCheck.IsMatch(path);
		}

		public static bool Exists(string path)
		{
			if (IsMultiFile(path))
			{
				SplitPathWithoutExtension(path, out string directory, out string file);
				return Exists(directory, file);
			}
			if (File.Exists(path))
			{
				return true;
			}

			{
				SplitPath(path, out string directory, out string file, true);
				if (string.IsNullOrEmpty(file))
					return false;
				else
					return Exists(directory, file);
			}
		}

		public static Stream OpenRead(string path)
		{
			if (IsMultiFile(path))
			{
				SplitPathWithoutExtension(path, out string directory, out string file);
				return OpenRead(directory, file);
			}
			if (File.Exists(path))
			{
				return File.OpenRead(path);
			}

			{
				SplitPath(path, out string directory, out string file);
				return OpenRead(directory, file);
			}
		}

		public static string GetFilePath(string path)
		{
			if (IsMultiFile(path))
			{
				int index = path.LastIndexOf('.');
				return path.Substring(0, index);
			}
			return path;
		}

		public static string GetFileName(string path)
		{
			if (IsMultiFile(path))
			{
				return Path.GetFileNameWithoutExtension(path);
			}
			return Path.GetFileName(path);
		}

		public static string[] GetFiles(string path)
		{
			if (IsMultiFile(path))
			{
				SplitPathWithoutExtension(path, out string directory, out string file);
				return GetFiles(directory, file);
			}

			if (File.Exists(path))
			{
				return new[] { path };
			}
			return Array.Empty<string>();
		}

		public static bool IsNameEquals(string fileName, string compare)
		{
			fileName = GetFileName(fileName);
			return fileName == compare;
		}

		/// <summary>
		/// Determines if a multi file exists
		/// </summary>
		/// <param name="dirPath">The directory containing the multi file</param>
		/// <param name="fileName">The name of the multi file without the split extension</param>
		/// <returns>True if a valid multi file exists in that directory with that name</returns>
		private static bool Exists(string dirPath, string fileName)
		{
			string filePath = Path.Combine(dirPath, fileName);
			string splitFilePath = filePath + ".split";

			string[] splitFiles = GetFiles(dirPath, fileName);
			if (splitFiles.Length == 0)
			{
				return false;
			}

			for (int i = 0; i < splitFiles.Length; i++)
			{
				string indexFileName = splitFilePath + i;
				if (!splitFiles.Contains(indexFileName))
				{
					return false;
				}
			}
			return true;
		}

		private static string[] GetFiles(string dirPath, string fileName)
		{
			if (!Directory.Exists(dirPath))
			{
				return Array.Empty<string>();
			}

			string filePatern = fileName + ".split*";
			return Directory.GetFiles(dirPath, filePatern);
		}

		private static Stream OpenRead(string dirPath, string fileName)
		{
			string filePath = Path.Combine(dirPath, fileName);
			string splitFilePath = filePath + ".split";

			string[] splitFiles = GetFiles(dirPath, fileName);
			for (int i = 0; i < splitFiles.Length; i++)
			{
				string indexFileName = splitFilePath + i;
				if (!splitFiles.Contains(indexFileName))
				{
					throw new Exception($"Try to open splited file part '{filePath}' but file part '{indexFileName}' wasn't found");
				}
			}

			splitFiles = splitFiles.OrderBy(t => t, s_splitNameComparer).ToArray();
			Stream[] streams = new Stream[splitFiles.Length];
			try
			{
				for (int i = 0; i < splitFiles.Length; i++)
				{
					Stream stream = File.OpenRead(splitFiles[i]);
					streams[i] = stream;
				}

				return new MultiFileStream(streams);
			}
			catch
			{
				foreach (Stream stream in streams)
				{
					if (stream == null)
					{
						break;
					}
					stream.Dispose();
				}
				throw;
			}
		}

		private static void SplitPath(string path, out string directory, out string file) => SplitPath(path, out directory, out file, false);
		private static void SplitPath(string path, out string directory, out string file, bool allowNullReturn)
		{
			directory = Path.GetDirectoryName(path);
			directory = string.IsNullOrEmpty(directory) ? "." : directory;
			file = Path.GetFileName(path);
			if (string.IsNullOrEmpty(file) && !allowNullReturn)
			{
				throw new Exception($"Can't determine file name for {path}");
			}
		}

		private static void SplitPathWithoutExtension(string path, out string directory, out string file)
		{
			directory = Path.GetDirectoryName(path);
			directory = string.IsNullOrEmpty(directory) ? "." : directory;
			file = Path.GetFileNameWithoutExtension(path);
			if (string.IsNullOrEmpty(file))
			{
				throw new Exception($"Can't determine file name for {path}");
			}
		}

		public override void Flush()
		{
			m_currentStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position += offset;
					break;
				case SeekOrigin.End:
					Position = Length - offset;
					break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int ReadByte()
		{
			int value = m_currentStream.ReadByte();
			if (value >= 0)
			{
				m_position++;
				if (m_position == m_currentEnd)
				{
					NextStream();
				}
			}
			return value;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = m_currentStream.Read(buffer, offset, count);
			m_position += read;
			if (m_position == m_currentEnd)
			{
				NextStream();
			}

			return read;
		}

		public override void WriteByte(byte value)
		{
			m_currentStream.WriteByte(value);
			m_position++;
			if (m_position == m_currentEnd)
			{
				NextStream();
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			while (count > 0)
			{
				long available = m_currentEnd - m_position;
				int toWrite = count < available ? count : (int)available;
				m_currentStream.Write(buffer, offset, toWrite);
				m_position += toWrite;
				if (m_position == m_currentEnd)
				{
					NextStream();
				}

				offset += toWrite;
				count -= toWrite;
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach (Stream stream in m_streams)
			{
				stream.Dispose();
			}
			base.Dispose(disposing);
		}

		private void NextStream()
		{
			int nextStreamIndex = m_streamIndex + 1;
			if (nextStreamIndex < m_streams.Count)
			{
				m_currentBegin += m_currentStream.Length;
				m_streamIndex = nextStreamIndex;
				m_currentStream = m_streams[m_streamIndex];
				m_currentStream.Position = 0;
				m_currentEnd += m_currentStream.Length;
			}
		}

		private void UpdateCurrentStream()
		{
			m_currentBegin = 0;
			m_currentEnd = 0;
			for (int i = 0; i < m_streams.Count; i++)
			{
				m_streamIndex = i;
				m_currentStream = m_streams[m_streamIndex];
				m_currentEnd = m_currentBegin + m_currentStream.Length;
				if (m_currentEnd > m_position)
				{
					m_currentStream.Position = m_position - m_currentBegin;
					return;
				}

				m_currentBegin += m_currentStream.Length;
			}
			m_currentBegin -= m_currentStream.Length;
			m_currentStream.Position = m_position - m_currentBegin;
		}

		public override long Position
		{
			get => m_position;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}

				m_position = value;
				if (value < m_currentBegin || value >= m_currentEnd)
				{
					UpdateCurrentStream();
				}
				else
				{
					m_currentStream.Position = value - m_currentBegin;
				}
			}
		}

		public override long Length { get; }

		public override bool CanRead { get; }
		public override bool CanWrite { get; }
		public override bool CanSeek => true;

		private static readonly Regex s_splitCheck = new Regex($@".+{MultifileRegPostfix}[0-9]+$", RegexOptions.Compiled);
		private static readonly SplitNameComparer s_splitNameComparer = new SplitNameComparer();

		public const string MultifileRegPostfix = @"\.split";

		private readonly IReadOnlyList<Stream> m_streams;

		private Stream m_currentStream;
		private int m_streamIndex;
		private long m_position;
		private long m_currentBegin;
		private long m_currentEnd;
	}
}
