using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class NumericNode<T>(T value) : YamlScalarNode where T : struct, INumber<T>, IConvertible
	{
		private protected override void EmitCore(Emitter emitter)
		{
			if (typeof(T) == typeof(float))
			{
				emitter.Write(Unsafe.As<T, float>(ref value));
			}
			else if (typeof(T) == typeof(double))
			{
				emitter.Write(Unsafe.As<T, double>(ref value));
			}
			else if (IsSigned)
			{
				emitter.Write(value.ToInt64(CultureInfo.InvariantCulture));
			}
			else
			{
				emitter.Write(value.ToUInt64(CultureInfo.InvariantCulture));
			}
		}

		public override string Value => value.ToString() ?? "";

		private static bool IsSigned => typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long);
	}
}
