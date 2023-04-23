using AssetRipper.IO.Endian;
using System.Collections;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public abstract class TypeTree<T> : ITypeTree where T : ITypeTreeNode, new()
{
	public List<T> Nodes { get; } = new();
	public byte[] StringBuffer { get; set; } = Array.Empty<byte>();

	public ITypeTreeNode this[int index] => Nodes[index];

	public int Count => Nodes.Count;

	public IEnumerator<ITypeTreeNode> GetEnumerator()
	{
		foreach (T node in Nodes)
		{
			yield return node;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public abstract void Read(EndianReader reader);

	public abstract void Write(EndianWriter writer);

	public string Dump() => TypeTreeDumper.Dump(this);
}
