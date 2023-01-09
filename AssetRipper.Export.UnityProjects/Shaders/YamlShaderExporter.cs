using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	/// <summary>
	/// An exporter for exporting shaders as unity assets. Shader.Find will not work in the Unity Editor with this exporter.
	/// </summary>
	public sealed class YamlShaderExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset) => asset is IShader;

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new YamlShaderExportCollection(this, asset);
		}
	}
}
