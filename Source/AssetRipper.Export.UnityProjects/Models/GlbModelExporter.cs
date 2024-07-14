using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using Microsoft.Win32.SafeHandles;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.UnityProjects.Models
{
	public partial class GlbModelExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			switch (asset.MainAsset)
			{
				case SceneHierarchyObject sceneHierarchyObject:
					exportCollection = new GlbSceneModelExportCollection(this, sceneHierarchyObject);
					return true;
				case PrefabHierarchyObject prefabHierarchyObject:
					exportCollection = new GlbPrefabModelExportCollection(this, prefabHierarchyObject);
					return true;
				default:
					exportCollection = null;
					return false;
			}
		}

		public override bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			return ExportModel(assets, path, false); //Called by the prefab exporter
		}

		public static bool ExportModel(IEnumerable<IUnityObjectBase> assets, string path, bool isScene)
		{
			ReadOnlySpan<byte> data = ExportBinary(assets, isScene);
			if (data.Length == 0)
			{
				return false;
			}

			WriteAllBytes(path, data);
			return true;
		}

		private static void WriteAllBytes(string path, ReadOnlySpan<byte> data)
		{
			ArgumentException.ThrowIfNullOrEmpty(path);

			using SafeFileHandle sfh = File.OpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			RandomAccess.Write(sfh, data, 0);
		}

		private static ArraySegment<byte> ExportBinary(IEnumerable<IUnityObjectBase> assets, bool isScene)
		{
			SceneBuilder sceneBuilder = GlbLevelBuilder.Build(assets, isScene);

			SharpGLTF.Schema2.WriteSettings writeSettings = new();

			try
			{
				return sceneBuilder.ToGltf2().WriteGLB(writeSettings);
			}
			catch (InvalidOperationException ex) when (ex.Message == "Can't merge a buffer larger than 2Gb")
			{
				Logger.Error(LogCategory.Export, $"Model was too large to export as GLB.");
				return default;
			}
		}
	}
}
