using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes.LightmapSettings
{
	public sealed class LightingSettings : NamedObject
	{
		public LightingSettings(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GIWorkflowMode = reader.ReadInt32();
			EnableBakedLightmaps = reader.ReadBoolean();
			EnableRealtimeLightmaps = reader.ReadBoolean();
			EnableRealtimeEnvironmentLighting = reader.ReadBoolean();
			reader.AlignStream();

			BounceScale = reader.ReadSingle();
			AlbedoBoost = reader.ReadSingle();
			IndirectOutputScale = reader.ReadSingle();
			UsingShadowmask = reader.ReadBoolean();
			reader.AlignStream();
		}

		public int GIWorkflowMode { get; set; }
		public bool EnableBakedLightmaps { get; set; }
		public bool EnableRealtimeLightmaps { get; set; }
		public bool EnableRealtimeEnvironmentLighting { get; set; }
		public float BounceScale { get; set; }
		public float AlbedoBoost { get; set; }
		public float IndirectOutputScale { get; set; }
		public bool UsingShadowmask { get; set; }
	}
}
