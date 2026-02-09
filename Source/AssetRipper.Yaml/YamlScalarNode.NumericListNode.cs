using AssetRipper.Yaml.Extensions;
using System.Numerics;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class NumericListNode<T>(IReadOnlyList<T> list) : YamlScalarNode where T : unmanaged, INumber<T>
	{
		private protected override void EmitCore(Emitter emitter)
		{
			Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<T>()];
			for (int i = 0; i < list.Count; i++)
			{
				ReverseHexString.WriteReverseHexString(list[i], buffer);
				emitter.Write(buffer);
			}
		}

		public override string Value
		{
			get
			{
				StringWriter sb = new();
				Span<char> buffer = stackalloc char[ReverseHexString.GetHexStringLength<T>()];
				for (int i = 0; i < list.Count; i++)
				{
					ReverseHexString.WriteReverseHexString(list[i], buffer);
					sb.Write(buffer);
				}
				return sb.ToString();
			}
		}
	}
}
