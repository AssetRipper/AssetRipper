using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public abstract class GameManager : EditorExtension
	{
		protected GameManager(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasEditorExtension(UnityVersion version) => version.IsLess(3);

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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			return HasEditorExtension(container.ExportVersion) ? base.ExportYamlRoot(container) : ExportYamlRootObject(container);
		}
	}
}
