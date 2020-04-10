/*
Original work:
LZ4 Library
Copyright (c) 2011-2016, Yann Collet
All rights reserved.

BSD 2-Clause License

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


Modified work:
MIT License

Copyright (c) 2020 mafaca

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

//#define STREAMING_INPUT

using System;
using System.IO;

namespace uTinyRipper.Lz4
{
	public class Lz4DecodeStream : Stream
	{
		private enum DecodePhase
		{
			ReadToken,
			ReadExLiteralLength,
			CopyLiteral,
			ReadMatch,
			ReadExMatchLength,
			CopyMatch,

			Finish,
		}

		public Lz4DecodeStream(byte[] buffer, int offset, int length) :
			this(new MemoryStream(buffer, offset, length), length, false)
		{
		}

		/// <summary>
		/// Whole base stream is compressed data
		/// </summary>
		/// <param name="baseStream">Stream with compressed data</param>
		public Lz4DecodeStream(Stream baseStream, bool leaveOpen = true) :
			this(baseStream, baseStream.Length, leaveOpen)
		{
		}

		/// <summary>
		/// Part of base stream is compressed data
		/// </summary>
		/// <param name="baseStream">Stream with compressed data</param>
		/// <param name="compressedSize">Amount of comprassed data</param>
		public Lz4DecodeStream(Stream baseStream, long compressedSize, bool leaveOpen = true)
		{
			if (compressedSize <= 0)
			{
				throw new ArgumentException($"Compressed size {compressedSize} must be greater then 0");
			}

			m_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			Length = compressedSize;
			m_inputLeft = compressedSize;
			m_phase = DecodePhase.ReadToken;
			m_leaveOpen = leaveOpen;
		}

		~Lz4DecodeStream()
		{
			Dispose(false);
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
			using (MemoryStream stream = new MemoryStream(buffer, offset, count))
			{
				return (int)Read(stream, count);
			}
		}

		public long Read(Stream stream, long count)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (count <= 0)
			{
				throw new ArgumentException(nameof(count));
			}

			long readLeft = count;
			while (true)
			{
				switch (m_phase)
				{
					case DecodePhase.ReadToken:
						{
							int token = ReadInputByte();

							m_literalLength = token >> 4;
							m_matchLength = (token & 0xF) + 4;
							if (m_literalLength == 0)
							{
								goto case DecodePhase.ReadMatch;
							}
							if (m_literalLength == 0xF)
							{
								goto case DecodePhase.ReadExLiteralLength;
							}
							goto case DecodePhase.CopyLiteral;
						}

					case DecodePhase.ReadExLiteralLength:
						{
							int exLiteralLength;
							do
							{
								exLiteralLength = ReadInputByte();
								m_literalLength += exLiteralLength;
							} while (exLiteralLength == byte.MaxValue);
							goto case DecodePhase.CopyLiteral;
						}

					case DecodePhase.CopyLiteral:
						{
							if (m_literalLength >= readLeft)
							{
								Write(stream, (int)readLeft);
								m_literalLength -= (int)readLeft;
								readLeft = 0;
								m_phase = DecodePhase.CopyLiteral;
								goto case DecodePhase.Finish;
							}

							Write(stream, m_literalLength);
							readLeft -= m_literalLength;
							goto case DecodePhase.ReadMatch;
						}

					case DecodePhase.ReadMatch:
						{
							m_matchDestination = ReadInputInt16();
							if (m_matchLength == 0xF + 4)
							{
								goto case DecodePhase.ReadExMatchLength;
							}
							goto case DecodePhase.CopyMatch;
						}

					case DecodePhase.ReadExMatchLength:
						{
							int exMatchLength;
							do
							{
								exMatchLength = ReadInputByte();
								m_matchLength += exMatchLength;
							} while (exMatchLength == byte.MaxValue);
							goto case DecodePhase.CopyMatch;
						}

					case DecodePhase.CopyMatch:
						{
							int toCopyTotal = m_matchLength < readLeft ? m_matchLength : (int)readLeft;
							while (toCopyTotal > 0)
							{
								int srcPosition = (m_decodeBufferPosition - m_matchDestination) & DecodeBufferMask;
								int srcAvailable = DecodeBufferCapacity - srcPosition;
								int destAvailable = DecodeBufferCapacity - m_decodeBufferPosition;
								int available = srcAvailable < destAvailable ? srcAvailable : destAvailable;
								int toCopy = toCopyTotal < available ? toCopyTotal : available;
								int delta = m_decodeBufferPosition - srcPosition;
								if (delta > 0 && delta < toCopy)
								{
									for (int i = 0; i < toCopy; i++)
									{
										m_decodeBuffer[m_decodeBufferPosition++] = m_decodeBuffer[srcPosition++];
									}
								}
								else
								{
									Buffer.BlockCopy(m_decodeBuffer, srcPosition, m_decodeBuffer, m_decodeBufferPosition, toCopy);
									m_decodeBufferPosition += toCopy;
								}

								toCopyTotal -= toCopy;
								m_matchLength -= toCopy;
								readLeft -= toCopy;

								if (m_decodeBufferPosition == DecodeBufferCapacity)
								{
									FillOutputStream(stream);
								}
							}

							if (readLeft == 0)
							{
								m_phase = DecodePhase.CopyMatch;
								goto case DecodePhase.Finish;
							}
							goto case DecodePhase.ReadToken;
						}

					case DecodePhase.Finish:
						{
							FillOutputStream(stream);
							return count - readLeft;
						}

					default:
						throw new Exception($"Unknonw decode phase {m_phase}");
				}
			}
		}

		public void ReadBuffer(byte[] buffer, int offset, int count)
		{
			using (MemoryStream stream = new MemoryStream(buffer, offset, count))
			{
				ReadBuffer(stream, count);
			}
		}

		public void ReadBuffer(Stream stream, long count)
		{
			int read = (int)Read(stream, count);
			if (read != count)
			{
				throw new Exception($"Unexpected end of input stream. Read {read} but expected {count}");
			}
			if (IsDataLeft)
			{
				throw new Exception($"Some data left");
			}
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
			if (!m_leaveOpen)
			{
				m_baseStream.Dispose();
			}
			base.Dispose(disposing);
		}

		// =====================================
		// Buffer processing
		// =====================================

		private int ReadInputByte()
		{
			if (m_inputBufferPosition == InputBufferCapacity)
			{
				FillInputBuffer();
			}

			return m_inputBuffer[m_inputBufferPosition++];
		}

		private int ReadInputInt16()
		{
			int available = InputBufferCapacity - m_inputBufferPosition;
			if (available == 0)
			{
				FillInputBuffer();
			}
			else if (available == 1)
			{
				m_inputBuffer[0] = m_inputBuffer[m_inputBufferPosition];
				FillInputBuffer(1);
			}

			int ret = m_inputBuffer[m_inputBufferPosition++];
			ret |= m_inputBuffer[m_inputBufferPosition++] << 8;
			return ret;
		}

		private void Write(Stream stream, int count)
		{
			while (count > 0)
			{
				if (m_inputBufferPosition == InputBufferCapacity)
				{
					FillInputBuffer();
				}

				int srcAvailable = InputBufferCapacity - m_inputBufferPosition;
				int destAvailable = DecodeBufferCapacity - m_decodeBufferPosition;
				int available = srcAvailable < destAvailable ? srcAvailable : destAvailable;
				int toWrite = count < available ? count : available;
				Buffer.BlockCopy(m_inputBuffer, m_inputBufferPosition, m_decodeBuffer, m_decodeBufferPosition, toWrite);
				count -= toWrite;
				m_inputBufferPosition += toWrite;
				m_decodeBufferPosition += toWrite;

				if (m_decodeBufferPosition == DecodeBufferCapacity)
				{
					FillOutputStream(stream);
				}
			}
		}

		private void FillInputBuffer(int offset = 0)
		{
			int available = InputBufferCapacity - offset;
			int count = available < m_inputLeft ? available : (int)m_inputLeft;

			m_inputBufferPosition = 0;
			while (count > 0)
			{
				int read = m_baseStream.Read(m_inputBuffer, offset, count);
				if (read == 0)
				{
#if STREAMING_INPUT
#error TODO: set processing to false and go to finish
#else
					throw new Exception("No data left");
#endif
				}
				m_position += read;
				offset += read;
				count -= read;
				m_inputLeft -= read;
			}
		}

		private void FillOutputStream(Stream stream)
		{
			int toWriteTotal = m_decodeBufferPosition - m_decodeBufferStart;
			int toEnd = DecodeBufferCapacity - m_decodeBufferStart;
			int toWrite = toEnd < toWriteTotal ? toEnd : toWriteTotal;
			stream.Write(m_decodeBuffer, m_decodeBufferStart, toWrite);
			stream.Write(m_decodeBuffer, 0, toWriteTotal - toWrite);
			m_decodeBufferPosition = m_decodeBufferPosition & DecodeBufferMask;
			m_decodeBufferStart = m_decodeBufferPosition;
		}

		public bool IsDataLeft => m_phase == DecodePhase.CopyLiteral ? (m_literalLength != 0) : (m_matchLength != 0);

		public override bool CanSeek => false;
		public override bool CanRead => true;
		public override bool CanWrite => false;

		public override long Length { get; }
		public override long Position
		{
			get => m_position;
			set => throw new NotSupportedException();
		}

		private const int InputBufferCapacity = 4096;
		private const int DecodeBufferCapacity = 0x10000;
		private const int DecodeBufferMask = 0xFFFF;

		private readonly byte[] m_inputBuffer = new byte[InputBufferCapacity];
		private readonly byte[] m_decodeBuffer = new byte[DecodeBufferCapacity];

		private readonly Stream m_baseStream;
		private readonly bool m_leaveOpen;

		private long m_position = 0;
		private long m_inputLeft = 0;
		private int m_inputBufferPosition = InputBufferCapacity;
		private int m_decodeBufferPosition = 0;

		/// <summary>
		/// State within interruptable phases and across phase boundaries is kept here - again, so that we can punt out and restart freely
		/// </summary>
		private DecodePhase m_phase;
		private int m_literalLength;
		private int m_matchLength;
		private int m_matchDestination;
		private int m_decodeBufferStart;
	}
}
