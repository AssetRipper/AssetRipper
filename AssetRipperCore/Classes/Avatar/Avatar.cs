using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Avatar : NamedObject
	{
		public Avatar(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasHumanDescription(UnityVersion version) => version.IsGreaterEqual(2019);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_TOS.Clear();

			AvatarSize = reader.ReadUInt32();
			AvatarConstant.Read(reader);
			m_TOS.Read(reader);
			if (HasHumanDescription(reader.Version))
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
			if (HasHumanDescription(container.Version))
			{
				node.Add(HumanDescriptionName, HumanDescription.ExportYAML(container));
			}
			return node;
		}

		public string FindBonePath(uint hash)
		{
			m_TOS.TryGetValue(hash, out string result);
			return result;
		}

		public uint AvatarSize { get; set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;

		public const string AvatarSizeName = "m_AvatarSize";
		public const string AvatarName = "m_Avatar";
		public const string TOSName = "m_TOS";
		public const string HumanDescriptionName = "m_HumanDescription";

		public AvatarConstant AvatarConstant = new();
		public HumanDescription HumanDescription = new();

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();
	}
}
