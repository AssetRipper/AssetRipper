using AssetRipper.SourceGenerated.Classes.ClassID_78;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TagManagerExtensions
	{
		/// <summary>
		/// 5.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsBrokenCustomTags(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLess(5, 5);

		/// <summary>
		/// 5.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsBrokenCustomTags(this ITagManager tagManager) => IsBrokenCustomTags(tagManager.SerializedFile.Version);
	}
}
