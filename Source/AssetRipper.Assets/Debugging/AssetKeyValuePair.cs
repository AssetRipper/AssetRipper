using System.Diagnostics;

namespace AssetRipper.Assets.Debugging;

[DebuggerDisplay("{Value}", Name = "{Key}")]
internal readonly struct AssetKeyValuePair
{
	public object? Key { get; }
	public object? Value { get; }

	public AssetKeyValuePair(object? key, object? value)
	{
		Key = key;
		Value = value;
	}
}
