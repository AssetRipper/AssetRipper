using AssetRipper.Core.Classes;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Layout.Classes
{
	public sealed class MonoBehaviourLayout
	{
		public MonoBehaviourLayout(LayoutInfo info)
		{
			if (!info.Flags.IsRelease())
			{
				HasEditorHideFlags = true;
			}
			if (info.Version.IsGreaterEqual(2019) && info.Version.IsLess(2019, 1, 0, UnityVersionType.Beta, 4) && !info.Flags.IsRelease())
			{
				HasGeneratorAsset = true;
			}
			if (info.Version.IsGreaterEqual(4, 2) && !info.Flags.IsRelease())
			{
				HasEditorClassIdentifier = true;
			}
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public bool HasEditorHideFlags { get; }
		/// <summary>
		/// 2019.1 to 2019.1.0b4 exclusive and Not Release
		/// </summary>
		public bool HasGeneratorAsset { get; }
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public bool HasEditorClassIdentifier { get; }

		public string Name => nameof(MonoBehaviour);
		public string EditorHideFlagsName => "m_EditorHideFlags";
		public string GeneratorAssetName => "m_GeneratorAsset";
		public string ScriptName => "m_Script";
		public string NameName => "m_Name";
		public string EditorClassIdentifierName => "m_EditorClassIdentifier";
	}
}
