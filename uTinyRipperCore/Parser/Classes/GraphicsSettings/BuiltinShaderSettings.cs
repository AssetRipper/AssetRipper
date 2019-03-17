using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct BuiltinShaderSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Mode = (BuiltinShaderMode)reader.ReadInt32();
			Shader.Read(reader);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Shader.FetchDependency(file, isLog, () => nameof(BuiltinShaderSettings), "m_Shader");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Mode", (int)Mode);
			node.Add("m_Shader", Shader.ExportYAML(container));
			return node;
		}

		public YAMLNode ExportYAML(IExportContainer container, string shaderName)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Mode", (int)BuiltinShaderMode.Builtin);
			EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.Shaders[shaderName];
			ExportPointer pointer = new ExportPointer(buildInAsset.ExportID, buildInAsset.GUID, AssetType.Internal);
			node.Add("m_Shader", pointer.ExportYAML(container));
			return node;
		}

		public BuiltinShaderMode Mode { get; private set; }

		public PPtr<Shader> Shader;

		private static readonly EngineGUID FGUID = new EngineGUID(0x00000000, 0xF0000000, 0x00000000, 0x00000000);
	}
}
