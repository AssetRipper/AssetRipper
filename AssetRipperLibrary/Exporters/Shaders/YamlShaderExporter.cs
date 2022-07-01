using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Library.Exporters.Shaders
{
	/// <summary>
	/// An exporter for exporting shaders as unity assets. Shader.Find will not work in the Unity Editor with this exporter.
	/// </summary>
	public sealed class YamlShaderExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset) => asset is IShader;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new YamlShaderExportCollection(this, asset);
		}
	}
}
