using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class CharListNode(IReadOnlyList<char> list) : YamlScalarNode
	{
		private protected override void EmitCore(Emitter emitter)
		{
			Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<ushort>()];
			for (int i = 0; i < list.Count; i++)
			{
				ReverseHexString.WriteReverseHexString((ushort)list[i], buffer);
				emitter.Write(buffer);
			}
		}

		public override string Value
		{
			get
			{
				StringWriter sb = new();
				Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<ushort>()];
				for (int i = 0; i < list.Count; i++)
				{
					ReverseHexString.WriteReverseHexString((ushort)list[i], buffer);
					sb.Write(buffer);
				}
				return sb.ToString();
			}
		}
	}
}
