using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;
using AssetRipper.Processing.Scenes;
using AssetRipper.SourceGenerated.Classes.ClassID_1030;
using AssetRipper.SourceGenerated.Classes.ClassID_3;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class SceneExportCollection : ExportCollection, IComparer<IUnityObjectBase>
	{
		public SceneExportCollection(IAssetExporter assetExporter, SceneDefinition scene)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			Scene = scene ?? throw new ArgumentNullException(nameof(scene));
			CurrentFile = scene.Collections[0];//Have to set it to something.

			List<IUnityObjectBase> components = new();
			foreach (IUnityObjectBase asset in Scene.Collections.SelectMany(c => c))
			{
				if (SceneHelpers.IsSceneCompatible(asset))
				{
					components.Add(asset);
					m_exportIDs.Add(asset.AssetInfo, asset.PathID);
				}
			}
			components.Sort(this);
			componentArray = components.ToArray();
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string filePath = Path.Combine(projectDirectory, $"{Scene.Path}.{ExportExtension}");
			string folderPath = Path.GetDirectoryName(filePath)!;

			if (SceneHelpers.IsDuplicate(container, File))
			{
				if (System.IO.File.Exists(filePath))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Duplicate scene '{Scene.Path}' has been found. Skipping");
					return false;
				}
			}

			Directory.CreateDirectory(folderPath);
			return ExportScene(container, folderPath, filePath, Scene.Name);
		}

		protected virtual bool ExportScene(IExportContainer container, string folderPath, string filePath, string sceneName)
		{
			AssetExporter.Export(container, Assets, filePath);
			IDefaultImporter sceneImporter = DefaultImporter.Create(container.File, container.ExportVersion);
			Meta meta = new Meta(GUID, sceneImporter);
			ExportMeta(container, meta, filePath);
			return true;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return m_exportIDs[asset.AssetInfo];
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			if (isLocal)
			{
				return new MetaPtr(exportID);
			}
			else
			{
				return new MetaPtr(exportID, GUID, AssetType.Serialized);
			}
		}

		public int Compare(IUnityObjectBase? obj1, IUnityObjectBase? obj2)
		{
			if (obj1?.ClassID == obj2?.ClassID)
			{
				return 0;
			}

			if (obj1 is ILevelGameManager)
			{
				if (obj2 is ILevelGameManager)
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else if (obj2 is ILevelGameManager)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				foreach (IUnityObjectBase asset in componentArray)
				{
					CurrentFile = asset.Collection;
					yield return asset;
				}
			}
		}

		public virtual string ExportExtension => "unity";

		/// <summary>
		/// The <see cref="SceneDefinition.Name"/> of <see cref="Scene"/>.
		/// </summary>
		public override string Name => Scene.Name;

		/// <summary>
		/// The <see cref="AssetCollection.Name"/> of the first <see cref="SerializedAssetCollection"/> in <see cref="SceneDefinition.Collections"/>.
		/// </summary>
		public string? FileName => Scene.Collections.FirstOrDefault(c => c is SerializedAssetCollection)?.Name;

		public override AssetCollection File => CurrentFile;
		public UnityGuid GUID => Scene.GUID;
		public override IAssetExporter AssetExporter { get; }
		public SceneDefinition Scene { get; }
		private AssetCollection CurrentFile { get; set; }

		private readonly IUnityObjectBase[] componentArray;
		private readonly Dictionary<AssetInfo, long> m_exportIDs = new();
	}
}
