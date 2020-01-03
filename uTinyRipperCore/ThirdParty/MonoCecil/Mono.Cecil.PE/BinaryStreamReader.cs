//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System.IO;

namespace Mono.Cecil.PE
{

	class BinaryStreamReader : BinaryReader {

		public int BasePosition { get; }

		public int Position {
			get { return (int) BaseStream.Position - BasePosition; }
			set { BaseStream.Position = BasePosition + value; }
		}

		public int Length {
			get { return (int) BaseStream.Length - BasePosition; }
		}

		public BinaryStreamReader (Stream stream)
			: base (stream)
		{
			BasePosition = Position;
		}

		public void Advance (int bytes)
		{
			BaseStream.Seek (bytes, SeekOrigin.Current);
		}

		public void MoveTo (uint position)
		{
			BaseStream.Seek (BasePosition + position, SeekOrigin.Begin);
		}

		public void Align (int align)
		{
			int shift = BasePosition % align;
			align--;
			var position = (int)BaseStream.Position - shift;
			Advance (((position + align) & ~align) - position + shift);
		}

		public DataDirectory ReadDataDirectory ()
		{
			return new DataDirectory (ReadUInt32 (), ReadUInt32 ());
		}
	}
}
