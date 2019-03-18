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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_TOS.Clear();

			AvatarSize = reader.ReadUInt32();
			AvatarConstant.Read(reader);
			m_TOS.Read(reader);
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_AvatarSize", AvatarSize);
			node.Add("m_Avatar", AvatarConstant.ExportYAML(container));
			node.Add("m_TOS", TOS.ExportYAML());
			return node;
		}

		public uint AvatarSize { get; private set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;

		public AvatarConstant AvatarConstant;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();
	}
}
