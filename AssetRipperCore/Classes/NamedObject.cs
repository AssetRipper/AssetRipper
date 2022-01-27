using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public abstract class NamedObject : EditorExtension, INamedObject
	{
		protected NamedObject(LayoutInfo layout) : base(layout)
		{
			Name = string.Empty;
		}

		protected NamedObject(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Name = reader.ReadString();
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			writer.Write(Name);
		}

		public override string ToString()
		{
			return $"{this.GetValidName()}({GetType().Name})";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode root = base.ExportYAMLRoot(container);
			root.Add(NameName, Name);
			return root;
		}

		public string Name { get; set; }

		public const string NameName = "m_Name";
	}
}
