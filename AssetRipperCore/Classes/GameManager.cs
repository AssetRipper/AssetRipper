using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public abstract class GameManager : EditorExtension, IGameManager
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

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			return HasEditorExtension(container.ExportVersion) ? base.ExportYAMLRoot(container) : ExportYAMLRootObject(container);
		}
	}
}
