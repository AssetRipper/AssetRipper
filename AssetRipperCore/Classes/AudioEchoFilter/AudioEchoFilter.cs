using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioEchoFilter
{
	public sealed class AudioEchoFilter : AudioBehaviour
	{
		public AudioEchoFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			delay = reader.ReadSingle();
			decayRatio = reader.ReadSingle();
			dryMix = reader.ReadSingle();
			wetMix = reader.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("delay", delay);
			node.Add("decayRatio", decayRatio);
			node.Add("dryMix", dryMix);
			node.Add("wetMix", wetMix);
			return node;
		}

		public float delay { get; set; }
		public float decayRatio { get; set; }
		public float dryMix { get; set; }
		public float wetMix { get; set; }
	}
}
