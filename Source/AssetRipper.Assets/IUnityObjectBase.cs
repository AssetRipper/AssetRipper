using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;

namespace AssetRipper.Assets;

public interface IUnityObjectBase : IUnityAssetBase
{
	/// <summary>
	/// The key information about the location of this asset.
	/// </summary>
	AssetInfo AssetInfo { get; }
	/// <summary>
	/// The native class ID number of this object.
	/// </summary>
	int ClassID { get; }
	/// <summary>
	/// The native class name of this object.
	/// </summary>
	string ClassName { get; }
	/// <summary>
	/// The <see cref="AssetCollection"/> this object belongs to.
	/// </summary>
	AssetCollection Collection { get; }
	/// <summary>
	/// The <see cref="AssetInfo.PathID"/> of this object within <see cref="Collection"/>.
	/// </summary>
	long PathID { get; }
	/// <summary>
	/// The original path of this object, if known.
	/// </summary>
	/// <remarks>
	/// The path is relative to the project root and may use forward or back slashes.
	/// </remarks>
	string? OriginalPath { get; set; }
	/// <summary>
	/// The original directory of this object, if known.
	/// </summary>
	/// <remarks>
	/// The path is relative to the project root and may use forward or back slashes.
	/// </remarks>
	string? OriginalDirectory { get; set; }
	/// <summary>
	/// The original file name of this object, if known.
	/// </summary>
	string? OriginalName { get; set; }
	/// <summary>
	/// The original file extension of this object, if known.
	/// </summary>
	string? OriginalExtension { get; set; }
	/// <summary>
	/// The name of the asset bundle this object belongs to, if known.
	/// </summary>
	string? AssetBundleName { get; set; }
	/// <summary>
	/// The primary asset that this object is associated with, if any.
	/// </summary>
	IUnityObjectBase? MainAsset { get; set; }

	/// <summary>
	/// Get the best name for this object.
	/// </summary>
	/// <remarks>
	/// In order of preference:<br/>
	/// 1. <see cref="IHasNameString.NameString"/><br/>
	/// 2. <see cref="OriginalName"/><br/>
	/// 3. <see cref="ClassName"/><br/>
	/// <see cref="OriginalName"/> has secondary preference because file importers can create assets with a different name from the file.
	/// </remarks>
	/// <returns>A nonempty string.</returns>
	public sealed string GetBestName()
	{
		string? name = (this as INamed)?.Name;
		if (!string.IsNullOrEmpty(name))
		{
			return name;
		}
		else if (!string.IsNullOrEmpty(OriginalName))
		{
			return OriginalName;
		}
		else
		{
			return ClassName;
		}
	}

	public sealed void CopyValues(IUnityObjectBase? source)
	{
		if (source is null)
		{
			Reset();
		}
		else
		{
			CopyValues(source, new PPtrConverter(source, this));
		}
	}
}
public static class UnityObjectBaseExtensions
{
	public static void Read(this IUnityObjectBase asset, ref EndianSpanReader reader) => asset.Read(ref reader, asset.Collection.Flags);
}
