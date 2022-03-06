using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioChorusFilter
{
	public sealed class AudioChorusFilter : AudioBehaviour
	{
		public AudioChorusFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public static bool HasFeedBack(UnityVersion version) => version.IsLess(4, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DryMix = reader.ReadSingle();
			WetMix1 = reader.ReadSingle();
			WetMix2 = reader.ReadSingle();
			WetMix3 = reader.ReadSingle();
			Delay = reader.ReadSingle();
			Rate = reader.ReadSingle();
			Depth = reader.ReadSingle();
			if (HasFeedBack(reader.Version))
			{
				FeedBack = reader.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_DryMix", DryMix);
			node.Add("m_WetMix1", WetMix1);
			node.Add("m_WetMix2", WetMix2);
			node.Add("m_WetMix3", WetMix3);
			node.Add("m_Delay", Delay);
			node.Add("m_Rate", Rate);
			node.Add("m_Depth", Depth);
			if (HasFeedBack(container.ExportVersion))
			{
				node.Add("m_FeedBack", FeedBack);
			}
			return node;
		}

		public float DryMix { get; set; }
		public float WetMix1 { get; set; }
		public float WetMix2 { get; set; }
		public float WetMix3 { get; set; }
		public float Delay { get; set; }
		public float Rate { get; set; }
		public float Depth { get; set; }
		public float FeedBack { get; set; }
	}
}
