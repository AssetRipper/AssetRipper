using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Avatars;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Avatar : NamedObject
	{
		public Avatar(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadHumanDescription(Version version)
		{
			return version.IsGreaterEqual(2019);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_TOS.Clear();

			AvatarSize = reader.ReadUInt32();
			AvatarConstant.Read(reader);
			m_TOS.Read(reader);
			if (IsReadHumanDescription(reader.Version))
			{
				HumanDescription.Read(reader);
			}
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(AvatarSizeName, AvatarSize);
			node.Add(AvatarName, AvatarConstant.ExportYAML(container));
			node.Add(TOSName, TOS.ExportYAML());
			if (IsReadHumanDescription(container.Version))
			{
				node.Add(HumanDescriptionName, HumanDescription.ExportYAML(container));
			}
			return node;
		}

		public uint AvatarSize { get; private set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;

		public const string AvatarSizeName = "m_AvatarSize";
		public const string AvatarName = "m_Avatar";
		public const string TOSName = "m_TOS";
		public const string HumanDescriptionName = "m_HumanDescription";

		public AvatarConstant AvatarConstant;
		public HumanDescription HumanDescription;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();
	}
}
