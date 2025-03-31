# AssetRipper.SmartEnums

User code:

```cs
[SmartEnum]
public readonly partial record struct MyEnum
{
	private enum __Internal : uint
	{
		Value0,
		Value1
	}

	public void CustomMethod() {}
}
```

Generated code:

```cs
readonly partial record struct MyEnum :
	global::System.IParsable<MyEnum>,
	global::System.Numerics.IBitwiseOperators<MyEnum, MyEnum, MyEnum>,
	global::System.Numerics.IComparisonOperators<MyEnum, MyEnum, bool>,
	global::System.Numerics.IEqualityOperators<MyEnum, MyEnum, bool>,
	global::System.Numerics.IShiftOperators<MyEnum, int, MyEnum>
{
	private readonly uint __value;

	public MyEnum(uint value) => __value = value;

	/// <inheritdoc cref="__Internal.Value0"/>
	public const uint Value0 = (uint)__Internal.Value0;

	/// <inheritdoc cref="__Internal.Value1"/>
	public const uint Value1 = (uint)__Internal.Value1;

	public static implicit operator uint(MyEnum value) => value.__value;
	public static implicit operator MyEnum(uint value) => new(value);

	public static MyEnum operator &(MyEnum left, MyEnum right) => new(left.__value & right.__value);
	public static MyEnum operator |(MyEnum left, MyEnum right) => new(left.__value | right.__value);
	public static MyEnum operator ^(MyEnum left, MyEnum right) => new(left.__value ^ right.__value);
	public static MyEnum operator ~(MyEnum value) => new(~value.__value);

	public static bool operator <(MyEnum left, MyEnum right) => left.__value < right.__value;
	public static bool operator >(MyEnum left, MyEnum right) => left.__value > right.__value;
	public static bool operator <=(MyEnum left, MyEnum right) => left.__value <= right.__value;
	public static bool operator >=(MyEnum left, MyEnum right) => left.__value >= right.__value;

	public static MyEnum operator <<(MyEnum value, int count) => new(value.__value << count);
	public static MyEnum operator >>(MyEnum value, int count) => new(value.__value >> count);
	public static MyEnum operator >>>(MyEnum value, int count) => new(value.__value >>> count);

	public override string ToString() => __value switch
	{
		Value0 => nameof(Value0),
		Value1 => nameof(Value1),
		_ => __value.ToString(),
	};

	public static MyEnum Parse(string s) => Parse(s, null);
	public static MyEnum Parse(string s, IFormatProvider? provider) => s switch
	{
		nameof(MyEnum.Value0) => MyEnum.Value0,
		nameof(MyEnum.Value1) => MyEnum.Value1,
		_ => uint.Parse(s, provider),
	};
	public static bool TryParse(string? s, out MyEnum result) => TryParse(s, null, out result);
	public static bool TryParse(string? s, IFormatProvider? provider, out MyEnum result)
	{
		switch (s)
		{
			case nameof(MyEnum.Value0):
				result = MyEnum.Value0;
				return true;
			case nameof(MyEnum.Value1):
				result = MyEnum.Value1;
				return true;
			default:
				if (uint.TryParse(s, provider, out uint value))
				{
					result = new(value);
					return true;
				}
				result = default;
				return false;
		}
	}

	public static global::System.ReadOnlySpan<MyEnum> GetValues() => __ValueCache.Values;
	public static global::System.ReadOnlySpan<uint> GetUnderlyingValues() =>
	[
		Value0,
		Value1,
	];
	public uint GetUnderlyingValue() => __value;
}
file static class __ValueCache
{
	public static MyEnum[] Values = new MyEnum[]
	{
		MyEnum.Value0,
		MyEnum.Value1,
	};
}
```