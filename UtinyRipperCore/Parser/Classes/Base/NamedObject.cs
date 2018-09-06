using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public abstract class NamedObject : EditorExtension
	{
		protected NamedObject(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Name = reader.ReadStringAligned();
			if(string.IsNullOrEmpty(Name))
			{
				Name = GetType().Name;
			}
		}

		public override string ToString()
		{
			return $"{Name}({GetType().Name})";
		}

		public override string ToLogString()
		{
			return $"{GetType().Name}'s({Name})[{PathID}]";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode root = base.ExportYAMLRoot(container);
			root.Add("m_Name", Name);
			return root;
		}
		
		public string Name { get; protected set; }
	}
}
