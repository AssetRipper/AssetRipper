using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct BundlePath : IPath<BundlePath>
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

	public ReadOnlyMemory<int> Path => _path;

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
	public BundlePath Parent => Depth > 1 ? new BundlePath(Path.Span[..^-1]) : default;

	public BundlePath GetChild(int index)
	{
		int[] path;
		if (_path is null)
		{
			path = new int[1] { index };
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

	public ResourcePath GetResource(int index)
	{
		return new ResourcePath(this, index);
	}

	public static implicit operator ReadOnlySpan<int>(BundlePath path) => path._path;

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.BundlePath);
	}

	public static BundlePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.BundlePath);
	}
}
