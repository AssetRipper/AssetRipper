using AssetRipper.AssemblyDumper.AST;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private static partial class NodeHelper
	{
		private static string GetPathContent(Node node) => node switch
		{
			FieldNode fieldNode => fieldNode.Field.Name ?? "",
			ListNode or DictionaryNode => "[]",
			KeyNode => ".Key",
			ValueNode => ".Value",
			_ => "",
		};

		private static char GetStateFieldTypeCharacter(Node node) => node switch
		{
			TypeNode => 'T',
			ListNode => 'L',
			DictionaryNode => 'D',
			PairNode => 'P',
			_ => 'U',
		};

		public static void Apply(Node node, DependencyMethodContext context, ParentContext parentContext)
		{
			switch (node)
			{
				case TypeNode typeNode:
					TypeNodeHelper.Apply(typeNode, context, parentContext);
					break;
				case FieldNode fieldNode:
					Apply(fieldNode.Child, context, parentContext);
					break;
				case ListNode listNode:
					ListNodeHelper.Apply(listNode, context, parentContext);
					break;
				case DictionaryNode dictionaryNode:
					DictionaryNodeHelper.Apply(dictionaryNode, context, parentContext);
					break;
				case KeyNode keyNode:
					Apply(keyNode.Child, context, parentContext);
					break;
				case ValueNode valueNode:
					Apply(valueNode.Child, context, parentContext);
					break;
				case PPtrNode pptrNode:
					PPtrNodeHelper.Apply(pptrNode, context, parentContext);
					break;
				case PairNode pairNode:
					PairNodeHelper.Apply(pairNode, context, parentContext);
					break;
				default:
					throw new NotSupportedException(node.GetType().Name);
			}
		}

		public static string GetFullPath(Node node)
		{
			if (node.Parent is null)
			{
				return GetPathContent(node);
			}

			StringBuilder sb = new();
			Node? current = node;
			while (current != null)
			{
				string content = GetPathContent(current);
				for (int i = content.Length - 1; i >= 0; i--)
				{
					sb.Append(content[i]);
				}
				if (current is FieldNode)
				{
					sb.Append('.');
				}
				current = current.Parent;
			}
			if (sb.Length > 0 && sb[^1] == '.')
			{
				sb.Remove(sb.Length - 1, 1);
			}

			ReverseCharacterOrder(sb);

			return sb.ToString();
		}

		private static void ReverseCharacterOrder(StringBuilder sb)
		{
			int midpoint = sb.Length / 2;
			int lastIndex = sb.Length - 1;
			for (int i = 0; i < midpoint; i++)
			{
				char first = sb[i];
				char second = sb[lastIndex - i];
				sb[i] = second;
				sb[lastIndex - i] = first;
			}
		}

		/// <summary>
		/// The name of the field that stores the state of this node, if it has one.
		/// </summary>
		public static string GetStateFieldName(Node node)
		{
			return $"{InvalidMethodCharacters().Replace(GetFullPath(node), "_")}_state{GetStateFieldTypeCharacter(node)}";
		}

		[GeneratedRegex(@"[\[\].]")]
		private static partial Regex InvalidMethodCharacters();
	}
}
