using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.TagManager
{
	public interface ITagManager : IUnityObjectBase
	{
		Utf8StringBase[] Tags { get; }
	}

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
