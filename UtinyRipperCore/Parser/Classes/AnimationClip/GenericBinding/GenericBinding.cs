using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct GenericBinding : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 an greater
		/// </summary>
		private static bool IsInt32ID(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public void Read(AssetStream stream)
		{
			Path = stream.ReadUInt32();
			Attribute = stream.ReadUInt32();
			Script.Read(stream);

			if(IsInt32ID(stream.Version))
			{
				ClassID = (ClassIDType)stream.ReadInt32();
			}
			else
			{
				ClassID = (ClassIDType)stream.ReadUInt16();
			}

			CustomType = stream.ReadByte();
			IsPPtrCurve = stream.ReadByte();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("path", Path);
			node.Add("attribute", Attribute);
			node.Add("script", Script.ExportYAML(container));
			node.Add("classID", (int)ClassID);
			node.Add("customType", CustomType);
			node.Add("isPPtrCurve", IsPPtrCurve);
			return node;
		}
		
		public BindingType BindingType => (BindingType)(Attribute);

		public uint Path { get; private set; }
		public uint Attribute { get; private set; }
		public ClassIDType ClassID { get; private set; }
		public byte CustomType { get; private set; }
		public byte IsPPtrCurve { get; private set; }
		
		public PPtr<Object> Script;
	}
}
