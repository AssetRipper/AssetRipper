using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct EffectConstant : IAssetReadable, IYAMLExportable
	{
		/*public static int ToSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			Type = reader.ReadInt32();
			GroupConstantIndex = reader.ReadUInt32();
			SendTargetEffectIndex = reader.ReadUInt32();
			WetMixLevelIndex = reader.ReadUInt32();
			PrevEffectIndex = reader.ReadUInt32();
			Bypass = reader.ReadBoolean();
			reader.AlignStream();
			
			ParameterIndices = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(TypeName, Type);
			node.Add(GroupConstantIndexName, GroupConstantIndex);
			node.Add(SendTargetEffectIndexName, SendTargetEffectIndex);
			node.Add(WetMixLevelIndexName, WetMixLevelIndex);
			node.Add(PrevEffectIndexName, PrevEffectIndex);
			node.Add(BypassName, Bypass);
			node.Add(ParameterIndicesName, ParameterIndices.ExportYAML(true));
			return node;
		}

		public int Type { get; set; }
		public uint GroupConstantIndex { get; set; }
		public uint SendTargetEffectIndex { get; set; }
		public uint WetMixLevelIndex { get; set; }
		public uint PrevEffectIndex { get; set; }
		public bool Bypass { get; set; }
		public uint[] ParameterIndices { get; set; }

		public const string TypeName = "type";
		public const string GroupConstantIndexName = "groupConstantIndex";
		public const string SendTargetEffectIndexName = "sendTargetEffectIndex";
		public const string WetMixLevelIndexName = "wetMixLevelIndex";
		public const string PrevEffectIndexName = "prevEffectIndex";
		public const string BypassName = "bypass";
		public const string ParameterIndicesName = "parameterIndices";
	}
}
