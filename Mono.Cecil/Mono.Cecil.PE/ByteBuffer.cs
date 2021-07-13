//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace Mono.Cecil.PE {

	class ByteBuffer {

		internal byte [] buffer;
		internal int length;
		internal int position;

		public ByteBuffer ()
		{
			this.buffer = Empty<byte>.Array;
		}

		public ByteBuffer (int length)
		{
			this.buffer = new byte [length];
		}

		public ByteBuffer (byte [] buffer)
		{
			this.buffer = buffer ?? Empty<byte>.Array;
			this.length = this.buffer.Length;
		}

		public void Advance (int length)
		{
			position += length;
		}

		public byte ReadByte ()
		{
			return buffer [position++];
		}

		public sbyte ReadSByte ()
		{
			return (sbyte) ReadByte ();
		}

		public byte [] ReadBytes (int length)
		{
			var bytes = new byte [length];
			Buffer.BlockCopy (buffer, position, bytes, 0, length);
			position += length;
			return bytes;
		}

		public ushort ReadUInt16 ()
		{
			ushort value = (ushort) (buffer [position]
				| (buffer [position + 1] << 8));
			position += 2;
			return value;
		}

		public short ReadInt16 ()
		{
			return unchecked((short) ReadUInt16 ());
		}

		public uint ReadUInt32 ()
		{
			uint value = unchecked((uint) (buffer [position]
				| (buffer [position + 1] << 8)
				| (buffer [position + 2] << 16)
				| (buffer [position + 3] << 24)));
			position += 4;
			return value;
		}

		public int ReadInt32 ()
		{
			return unchecked((int) ReadUInt32 ());
		}

		public ulong ReadUInt64 ()
		{
			uint low = ReadUInt32 ();
			uint high = ReadUInt32 ();

			return (((ulong) high) << 32) | low;
		}

		public long ReadInt64 ()
		{
			return unchecked((long) ReadUInt64 ());
		}

		public uint ReadCompressedUInt32 ()
		{
			byte first = ReadByte ();
			if ((first & 0x80) == 0)
				return first;

			if ((first & 0x40) == 0)
				return ((uint) (first & ~0x80) << 8)
					| ReadByte ();

			return ((uint) (first & ~0xc0) << 24)
				| (uint) ReadByte () << 16
				| (uint) ReadByte () << 8
				| ReadByte ();
		}

		public int ReadCompressedInt32 ()
		{
			var b = buffer [position];
			var u = (int) ReadCompressedUInt32 ();
			var v = u >> 1;
			if ((u & 1) == 0)
				return v;

			switch (b & 0xc0)
			{
				case 0:
				case 0x40:
					return v - 0x40;
				case 0x80:
					return v - 0x2000;
				default:
					return v - 0x10000000;
			}
		}

		public float ReadSingle ()
		{
			if (!BitConverter.IsLittleEndian) {
				var bytes = ReadBytes (4);
				Array.Reverse (bytes);
				return BitConverter.ToSingle (bytes, 0);
			}

			float value = BitConverter.ToSingle (buffer, position);
			position += 4;
			return value;
		}

		public double ReadDouble ()
		{
			if (!BitConverter.IsLittleEndian) {
				var bytes = ReadBytes (8);
				Array.Reverse (bytes);
				return BitConverter.ToDouble (bytes, 0);
			}

			double value = BitConverter.ToDouble (buffer, position);
			position += 8;
			return value;
		}
	}
}
