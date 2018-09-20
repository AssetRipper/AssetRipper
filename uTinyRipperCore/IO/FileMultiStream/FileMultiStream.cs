using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace uTinyRipper
{
	public sealed class FileMultiStream : Stream
	{
		public FileMultiStream(IReadOnlyList<Stream> streams)
		{
			if (streams == null || streams.Count == 0)
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
					throw new Exception($"Stream {stream} can't seek");
				}
			}

			m_streams = streams;
			Length = streams.Sum(t => t.Length);
			UpdateCurrentStream();
		}

		~FileMultiStream()
		{
			Dispose(false);
		}

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
			if (FileUtils.Exists(path))
			{
				return true;
			}

			{
				SplitPath(path, out string directory, out string file);
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
			if (FileUtils.Exists(path))
			{
				return FileUtils.OpenRead(path);
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
				SplitPathWithoutExtension(path, out string directory, out string file);
				return Path.Combine(directory, file);
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
			
			if (FileUtils.Exists(path))
			{
				return new [] { path };
			}
			return new string[0];
		}

		public static bool IsNameEquals(string fileName, string compare)
		{
			fileName = GetFileName(fileName);
			return fileName == compare;
		}

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
			if (!DirectoryUtils.Exists(dirPath))
			{
				return new string[0];
			}

			string filePatern = fileName + ".split*";
			return DirectoryUtils.GetFiles(dirPath, filePatern);
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
					Stream stream = FileUtils.OpenRead(splitFiles[i]);
					streams[i] = stream;
				}

				return new FileMultiStream(streams);
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
		
		private static void SplitPath(string path, out string directory, out string file)
		{
			directory = Path.GetDirectoryName(path);
			directory = string.IsNullOrEmpty(directory) ? "." : directory;
			file = Path.GetFileName(path);
			if (string.IsNullOrEmpty(file))
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
			if (m_currentStream != null)
			{
				m_currentStream.Flush();
			}
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
			m_position++;
			if (m_position == m_currentEnd)
			{
				UpdateCurrentStream();
			}
			return value;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int totalRead = 0;
			while (true)
			{
				int curCount = count;
				if (curCount > m_currentEnd - m_position)
				{
					curCount = unchecked((int)(m_currentEnd - m_position));
				}
				int read = m_currentStream.Read(buffer, offset, curCount);
				totalRead += read;
				m_position += read;
				if (m_position == m_currentEnd)
				{
					bool isUpdated = UpdateCurrentStream();
					if (!isUpdated)
					{
						return totalRead;
					}
				}

				if (read != curCount)
				{
					return totalRead;
				}

				offset += curCount;
				count -= curCount;
				if (count == 0)
				{
					return totalRead;
				}
			}
		}

		public override void WriteByte(byte value)
		{
			m_currentStream.WriteByte(value);
			m_position++;
			if (m_position == m_currentEnd)
			{
				UpdateCurrentStream();
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			while (true)
			{
				int curCount = count;
				if (curCount > m_currentEnd - m_position)
				{
					curCount = unchecked((int)(m_currentEnd - m_position));
				}
				m_currentStream.Write(buffer, offset, curCount);
				m_position += curCount;
				if (m_position == m_currentEnd)
				{
					UpdateCurrentStream();
				}

				offset += curCount;
				count -= curCount;
				if (count == 0)
				{
					return;
				}
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

		private bool UpdateCurrentStream()
		{
			bool result = false;
			m_currentBegin = 0;
			for(int i = 0; i < m_streams.Count; i++)
			{
				m_currentStream = m_streams[i];
				m_currentEnd = m_currentBegin + m_currentStream.Length;
				if (m_currentBegin <= m_position && m_currentEnd > m_position)
				{
					m_currentStream.Position = m_position - m_currentBegin;
					result = true;
					break;
				}

				m_currentBegin += m_currentStream.Length;
			}
			m_currentEnd = m_currentBegin + m_currentStream.Length;
			m_currentStream.Position = m_position - m_currentBegin;
			return result;
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
				if (value < m_currentBegin || value > m_currentEnd)
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

		public override bool CanRead => m_streams.All(t => t.CanRead);
		public override bool CanWrite => m_streams.All(t => t.CanWrite);
		public override bool CanSeek => true;
		
		private static readonly Regex s_splitCheck = new Regex($@".+{MultifileRegex}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		private static readonly SplitNameComparer s_splitNameComparer = new SplitNameComparer();

		public const string MultifileRegPostfix = @"\.split";
		public const string MultifileRegex = @"\.split[0-9]+";

		private readonly IReadOnlyList<Stream> m_streams;

		private Stream m_currentStream;
		private long m_position;
		private long m_currentBegin;
		private long m_currentEnd;
	}
}
