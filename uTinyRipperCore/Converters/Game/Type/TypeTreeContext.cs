using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Converters
{
	public delegate void TypeTreeGenerator(TypeTreeContext context, string name);

	public sealed class TypeTreeContext
	{
		private readonly struct HierarchyData
		{
			public HierarchyData(int index, int size)
			{
				Index = index;
				Size = size;
			}

			public int Index { get; }
			public int Size { get; }
		}

		public TypeTreeContext(Version version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;

			DefaultStringFlag = AlignStrings(version) ? TransferMetaFlags.AlignBytesFlag : TransferMetaFlags.NoTransferFlags;
			DefaultArrayFlag = AlignArrays(version) ? TransferMetaFlags.AlignBytesFlag : TransferMetaFlags.NoTransferFlags;
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool AlignStrings(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool AlignArrays(Version version) => version.IsGreaterEqual(2017);

		public void AddNode(string type, string name)
		{
			AddNode(type, name, 0);
		}

		public void AddNode(string type, string name, int size)
		{
			AddNode(type, name, size, 1);
		}

		public void AddNode(string type, string name, int size, int version)
		{
			AddNode(type, name, size, version, TransferMetaFlags.NoTransferFlags);
		}

		public void AddNode(string type, string name, int size, int version, TransferMetaFlags flags)
		{
			AddNode(type, name, size, version, false, TransferMetaFlags.NoTransferFlags);
		}

		private void AddNode(string type, string name, int size, int version, bool isArray, TransferMetaFlags flags)
		{
			TypeTreeNode node = new TypeTreeNode();
			node.Version = version;
			node.Depth = (byte)Depth;
			node.ByteSize = size;
			node.Type = type;
			node.Name = name;
			node.Index = Index++;
			node.IsArrayBool = isArray;
			node.MetaFlag = flags;
			m_nodes.Add(node);

			if (Size >= 0)
			{
				if (size >= 0)
				{
					Size += size;
				}
				else
				{
					Size = -1;
				}
			}
		}

		public void AddPrimitive(string type, string name)
		{
			switch (type)
			{
				case MonoUtils.BooleanName:
				case MonoUtils.BoolName:
					AddBool(name);
					break;
				case MonoUtils.SByteName:
				case MonoUtils.CSByteName:
					AddSByte(name);
					break;
				case MonoUtils.ByteName:
				case MonoUtils.CByteName:
					AddByte(name);
					break;
				case MonoUtils.CharName:
				case MonoUtils.CCharName:
					AddUInt16(name);
					break;
				case MonoUtils.Int16Name:
				case MonoUtils.ShortName:
					AddInt16(name);
					break;
				case MonoUtils.UInt16Name:
				case MonoUtils.UShortName:
					AddUInt16(name);
					break;
				case MonoUtils.Int32Name:
				case MonoUtils.IntName:
					AddInt32(name);
					break;
				case MonoUtils.UInt32Name:
				case MonoUtils.UIntName:
					AddUInt32(name);
					break;
				case MonoUtils.Int64Name:
				case MonoUtils.LongName:
					AddInt64(name);
					break;
				case MonoUtils.UInt64Name:
				case MonoUtils.ULongName:
					AddUInt64(name);
					break;
				case MonoUtils.SingleName:
				case MonoUtils.FloatName:
					AddSingle(name);
					break;
				case MonoUtils.DoubleName:
				case MonoUtils.CDoubleName:
					AddDouble(name);
					break;

				default:
					throw new Exception("Unknown primitive type " + type);
			}
		}

		public void AddBool(string name)
		{
			AddBool(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddBool(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.BoolName, name, sizeof(bool), 1, flags);
		}

		public void AddSByte(string name)
		{
			AddSByte(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddSByte(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.SInt8Name, name, sizeof(sbyte), 1, flags);
		}

		public void AddByte(string name)
		{
			AddByte(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddByte(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.UInt8Name, name, sizeof(byte), 1, flags);
		}

		public void AddInt16(string name)
		{
			AddInt16(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddInt16(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.SInt16Name, name, sizeof(short), 1, flags);
		}

		public void AddUInt16(string name)
		{
			AddUInt16(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddUInt16(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.UInt16Name, name, sizeof(ushort), 1, flags);
		}

		public void AddInt32(string name)
		{
			AddInt32(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddInt32(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.IntName, name, sizeof(int), 1, flags);
		}

		public void AddUInt32(string name)
		{
			AddUInt32(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddUInt32(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.UnsignedIntName, name, sizeof(uint), 1, flags);
		}

		public void AddInt64(string name)
		{
			AddInt64(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddInt64(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.SInt64Name, name, sizeof(long), 1, flags);
		}

		public void AddUInt64(string name)
		{
			AddUInt64(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddUInt64(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.UInt64Name, name, sizeof(ulong), 1, flags);
		}

		public void AddSingle(string name)
		{
			AddSingle(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddSingle(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.FloatName, name, sizeof(float), 1, flags);
		}

		public void AddDouble(string name)
		{
			AddDouble(name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddDouble(string name, TransferMetaFlags flags)
		{
			AddNode(TypeTreeUtils.DoubleName, name, sizeof(double), 1, flags);
		}

		public void AddString(string name)
		{
			BeginArray(TypeTreeUtils.StringName, name, DefaultStringFlag);
			AddNode(TypeTreeUtils.CharName, TypeTreeUtils.DataName, sizeof(byte));
			EndArray();
		}

		public void AddPPtr<T>(string name)
		{
			AddPPtr(typeof(T).Name, name);
		}

		public void AddPPtr<T>(string name, TransferMetaFlags flags)
		{
			AddPPtr(typeof(T).Name, name, flags);
		}

		public void AddPPtr(string type, string name)
		{
			AddPPtr(type, name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddPPtr(string type, string name, TransferMetaFlags flags)
		{
			AddNode($"PPtr<{type}>", name, 0, 1, flags);
			BeginChildren();
			AddInt32(PPtr<Object>.FileIDName);
			if (PPtr<Object>.IsLongID(Version))
			{
				AddInt64(PPtr<Object>.PathIDName);
			}
			else
			{
				AddInt32(PPtr<Object>.PathIDName);
			}
			EndChildren();
		}

		public void AddVector(string name, TypeTreeGenerator generator)
		{
			AddVector(name, TransferMetaFlags.NoTransferFlags, generator);
		}

		public void AddVector(string name, TransferMetaFlags flags, TypeTreeGenerator generator)
		{
			BeginVector(name, flags);
			generator.Invoke(this, TypeTreeUtils.DataName);
			EndVector();
		}

		public void BeginVector(string name)
		{
			BeginVector(name, TransferMetaFlags.NoTransferFlags);
		}

		public void BeginVector(string name, TransferMetaFlags flags)
		{
			BeginArray(TypeTreeUtils.VectorName, name, flags);
		}

		public void EndVector()
		{
			EndArray();
		}

		private void BeginArray(string type, string name, TransferMetaFlags flags)
		{
			AddNode(type, name, -1);
			BeginChildren();
			AddNode(TypeTreeUtils.ArrayName, TypeTreeUtils.ArrayName, -1, 1, true, DefaultArrayFlag | flags);
			BeginChildren();
			AddInt32(TypeTreeUtils.SizeName);
		}

		private void EndArray()
		{
			EndChildren();
			EndChildren();
		}

		public void BeginChildren()
		{
			Depth++;
			m_hierarchy.Push(new HierarchyData(Index - 1, Size));
			Size = 0;
		}

		public void EndChildren()
		{
			int size = Size;
			HierarchyData hierarchy = m_hierarchy.Pop();
			int parentSize = Nodes[hierarchy.Index].ByteSize;
			if (parentSize >= 0)
			{
				Nodes[hierarchy.Index].ByteSize = size;
			}
			Size = hierarchy.Size;
			if (Size >= 0)
			{
				Size += size;
			}
			Depth--;
		}

		public Version Version { get; }
		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }
		
		public IReadOnlyList<TypeTreeNode> Nodes => m_nodes;
		private int Depth { get; set; }
		private int Index { get; set; }
		private int Size { get; set; }

		private TransferMetaFlags DefaultStringFlag { get; }
		private TransferMetaFlags DefaultArrayFlag { get; }

		private readonly List<TypeTreeNode> m_nodes = new List<TypeTreeNode>();
		private readonly Stack<HierarchyData> m_hierarchy = new Stack<HierarchyData>();
	}
}
