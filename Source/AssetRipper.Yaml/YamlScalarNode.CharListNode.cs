using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class CharListNode(IReadOnlyList<char> list) : YamlScalarNode
	{
		private protected override void EmitCore(Emitter emitter)
		{
			Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<ushort>()];
			foreach (char value in list)
			{
				ReverseHexString.WriteReverseHexString((ushort)value, buffer);
				emitter.Write(buffer);
			}
		}

		public override string Value => list.ToString() ?? "";
	}
}
