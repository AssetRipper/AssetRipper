using AssetRipper.SourceGenerated.Classes.ClassID_142;

namespace AssetRipper.Core.SourceGenExtensions
{
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
			return bundle.Has_AssetBundleName_C142() ? bundle.AssetBundleName_C142.String : bundle.SerializedFile.Name;
		}
	}
}
