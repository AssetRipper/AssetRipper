using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class NamedObject : EditorExtension
	{
		protected NamedObject(AssetLayout layout):
			base(layout)
		{
			Name = string.Empty;
		}

		protected NamedObject(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

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
			return $"{ValidName}({GetType().Name})";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode root = base.ExportYAMLRoot(container);
			NamedObjectLayout layout = container.ExportLayout.NamedObject;
			root.Add(layout.NameName, Name);
			return root;
		}

		public virtual string ValidName => Name.Length == 0 ? GetType().Name : Name;

		public string Name { get; set; }
	}
}
