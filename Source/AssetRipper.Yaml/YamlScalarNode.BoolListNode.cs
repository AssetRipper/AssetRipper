using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class BoolListNode(IReadOnlyList<bool> list) : YamlScalarNode
	{
		private protected override void EmitCore(Emitter emitter)
		{
			Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<byte>()];
			for (int i = 0; i < list.Count; i++)
			{
				byte b = list[i] ? (byte)1 : (byte)0;
				ReverseHexString.WriteReverseHexString(b, buffer);
				emitter.Write(buffer);
			}
		}

		public override string Value => list.ToString() ?? "";
	}
}
