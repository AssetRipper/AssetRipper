using uTinyRipper.AssetExporters;
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
		public static bool IsReadRoomRolloffFactor(Version version)
		{
			return version.IsLess(5, 6);
		}

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
			if (IsReadRoomRolloffFactor(reader.Version))
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
            node.Add("m_MinDistance", MinDistance);
            node.Add("m_MaxDistance", MaxDistance);
            node.Add("m_ReverbPreset", ReverbPreset);
            node.Add("m_Room", Room);
            node.Add("m_RoomHF", RoomHF);
            node.Add("m_DecayTime", DecayTime);
            node.Add("m_DecayHFRatio", DecayHFRatio);
            node.Add("m_Reflections", Reflections);
            node.Add("m_ReflectionsDelay", ReflectionsDelay);
            node.Add("m_Reverb", Reverb);
            node.Add("m_ReverbDelay", ReverbDelay);
            node.Add("m_HFReference", HFReference);
            node.Add("m_Diffusion", Diffusion);
            node.Add("m_Density", Density);
            node.Add("m_LFReference", LFReference);
            node.Add("m_RoomLF", RoomLF);
            return node;
		}
        
        public float MinDistance { get; private set; }
        public float MaxDistance { get; private set; }
        public int ReverbPreset { get; private set; }
        public int Room { get; private set; }
        public int RoomHF { get; private set; }
        public int RoomLF { get; private set; }
        public float DecayTime { get; private set; }
        public float DecayHFRatio { get; private set; }
        public int Reflections { get; private set; }
        public float ReflectionsDelay { get; private set; }
        public int Reverb { get; private set; }
        public float ReverbDelay { get; private set; }
        public float HFReference { get; private set; }
		public float RoomRolloffFactor { get; private set; }
		public float LFReference { get; private set; }
        public float Diffusion { get; private set; }
        public float Density { get; private set; }
	}
}
