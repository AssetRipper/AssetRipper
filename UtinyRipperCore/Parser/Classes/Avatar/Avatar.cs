using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Avatars;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class Avatar : NamedObject
	{
		public Avatar(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_TOS.Clear();

			AvatarSize = stream.ReadUInt32();
			AvatarConstant.Read(stream);
			m_TOS.Read(stream);
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
