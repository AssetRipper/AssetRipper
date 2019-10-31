using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class GameManager : EditorExtension
	{
		protected GameManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasEditorExtension(Version version) => version.IsLess(3);

		public override void Read(AssetReader reader)
		{
			if (HasEditorExtension(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadObject(reader);
			}
		}

		public override void Write(AssetWriter writer)
		{
			if (HasEditorExtension(writer.Version))
			{
				base.Write(writer);
			}
			else
			{
				WriteObject(writer);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			return HasEditorExtension(container.ExportVersion) ? base.ExportYAMLRoot(container) : ExportYAMLRootObject(container);
		}
	}
}
