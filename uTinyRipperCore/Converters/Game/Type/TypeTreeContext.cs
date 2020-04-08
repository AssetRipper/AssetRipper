using System;
using System.Collections.Generic;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public delegate void TypeTreeGenerator(TypeTreeContext context, string name);
	public delegate void TypeTreeExGenerator(TypeTreeContext context, string type, string name);
	public delegate void TypeTreeTGenerator(TypeTreeContext context, string name, TypeTreeGenerator type);
	public delegate void TypeTreeT2Generator(TypeTreeContext context, string name, TypeTreeGenerator firstType, TypeTreeGenerator secondType);

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

		public TypeTreeContext(AssetLayout layout)
		{
			Layout = layout;
			DefaultStringFlag = Layout.IsAlign ? TransferMetaFlags.AlignBytesFlag : TransferMetaFlags.NoTransferFlags;
			DefaultArrayFlag = Layout.IsAlignArrays ? TransferMetaFlags.AlignBytesFlag : TransferMetaFlags.NoTransferFlags;
		}

		public void AddNode(string type, string name)
		{
			AddNode(type, name, 1);
		}

		public void AddNode(string type, string name, int version)
		{
			AddNode(type, name, version, 0);
		}

		public void AddNode(string type, string name, int version, int size)
		{
			AddNode(type, name, size, version, TransferMetaFlags.NoTransferFlags);
		}

		public void AddNode(string type, string name, int version, int size, TransferMetaFlags flags)
		{
			AddNode(type, name, size, version, false, flags);
		}

		private void AddNode(string type, string name, int version, int size, bool isArray, TransferMetaFlags flags)
		{
			TypeTreeNode node = new TypeTreeNode();
			node.Version = version;
			node.Level = (byte)Depth;
			node.ByteSize = size;
			node.Type = type;
			node.Name = name;
			node.Index = Index;
			node.TypeFlags = isArray ? 1 : 0;
			node.MetaFlag = flags;
			m_nodes.Add(node);

			DepthIndex = Index;
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
			Index++;
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
			BeginArrayInner(TypeTreeUtils.StringName, name, DefaultStringFlag);
			AddNode(TypeTreeUtils.CharName, TypeTreeUtils.DataName, 1, sizeof(byte));
			EndArrayInner();
		}

		public void AddPPtr(string type, string name)
		{
			AddPPtr(type, name, TransferMetaFlags.NoTransferFlags);
		}

		public void AddPPtr(string type, string name, TransferMetaFlags flags)
		{
			PPtrLayout.GenerateTypeTree(this, type, name);
			Nodes[DepthIndex].MetaFlag = flags;
		}

		public void AddArray(string name, TypeTreeGenerator generator)
		{
			AddArray(name, TransferMetaFlags.NoTransferFlags, generator);
		}

		public void AddArray(string name, TransferMetaFlags flags, TypeTreeGenerator generator)
		{
			BeginArray(name, flags);
			generator.Invoke(this, TypeTreeUtils.DataName);
			EndArray();
		}

		public void AddArray(string type, string name, TypeTreeExGenerator generator)
		{
			BeginArray(name);
			generator.Invoke(this, type, TypeTreeUtils.DataName);
			EndArray();
		}

		public void AddArray(string name, TypeTreeTGenerator generator, TypeTreeGenerator type)
		{
			BeginArray(name);
			generator.Invoke(this, TypeTreeUtils.DataName, type);
			EndArray();
		}

		public void AddArray(string name, TypeTreeT2Generator generator, TypeTreeGenerator first, TypeTreeGenerator second)
		{
			BeginArray(name);
			generator.Invoke(this, TypeTreeUtils.DataName, first, second);
			EndArray();
		}

		public void BeginArray(string name)
		{
			BeginArray(name, TransferMetaFlags.NoTransferFlags);
		}

		public void BeginArray(string name, TransferMetaFlags flags)
		{
			BeginArrayInner(TypeTreeUtils.VectorName, name, flags);
		}

		public void EndArray()
		{
			EndArrayInner();
		}

		public void Align()
		{
			Nodes[DepthIndex].MetaFlag |= TransferMetaFlags.AlignBytesFlag;
		}

		private void BeginArrayInner(string type, string name, TransferMetaFlags flags)
		{
			AddNode(type, name, 1, -1);
			BeginChildren();
			AddNode(TypeTreeUtils.ArrayName, TypeTreeUtils.ArrayName, 1, -1, true, DefaultArrayFlag | flags);
			BeginChildren();
			AddInt32(TypeTreeUtils.SizeName);
		}

		private void EndArrayInner()
		{
			EndChildren();
			EndChildren();
		}

		public void BeginChildren()
		{
			Depth++;
			m_hierarchy.Push(new HierarchyData(DepthIndex, Size));
			DepthIndex = -1;
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
			DepthIndex = hierarchy.Index;
			Size = hierarchy.Size;
			if (Size >= 0)
			{
				Size += size;
			}
			Depth--;
		}

		public AssetLayout Layout { get; }
		
		public IReadOnlyList<TypeTreeNode> Nodes => m_nodes;
		private int Depth { get; set; }
		private int Index { get; set; }
		private int DepthIndex { get; set; }
		private int Size { get; set; }

		private TransferMetaFlags DefaultStringFlag { get; }
		private TransferMetaFlags DefaultArrayFlag { get; }

		private readonly List<TypeTreeNode> m_nodes = new List<TypeTreeNode>();
		private readonly Stack<HierarchyData> m_hierarchy = new Stack<HierarchyData>();
	}
}
