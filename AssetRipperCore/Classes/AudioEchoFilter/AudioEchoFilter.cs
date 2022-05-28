using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioEchoFilter
{
	public sealed class AudioEchoFilter : AudioBehaviour
	{
		public AudioEchoFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Delay = reader.ReadSingle();
			DecayRatio = reader.ReadSingle();
			DryMix = reader.ReadSingle();
			WetMix = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Delay", Delay);
			node.Add("m_DecayRatio", DecayRatio);
			node.Add("m_DryMix", DryMix);
			node.Add("m_WetMix", WetMix);
			return node;
		}

		public float Delay { get; set; }
		public float DecayRatio { get; set; }
		public float DryMix { get; set; }
		public float WetMix { get; set; }
	}
}
