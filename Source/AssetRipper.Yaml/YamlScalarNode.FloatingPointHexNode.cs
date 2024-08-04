using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	/// <summary>
	/// Hexidecimal representation of a floating point number. <see href="https://docs.unity3d.com/2022.3/Documentation/Manual/FormatDescription.html"/>
	/// </summary>
	/// <remarks>
	/// This is the only lossless way to represent a floating point number in YAML.
	/// However, the output is less readable than the default representation.
	/// </remarks>
	/// <typeparam name="T">The floating point type.</typeparam>
	/// <param name="value"></param>
	private sealed class FloatingPointHexNode<T>(T value) : YamlScalarNode where T : IBinaryFloatingPointIeee754<T>
	{
		private protected override void EmitCore(Emitter emitter)
		{
			emitter.Write(Value);
		}

		public override string Value
		{
			get
			{
				if (typeof(T) == typeof(float))
				{
					float single = Unsafe.As<T, float>(ref value);
					uint hex = BitConverter.SingleToUInt32Bits(single);
					return $"0x{hex:x8}({single.ToString(CultureInfo.InvariantCulture)})";
				}
				else if (typeof(T) == typeof(double))
				{
					double single = Unsafe.As<T, double>(ref value);
					ulong hex = BitConverter.DoubleToUInt64Bits(single);
					return $"0x{hex:x16}({single.ToString(CultureInfo.InvariantCulture)})";
				}
				else
				{
					// This can't happen
					return value.ToString() ?? "";
				}
			}
		}
	}
}
