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

			DryLevel = reader.ReadSingle();
			Room = reader.ReadSingle();
			RoomHF = reader.ReadSingle();
			if (HasRoomRolloff(reader.Version))
			{
				RoomRolloff = reader.ReadSingle();
			}
			DecayTime = reader.ReadSingle();
			DecayHFRatio = reader.ReadSingle();
			ReflectionsLevel = reader.ReadSingle();
			ReverbLevel = reader.ReadSingle();
			ReverbDelay = reader.ReadSingle();
			Diffusion = reader.ReadSingle();
			Density = reader.ReadSingle();
			HFReference = reader.ReadSingle();
			RoomLF = reader.ReadSingle();
			LFReference = reader.ReadSingle();
			ReflectionsDelay = reader.ReadSingle();
			ReverbPreset = (AudioReverbPreset)reader.ReadInt32();
		}

		public static bool HasRoomRolloff(UnityVersion version) => version.IsLessEqual(5, 4);

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("DryLevel", DryLevel);
			node.Add("Room", Room);
			node.Add("RoomHF", RoomHF);
			node.Add("DecayTime", DecayTime);
			node.Add("DecayHFRatio", DecayHFRatio);
			node.Add("ReflectionsLevel", ReflectionsLevel);
			node.Add("ReverbLevel", ReverbLevel);
			node.Add("ReverbDelay", ReverbDelay);
			node.Add("Diffusion", Diffusion);
			node.Add("Density", Density);
			node.Add("HfReference", HFReference);
			node.Add("RoomLF", RoomLF);
			node.Add("LfReference", LFReference);
			node.Add("ReflectionsDelay", ReflectionsDelay);
			node.Add("reverbPreset", (int)ReverbPreset);
			return node;
		}
		public float DryLevel { get; set; }
		public float Room { get; set; }
		public float RoomHF { get; set; }
		public float RoomRolloff { get; set; }
		public float DecayTime { get; set; }
		public float DecayHFRatio { get; set; }
		public float ReflectionsLevel { get; set; }
		public float ReverbLevel { get; set; }
		public float ReverbDelay { get; set; }
		public float Diffusion { get; set; }
		public float Density { get; set; }
		public float HFReference { get; set; }
		public float RoomLF { get; set; }
		public float LFReference { get; set; }
		public float ReflectionsDelay { get; set; }
		public AudioReverbPreset ReverbPreset { get; set; }
	}
}
