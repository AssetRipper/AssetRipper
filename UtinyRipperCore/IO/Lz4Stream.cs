using System;
using System.IO;

namespace UtinyRipper
{
	public class Lz4Stream : Stream
	{
		private enum DecodePhase
		{
			ReadToken,
			ReadExLiteralLength,
			CopyLiteral,
			ReadOffset,
			ReadExMatchLength,
			CopyMatch,

			Finish,
		}

		public Lz4Stream(byte[] buffer, long offset, long length)
		{
			if (buffer == null || buffer.Length == 0)
			{
				throw new ArgumentNullException(nameof(buffer));
			}
			if (offset < 0)
			{
				throw new ArgumentException($"Invalid offset value {offset}", nameof(offset));
			}
			if (length <= 0)
			{
				throw new ArgumentException($"Invalid length value {length}", nameof(length));
			}

			m_baseStream = new MemoryStream(buffer);
			m_inputLength = length;
			m_phase = DecodePhase.ReadToken;
		}

		/// <summary>
		/// Whole base stream is compressed data
		/// </summary>
		/// <param name="baseStream">Stream with compressed data</param>
		public Lz4Stream(Stream baseStream) :
			this(baseStream, baseStream.Length - baseStream.Position)
		{
		}

		/// <summary>
		/// Part of base stream is compressed data
		/// </summary>
		/// <param name="baseStream">Stream with compressed data</param>
		/// <param name="compressedSize">Amount of comprassed data</param>
		public Lz4Stream(Stream baseStream, long compressedSize)
		{
			if(baseStream == null)
			{
				throw new ArgumentNullException(nameof(baseStream));
			}
			if(compressedSize <= 0)
			{
				throw new ArgumentException($"Compress length {compressedSize} must be greater then 0");
			}

			m_baseStream = baseStream;
			m_inputLength = compressedSize;
			m_phase = DecodePhase.ReadToken;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Position = offset;
				return unchecked((int)Read(stream, count));
			}
		}
		
		public long Read(Stream stream, long count)
		{
			if(stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if(count <= 0)
			{
				throw new ArgumentException(nameof(count));
			}
			
			long readLeft = count;
			bool processing = true;
			while (processing)
			{
				switch (m_phase)
				{
					case DecodePhase.ReadToken:
						ReadToken();
						break;

					case DecodePhase.ReadExLiteralLength:
						ReadExLiteralLength();
						break;

					case DecodePhase.CopyLiteral:
						CopyLiteral(stream, ref readLeft);
						break;

					case DecodePhase.ReadOffset:
						ReadOffset();
						break;

					case DecodePhase.ReadExMatchLength:
						ReadExMatchLength();
						break;

					case DecodePhase.CopyMatch:
						CopyMatch(stream, ref readLeft, count);
						break;

					case DecodePhase.Finish:
						Finish(stream, readLeft, count);
						processing = false;
						break;

					default:
						throw new Exception($"Unknonw decode phase {m_phase}");
				}
			}
			
			return count - readLeft;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
		
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			m_baseStream.Dispose();
		}

		private void ReadToken()
		{
			int token = ReadInputByte();

			m_literalLength = token >> 4;
			m_matchLength = (token & 0xF) + 4;

			switch(m_literalLength)
			{
				case 0:
					m_phase = DecodePhase.ReadOffset;
					break;

				case 0xF:
					m_phase = DecodePhase.ReadExLiteralLength;
					break;

				default:
					m_phase = DecodePhase.CopyLiteral;
					break;
			}
		}

		private void ReadExLiteralLength()
		{
			int exLiteralLength = ReadInputByte();
			m_literalLength += exLiteralLength;
			if(exLiteralLength == byte.MaxValue)
			{
				m_phase = DecodePhase.ReadExLiteralLength;
			}
			else
			{
				m_phase = DecodePhase.CopyLiteral;
			}
		}

		private void CopyLiteral(Stream stream, ref long readLeft)
		{
			int readCount = m_literalLength < readLeft ? m_literalLength : unchecked((int)readLeft);
			if(readCount != 0)
			{
				int read = ReadInput(stream, readCount);

				readLeft -= read;

				m_literalLength -= read;
				if(m_literalLength != 0)
				{
					m_phase = DecodePhase.CopyLiteral;
					return;
				}
			}

			if (readLeft == 0)
			{
				m_phase = DecodePhase.Finish;
			}
			else
			{
				m_phase = DecodePhase.ReadOffset;
			}
		}

		private void ReadOffset()
		{
			m_matchDestination = ReadInputInt16();
			if(m_matchLength == 15 + 4)
			{
				m_phase = DecodePhase.ReadExMatchLength;
			}
			else
			{
				m_phase = DecodePhase.CopyMatch;
			}
		}

		private void ReadExMatchLength()
		{
			int exMatchLength = ReadInputByte();
			m_matchLength += exMatchLength;
			if(exMatchLength == byte.MaxValue)
			{
				m_phase = DecodePhase.ReadExMatchLength;
			}
			else
			{
				m_phase = DecodePhase.CopyMatch;
			}
		}

		private void CopyMatch(Stream stream, ref long readLeft, long count)
		{
			int readCount = m_matchLength < readLeft ? m_matchLength : unchecked((int)readLeft);
			if (readCount != 0)
			{
				long read = count - readLeft;
				long decodeCount = m_matchDestination - read;
				if(decodeCount > 0)
				{
					//offset is fairly far back, we need to pull from the buffer
					int source = m_decodeBufferPos - unchecked((int)decodeCount);
					if(source < 0)
					{
						source += DecodeBufferLength;
					}
					int destCount = decodeCount < readCount ? unchecked((int)decodeCount) : readCount;
					for(int i = 0; i < destCount; i++)
					{
						stream.WriteByte(m_decodeBuffer[source & DecodeBufferMask]);
						source++;
					}
				}
				else
				{
					decodeCount = 0;
				}
				
				long srcPosition = stream.Position - m_matchDestination;
				long destPosition = stream.Position;
				for(int i = unchecked((int)decodeCount); i < readCount; i ++)
				{
					stream.Position = srcPosition;
					byte matchValue = (byte)stream.ReadByte();
					stream.Position = destPosition;
					stream.WriteByte(matchValue);
					srcPosition++;
					destPosition++;
				}

				readLeft -= readCount;
				m_matchLength -= readCount;
			}

			if (readLeft == 0)
			{
				m_phase = DecodePhase.Finish;
			}
			else
			{
				m_phase = DecodePhase.ReadToken;
			}
		}

		private void Finish(Stream stream, long readLeft, long count)
		{
			long read = count - readLeft;
			int toBuffer = read < DecodeBufferLength ? unchecked((int)read) : DecodeBufferLength;

			stream.Position -= toBuffer;
			if(toBuffer == DecodeBufferLength)
			{
				stream.Read(m_decodeBuffer, 0, DecodeBufferLength);
				m_decodeBufferPos = 0;
			}
			else
			{
				int decodePosition = m_decodeBufferPos;
				for(int i = 0; i < toBuffer; i++)
				{
					m_decodeBuffer[decodePosition & DecodeBufferMask] = (byte)stream.ReadByte();
					decodePosition++;
				}
				stream.Position -= toBuffer;

				m_decodeBufferPos = decodePosition & DecodeBufferMask;
			}
		}

		private int ReadInputByte()
		{
			if (m_inputBufferPosition == m_inputBufferEnd)
			{
				FillInputBuffer();
			}

			return m_inputBuffer[m_inputBufferPosition++];
		}

		private int ReadInputInt16()
		{
			if (m_inputBufferPosition == m_inputBufferEnd)
			{
				FillInputBuffer();
			}

			if(m_inputBufferEnd - m_inputBufferPosition == 1)
			{
				// read last byte and refill
				m_inputBuffer[0] = m_inputBuffer[m_inputBufferPosition];
				FillInputBuffer(1);
			}

			int ret = m_inputBuffer[m_inputBufferPosition++];
			ret |= m_inputBuffer[m_inputBufferPosition++] << 8;
			return ret;
		}

		private int ReadInput(Stream stream, int count)
		{
			int readLeft = count;
			int read = FillBuffer(stream, count);
			readLeft -= read;

			if (readLeft != 0)
			{
				if (readLeft >= InputChunkSize)
				{
					int readCount = readLeft < m_inputLength ? readLeft : unchecked((int)m_inputLength);
					m_baseStream.CopyStream(stream, readCount);
					readLeft -= readCount;
					m_inputLength -= readCount;
				}
				else
				{
					FillInputBuffer();
					read = FillBuffer(stream, readLeft);
					readLeft -= read;
				}
			}

			return count - readLeft;
		}

		private void FillInputBuffer(int offset = 0)
		{
			int count = InputChunkSize < m_inputLength ? InputChunkSize : unchecked((int)m_inputLength);
			count -= offset;
			int read = m_baseStream.Read(m_inputBuffer, offset, count);
			if (read == 0)
			{
#warning replace this place to m_phase = Finish for partial reading
				throw new Exception("No data left");
			}
			if(read != count)
			{
				throw new Exception("Unable to read enough data");
			}

			m_inputLength -= read;

			m_inputBufferPosition = 0;
			m_inputBufferEnd = read + offset;
		}

		private int FillBuffer(Stream stream, int count)
		{
			int inputBufferLength = m_inputBufferEnd - m_inputBufferPosition;
			int inputLeft = count < inputBufferLength ? count : inputBufferLength;
			stream.Write(m_inputBuffer, m_inputBufferPosition, inputLeft);
			m_inputBufferPosition += inputLeft;
			return inputLeft;
		}

		public override bool CanSeek => false;
		public override bool CanRead => true;
		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException();
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		private const int InputChunkSize = 128;
		private const int DecodeBufferLength = 0x10000;
		private const int DecodeBufferMask = 0xFFFF;

		private readonly byte[] m_inputBuffer = new byte[InputChunkSize];
		private readonly byte[] m_decodeBuffer = new byte[DecodeBufferLength];

		private readonly Stream m_baseStream;
		
		private DecodePhase m_phase;
		private long m_inputLength = 0;
		private int m_inputBufferPosition = 0;
		private int m_inputBufferEnd = 0;
		private int m_decodeBufferPos;

		//state within interruptable phases and across phase boundaries is
		//kept here - again, so that we can punt out and restart freely
		private int m_literalLength;
		private int m_matchLength;
		private int m_matchDestination;
	}
}
