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

		protected NamedObject(AssetInfo assetInfo, uint hideFlags) :
			base(assetInfo, hideFlags)
		{
			Name = string.Empty;
		}

		protected NamedObject(AssetInfo assetInfo, bool _) :
			base(assetInfo)
		{
			Name = string.Empty;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Name = reader.ReadStringAligned();
		}

		public override string ToString()
		{
			return $"{ValidName}({GetType().Name})";
		}

		public override string ToLogString()
		{
			return $"{GetType().Name}'s({ValidName})[{PathID}]";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode root = base.ExportYAMLRoot(container);
			root.Add("m_Name", Name);
			return root;
		}

		public virtual string ValidName
		{
			get
			{
				if (Name == string.Empty)
				{
					return GetType().Name;
				}
				return Name;
			}
		}
		public string Name { get; protected set; }
	}
}
