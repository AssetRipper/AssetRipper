using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Shaders
{
	/// <summary>
	/// An exporter for exporting shaders as unity assets. Shader.Find will not work in the Unity Editor with this exporter.
	/// </summary>
	public class YamlShaderExporter : YamlExporterBase
	{
		private readonly ShaderExportMode exportMode;

		public YamlShaderExporter(LibraryConfiguration configuration)
		{
			exportMode = configuration.ShaderExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return exportMode is ShaderExportMode.Yaml && asset is IShader;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "asset");
		}
	}
}
