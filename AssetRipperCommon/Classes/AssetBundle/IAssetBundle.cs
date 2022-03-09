using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public interface IAssetBundle : IUnityObjectBase
	{
		bool HasAssetBundleName { get; }
		string AssetBundleName { get; set; }
		NullableKeyValuePair<Utf8StringBase, IAssetInfo>[] GetAssets();
	}

	public static class AssetBundleExtensions
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(UnityVersion version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(this IAssetBundle bundle) => HasPathExtension(bundle.SerializedFile.Version);

		public static string GetAssetBundleName(this IAssetBundle bundle)
		{
			return bundle.HasAssetBundleName ? bundle.AssetBundleName : bundle.SerializedFile.Name;
		}
	}
}
