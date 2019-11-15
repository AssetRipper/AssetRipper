using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class MonoBehaviourLayout
	{
		public MonoBehaviourLayout(LayoutInfo info)
		{
			if (!info.Flags.IsRelease())
			{
				HasEditorHideFlags = true;
			}
			if (info.Version.IsGreaterEqual(2019) && info.Version.IsLess(2019, 1, 0, VersionType.Beta, 4) && !info.Flags.IsRelease())
			{
				HasGeneratorAsset = true;
			}
			if (info.Version.IsGreaterEqual(4, 2) && !info.Flags.IsRelease())
			{
				HasEditorClassIdentifier = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			BehaviourLayout.GenerateTypeTree(context);

			MonoBehaviourLayout layout = context.Layout.MonoBehaviour;
			if (layout.HasEditorHideFlags)
			{
				context.AddUInt32(layout.EditorHideFlagsName);
			}
			if (layout.HasGeneratorAsset)
			{
				context.AddPPtr(context.Layout.Object.Name, layout.GeneratorAssetName);
			}
			context.AddPPtr(context.Layout.MonoScript.Name, layout.ScriptName);
			context.AddString(layout.NameName);
			if (layout.HasEditorClassIdentifier)
			{
				context.AddString(layout.EditorClassIdentifierName);
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
		/// All versions
		/// </summary>
		public bool HasScript => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasName => true;
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
