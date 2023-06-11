using System.Text;

namespace AssetRipper.Primitives;

/// <summary>
/// An immutable, thread-safe, representation of UTF8 text data.
/// </summary>
/// <remarks>
/// Conversions to and from <see cref="string"/> are handled by <see cref="Encoding.UTF8"/>.
/// </remarks>
public sealed class Utf8String : IEquatable<Utf8String>
{
	private readonly byte[] data;
	private string? cachedString;

	public static Utf8String Empty { get; } = new();

	/// <summary>
	/// The empty string. A shared instance is available in <see cref="Empty"/>.
	/// </summary>
	public Utf8String()
	{
		data = Array.Empty<byte>();
		cachedString = "";
	}

	public Utf8String(ReadOnlySpan<byte> data)
	{
		this.data = data.ToArray();
	}

	public Utf8String(string @string)
	{
		cachedString = @string;
		data = Encoding.UTF8.GetBytes(@string);
	}

	public ReadOnlySpan<byte> Data => data;

	public string String
	{
		get
		{
			if (cachedString is null)
			{
				Interlocked.Exchange(ref cachedString, Encoding.UTF8.GetString(data));
			}
			return cachedString;
		}
	}

	public override string ToString() => String;

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.AddBytes(data);
		return hash.ToHashCode();
	}

	public override bool Equals(object? obj) => Equals(obj as Utf8String);

	public bool Equals(Utf8String? other) => other is not null && Data.SequenceEqual(other.Data);

	public static bool operator ==(Utf8String? string1, Utf8String? string2)
	{
		return EqualityComparer<Utf8String>.Default.Equals(string1, string2);
	}
	public static bool operator !=(Utf8String? string1, Utf8String? string2) => !(string1 == string2);

	public static bool operator ==(Utf8String? utf8String, string? @string) => utf8String?.String == @string;
	public static bool operator !=(Utf8String? utf8String, string? @string) => utf8String?.String != @string;

	public static bool operator ==(string? @string, Utf8String? utf8String) => utf8String?.String == @string;
	public static bool operator !=(string? @string, Utf8String? utf8String) => utf8String?.String != @string;

	[return: NotNullIfNotNull(nameof(@string))]
	public static implicit operator Utf8String?(string? @string)
	{
		return @string is null ? null : new Utf8String(@string);
	}

	[return: NotNullIfNotNull(nameof(utf8String))]
	public static implicit operator string?(Utf8String? utf8String)
	{
		return utf8String?.String;
	}

	public static implicit operator Utf8String(ReadOnlySpan<byte> data)
	{
		return new Utf8String(data);
	}

	public static implicit operator ReadOnlySpan<byte>(Utf8String? utf8String)
	{
		return utf8String is null ? default : utf8String.Data;
	}
}
