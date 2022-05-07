﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimationClip.GenericBinding
{
	public sealed class GenericBinding : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// 5.6.0 an greater
		/// </summary>
		private static bool IsInt32ID(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			Path = reader.ReadUInt32();
			Attribute = reader.ReadUInt32();
			Script.Read(reader);

			if (IsInt32ID(reader.Version))
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
				reader.AlignStream();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(PathName, Path);
			node.Add(AttributeName, Attribute);
			node.Add(ScriptName, Script.ExportYaml(container));
			node.Add(ClassIDName, (int)ClassID);
			node.Add(CustomTypeName, (byte)CustomType);
			node.Add(IsPPtrCurveName, IsPPtrCurve);
			return node;
		}

		public HumanoidMuscleType GetHumanoidMuscle(UnityVersion version)
		{
			return ((HumanoidMuscleType)Attribute).Update(version);
		}

		public bool IsTransform => ClassID == ClassIDType.Transform || ClassID == ClassIDType.RectTransform && TransformType.IsValid();
		public TransformType TransformType => unchecked((TransformType)Attribute);

		public uint Path { get; set; }
		public uint Attribute { get; set; }
		public ClassIDType ClassID { get; set; }
		public BindingCustomType CustomType { get; set; }
		public bool IsPPtrCurve { get; set; }

		public const string PathName = "path";
		public const string AttributeName = "attribute";
		public const string ScriptName = "script";
		public const string ClassIDName = "classID";
		public const string CustomTypeName = "customType";
		public const string IsPPtrCurveName = "isPPtrCurve";

		public PPtr<Object.Object> Script = new();
	}
}
