using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly struct BundlePath : IPath<BundlePath>, IEquatable<BundlePath>
{
	private readonly int[]? _path;

	private BundlePath(int[]? path)
	{
		_path = path;
	}

	public BundlePath(ReadOnlySpan<int> path)
	{
		_path = path.Length == 0 ? null : path.ToArray();
	}

	[JsonConstructor]
	public BundlePath(ReadOnlyMemory<int> path)
	{
		_path = path.Length == 0 ? null : path.ToArray();
	}

	[JsonPropertyName("P")]
	public ReadOnlyMemory<int> Path => _path;

	[JsonIgnore]
	public ReadOnlySpan<int> Span => _path;

	[JsonIgnore]
	public int Depth => Path.Length;

	[JsonIgnore]
	public bool IsRoot => Depth == 0;

	/// <summary>
	/// Get the path of the parent bundle.
	/// </summary>
	/// <remarks>
	/// If <see cref="IsRoot"/>, then this will return <see langword="default"/>.
	/// </remarks>
	[JsonIgnore]
	public BundlePath Parent => Depth > 1 ? new BundlePath(Path.Span[..^1]) : default;

	public BundlePath GetChild(int index)
	{
		int[] path;
		if (_path is null)
		{
			path = [index];
		}
		else
		{
			path = new int[_path.Length + 1];
			_path.CopyTo((Span<int>)path);
			path[^1] = index;
		}
		return new BundlePath(path);
	}

	public CollectionPath GetCollection(int index)
	{
		return new CollectionPath(this, index);
	}

	public FailedFilePath GetFailedFile(int index)
	{
		return new FailedFilePath(this, index);
	}

	public ResourcePath GetResource(int index)
	{
		return new ResourcePath(this, index);
	}

	public static implicit operator ReadOnlySpan<int>(BundlePath path) => path.Span;

	public static bool operator ==(BundlePath left, BundlePath right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BundlePath left, BundlePath right)
	{
		return !(left == right);
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.BundlePath);
	}

	public static BundlePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.BundlePath);
	}

	public override string ToString() => ToJson();

	public override bool Equals(object? obj)
	{
		return obj is BundlePath path && Equals(path);
	}

	public bool Equals(BundlePath other)
	{
		return Span.SequenceEqual(other.Span);
	}

	public override int GetHashCode()
	{
		HashCode code = new();
		foreach (int item in Span)
		{
			code.Add(item);
		}
		return code.ToHashCode();
	}
}
