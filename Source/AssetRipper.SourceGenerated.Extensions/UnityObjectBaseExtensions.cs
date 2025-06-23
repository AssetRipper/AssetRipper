using AssetRipper.Assets;

namespace AssetRipper.SourceGenerated.Extensions;

public static class UnityObjectBaseExtensions
{
	public static string GetOriginalName(this IUnityObjectBase _this)
	{
		if (_this is INamed named)
		{
			return named.Name;
		}
		else
		{
			throw new Exception($"Unable to get name for {_this.ClassID}");
		}
	}

	public static string? TryGetName(this IUnityObjectBase _this)
	{
		return (_this as INamed)?.Name;
	}

	public static string GetLogString(this IUnityObjectBase asset)
	{
		string? name = asset.TryGetName();
		if (name == null)
		{
			return $"{asset.ClassID}_{asset.PathID}";
		}
		else
		{
			return $"{asset.ClassID}_{asset.PathID}({name})";
		}
	}
}
