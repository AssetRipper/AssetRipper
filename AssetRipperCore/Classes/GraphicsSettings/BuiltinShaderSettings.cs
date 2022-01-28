using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public sealed class BuiltinShaderSettings : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Mode = (BuiltinShaderMode)reader.ReadInt32();
			Shader.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Shader, ShaderName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ModeName, (int)Mode);
			node.Add(ShaderName, Shader.ExportYAML(container));
			return node;
		}

		public YAMLNode ExportYAML(IExportContainer container, string shaderName)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ModeName, (int)BuiltinShaderMode.Builtin);
			EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(shaderName, container.ExportVersion);
			node.Add(ShaderName, buildInAsset.ToExportPointer().ExportYAML(container));
			return node;
		}

		public BuiltinShaderMode Mode { get; set; }

		public const string ModeName = "m_Mode";
		public const string ShaderName = "m_Shader";

		public PPtr<Shader.Shader> Shader = new();
	}
}
