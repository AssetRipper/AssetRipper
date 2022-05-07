using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public abstract class NamedObject : EditorExtension, INamedObject
	{
		protected NamedObject(LayoutInfo layout) : base(layout)
		{
			NameString = string.Empty;
		}

		protected NamedObject(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			NameString = reader.ReadString();
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			writer.Write(NameString);
		}

		public override string ToString()
		{
			return $"{this.GetValidName()}({GetType().Name})";
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode root = base.ExportYamlRoot(container);
			root.Add(NameName, NameString);
			return root;
		}

		public string NameString { get; set; }

		public const string NameName = "m_Name";
	}
}
