using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AudioReverbFilter
{
	public sealed class AudioReverbFilter : AudioBehaviour
	{
		public AudioReverbFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_DryLevel = reader.ReadSingle();
			m_Room = reader.ReadSingle();
			m_RoomHF = reader.ReadSingle();
			if (HasRoomRolloff(reader.Version))
			{
				m_RoomRolloff = reader.ReadSingle();
			}
			m_DecayTime = reader.ReadSingle();
			m_DecayHFRatio = reader.ReadSingle();
			m_ReflectionsLevel = reader.ReadSingle();
			m_ReverbLevel = reader.ReadSingle();
			m_ReverbDelay = reader.ReadSingle();
			m_Diffusion = reader.ReadSingle();
			m_Density = reader.ReadSingle();
			m_HFReference = reader.ReadSingle();
			m_RoomLF = reader.ReadSingle();
			m_LFReference = reader.ReadSingle();
			m_ReflectionsDelay = reader.ReadSingle();
			m_ReverbPreset = (AudioReverbPreset)reader.ReadInt32();
		}

		public static bool HasRoomRolloff(UnityVersion version) => version.IsLessEqual(5, 6);

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("DryLevel", m_DryLevel);
			node.Add("Room", m_Room);
			node.Add("RoomHF", m_RoomHF);
			node.Add("DecayTime", m_DecayTime);
			node.Add("DecayHFRatio", m_DecayHFRatio);
			node.Add("ReflectionsLevel", m_ReflectionsLevel);
			node.Add("ReverbLevel", m_ReverbLevel);
			node.Add("ReverbDelay", m_ReverbDelay);
			node.Add("Diffusion", m_Diffusion);
			node.Add("Density", m_Density);
			node.Add("HfReference", m_HFReference);
			node.Add("RoomLF", m_RoomLF);
			node.Add("LfReference", m_LFReference);
			node.Add("ReflectionsDelay", m_ReflectionsDelay);
			node.Add("reverbPreset", (int)m_ReverbPreset);
			return node;
		}
		public float m_DryLevel { get; set; }
		public float m_Room { get; set; }
		public float m_RoomHF { get; set; }
		public float m_RoomRolloff { get; set; }
		public float m_DecayTime { get; set; }
		public float m_DecayHFRatio { get; set; }
		public float m_ReflectionsLevel { get; set; }
		public float m_ReverbLevel { get; set; }
		public float m_ReverbDelay { get; set; }
		public float m_Diffusion { get; set; }
		public float m_Density { get; set; }
		public float m_HFReference { get; set; }
		public float m_RoomLF { get; set; }
		public float m_LFReference { get; set; }
		public float m_ReflectionsDelay { get; set; }
		public AudioReverbPreset m_ReverbPreset { get; set; }
	}
}
