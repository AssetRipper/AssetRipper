using System.Text;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public static class TypeTreeDumper
{
	public static string Dump(ITypeTree tree)
	{
		StringBuilder sb = new();
		tree.ToString(sb);
		return sb.ToString();
	}

	private static StringBuilder ToString(this ITypeTree tree, StringBuilder sb)
	{
		foreach (ITypeTreeNode node in tree)
		{
			node.ToString(sb).AppendLine();
		}
		return sb;
	}

	private static StringBuilder ToString(this ITypeTreeNode node, StringBuilder sb)
	{
		sb.Append('\t', node.Level).Append(node.Type).Append(' ').Append(node.Name);
		sb.Append($" // ByteSize{"{"}{node.ByteSize:x}{"}"}, Index{"{"}{node.Index:x}{"}"}, Version{"{"}{node.Version:x}{"}"}, IsArray{{{node.TypeFlags}}}, MetaFlag{"{"}{(uint)node.MetaFlag:x}{"}"}");
		return sb;
	}
}
