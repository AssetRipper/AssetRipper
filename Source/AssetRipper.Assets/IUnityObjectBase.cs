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

	string GetBestName();
	void CopyValues(IUnityObjectBase? source) => CopyValues(source, new PPtrConverter(source, this));
}
public static class UnityObjectBaseExtensions
{
	public static void Read(this IUnityObjectBase asset, ref EndianSpanReader reader) => asset.Read(ref reader, asset.Collection.Flags);
}
