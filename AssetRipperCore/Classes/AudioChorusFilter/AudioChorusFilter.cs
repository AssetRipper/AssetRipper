using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioChorusFilter
{
	public sealed class AudioChorusFilter : AudioBehaviour
	{
		public AudioChorusFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			dryMix = reader.ReadSingle();
			wetMix1 = reader.ReadSingle();
			wetMix2 = reader.ReadSingle();
			wetMix3 = reader.ReadSingle();
			delay = reader.ReadSingle();
			rate = reader.ReadSingle();
			depth = reader.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("dryMix", dryMix);
			node.Add("wetMix1", wetMix1);
			node.Add("wetMix2", wetMix2);
			node.Add("wetMix3", wetMix3);
			node.Add("delay", delay);
			node.Add("rate", rate);
			node.Add("depth", depth);
			return node;
		}

		public float dryMix { get; set; }
		public float wetMix1 { get; set; }
		public float wetMix2 { get; set; }
		public float wetMix3 { get; set; }
		public float delay { get; set; }
		public float rate { get; set; }
		public float depth { get; set; }
	}
}
