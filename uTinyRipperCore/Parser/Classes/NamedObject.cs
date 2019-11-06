using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class NamedObject : EditorExtension
	{
		protected NamedObject(Version version):
			base(version)
		{
			Name = string.Empty;
		}

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

		protected new static void GenerateTypeTree(TypeTreeContext context)
		{
			EditorExtension.GenerateTypeTree(context);
			context.AddString(NameName);
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
			root.Add(NameName, Name);
			return root;
		}

		public virtual string ValidName
		{
			get
			{
				if (Name.Length > 0)
				{
					return GetType().Name;
				}
				return Name;
			}
		}
		public string Name { get; set; }

		public const string NameName = "m_Name";
	}
}
