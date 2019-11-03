using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class AudioReverbZone : Behaviour
	{
		public AudioReverbZone(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool HasRoomRolloffFactor(Version version) => version.IsLess(5, 6);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

            MinDistance = reader.ReadSingle();
            MaxDistance = reader.ReadSingle();
            ReverbPreset = reader.ReadInt32();
            Room = reader.ReadInt32();
            RoomHF = reader.ReadInt32();
            DecayTime = reader.ReadSingle();
            DecayHFRatio = reader.ReadSingle();
            Reflections = reader.ReadInt32();
            ReflectionsDelay = reader.ReadSingle();
            Reverb = reader.ReadInt32();
            ReverbDelay = reader.ReadSingle();
            HFReference = reader.ReadSingle();
			if (HasRoomRolloffFactor(reader.Version))
			{
				RoomRolloffFactor = reader.ReadSingle();
			}
            Diffusion = reader.ReadSingle();
            Density = reader.ReadSingle();
            LFReference = reader.ReadSingle();
            RoomLF = reader.ReadInt32();
        }

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(MinDistanceName, MinDistance);
			node.Add(MaxDistanceName, MaxDistance);
			node.Add(ReverbPresetName, ReverbPreset);
			node.Add(RoomName, Room);
			node.Add(RoomHFName, RoomHF);
			node.Add(DecayTimeName, DecayTime);
			node.Add(DecayHFRatioName, DecayHFRatio);
			node.Add(ReflectionsName, Reflections);
			node.Add(ReflectionsDelayName, ReflectionsDelay);
			node.Add(ReverbName, Reverb);
			node.Add(ReverbDelayName, ReverbDelay);
			node.Add(HFReferenceName, HFReference);
			node.Add(DiffusionName, Diffusion);
			node.Add(DensityName, Density);
			node.Add(LFReferenceName, LFReference);
			node.Add(RoomLFName, RoomLF);
			return node;
		}
        
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public int ReverbPreset { get; set; }
        public int Room { get; set; }
        public int RoomHF { get; set; }
        public int RoomLF { get; set; }
        public float DecayTime { get; set; }
        public float DecayHFRatio { get; set; }
        public int Reflections { get; set; }
        public float ReflectionsDelay { get; set; }
        public int Reverb { get; set; }
        public float ReverbDelay { get; set; }
        public float HFReference { get; set; }
		public float RoomRolloffFactor { get; set; }
		public float LFReference { get; set; }
        public float Diffusion { get; set; }
        public float Density { get; set; }

		public const string MinDistanceName = "m_MinDistance";
		public const string MaxDistanceName = "m_MaxDistance";
		public const string ReverbPresetName = "m_ReverbPreset";
		public const string RoomName = "m_Room";
		public const string RoomHFName = "m_RoomHF";
		public const string DecayTimeName = "m_DecayTime";
		public const string DecayHFRatioName = "m_DecayHFRatio";
		public const string ReflectionsName = "m_Reflections";
		public const string ReflectionsDelayName = "m_ReflectionsDelay";
		public const string ReverbName = "m_Reverb";
		public const string ReverbDelayName = "m_ReverbDelay";
		public const string HFReferenceName = "m_HFReference";
		public const string DiffusionName = "m_Diffusion";
		public const string DensityName = "m_Density";
		public const string LFReferenceName = "m_LFReference";
		public const string RoomLFName = "m_RoomLF";
	}
}
