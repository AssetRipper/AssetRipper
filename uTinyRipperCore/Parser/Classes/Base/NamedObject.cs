using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Objects;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class NamedObject : EditorExtension
	{
		protected NamedObject(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		protected NamedObject(AssetInfo assetInfo, HideFlags hideFlags) :
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

			Name = reader.ReadString();
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
			root.Add(NameName, Name);
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

		public const string NameName = "m_Name";
	}
}
