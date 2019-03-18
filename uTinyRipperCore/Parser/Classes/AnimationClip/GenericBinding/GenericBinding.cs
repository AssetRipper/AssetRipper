using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
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

		public void Read(AssetReader reader)
		{
			Path = reader.ReadUInt32();
			Attribute = reader.ReadUInt32();
			Script.Read(reader);

			if(IsInt32ID(reader.Version))
			{
				ClassID = (ClassIDType)reader.ReadInt32();
			}
			else
			{
				ClassID = (ClassIDType)reader.ReadUInt16();
			}

			CustomType = (BindingCustomType)reader.ReadByte();
			IsPPtrCurve = reader.ReadByte() == 0 ? false : true;
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("path", Path);
			node.Add("attribute", Attribute);
			node.Add("script", Script.ExportYAML(container));
			node.Add("classID", (int)ClassID);
			node.Add("customType", (byte)CustomType);
			node.Add("isPPtrCurve", IsPPtrCurve);
			return node;
		}

		public HumanoidMuscleType GetHumanoidMuscle(Version version)
		{
			return ((HumanoidMuscleType)Attribute).Update(version);
		}
		
		public bool IsTransform => ClassID == ClassIDType.Transform || ClassID == ClassIDType.RectTransform && TransformType.IsValid();
		public TransformType TransformType => unchecked((TransformType)Attribute);

		public uint Path { get; private set; }
		public uint Attribute { get; private set; }
		public ClassIDType ClassID { get; private set; }
		public BindingCustomType CustomType { get; private set; }
		public bool IsPPtrCurve { get; private set; }
		
		public PPtr<Object> Script;
	}
}
