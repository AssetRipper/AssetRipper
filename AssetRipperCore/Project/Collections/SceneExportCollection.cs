using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.OcclusionCullingData;
using AssetRipper.Core.Classes.OcclusionCullingSettings;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class SceneExportCollection : ExportCollection, IComparer<UnityObjectBase>
	{
		public SceneExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, ISerializedFile file)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			AssetExporter = assetExporter;
			Name = file.Name;
			m_file = file;

			List<UnityObjectBase> components = new List<UnityObjectBase>();
			foreach (UnityObjectBase asset in file.FetchAssets())
			{
				if (IsSceneCompatible(asset))
				{
					components.Add(asset);
					m_cexportIDs.Add(asset.AssetInfo, asset.PathID);
				}
			}
			m_components = components.OrderBy(t => t, this).ToArray();

			if (OcclusionCullingSettings.HasSceneGUID(file.Version))
			{
				OcclusionCullingSettings sceneSettings = Components.Where(t => t.ClassID == ClassIDType.OcclusionCullingSettings).Select(t => (OcclusionCullingSettings)t).FirstOrDefault();
				if (sceneSettings != null)
				{
					GUID = sceneSettings.SceneGUID;
				}
			}
			if (GUID.IsZero)
			{
				GUID = new UnityGUID(Guid.NewGuid());
			}

			if (OcclusionCullingSettings.HasReadPVSData(File.Version))
			{
				foreach (UnityObjectBase comp in Components)
				{
					if (comp.ClassID == ClassIDType.OcclusionCullingSettings)
					{
						OcclusionCullingSettings settings = (OcclusionCullingSettings)comp;
						if (settings.PVSData.Length > 0)
						{
							m_occlusionCullingSettings = settings;
							OcclusionCullingData = OcclusionCullingData.CreateVirtualInstance(virtualFile);
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasMainData(UnityVersion version) => version.IsLess(5, 3);

		public static string SceneIndexToFileName(int index, UnityVersion version)
		{
			if (HasMainData(version))
			{
				if (index == 0)
				{
					return MainSceneName;
				}
				return LevelName + (index - 1).ToString();
			}
			return LevelName + index.ToString();
		}

		public static int FileNameToSceneIndex(string name, UnityVersion version)
		{
			if (HasMainData(version))
			{
				if (name == MainSceneName)
				{
					return 0;
				}

				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr) + 1;
			}
			else
			{
				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr);
			}
		}

		public override bool Export(IProjectAssetContainer container, string dirPath)
		{
			string folderPath = Path.Combine(dirPath, UnityObjectBase.AssetsKeyword, OcclusionCullingSettings.SceneKeyword);
			string sceneSubPath = GetSceneName(container);
			string fileName = $"{sceneSubPath}.unity";
			string filePath = Path.Combine(folderPath, fileName);

			if (IsDuplicate(container))
			{
				if (System.IO.File.Exists(filePath))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Duplicate scene '{sceneSubPath}' has been found. Skipping");
					return false;
				}
			}

			folderPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(folderPath))
			{
				DirectoryUtils.CreateVirtualDirectory(folderPath);
			}

			AssetExporter.Export(container, Components.Select(t => t.Convert(container)), filePath);
			DefaultImporter sceneImporter = new DefaultImporter(container.ExportLayout);
			Meta meta = new Meta(GUID, sceneImporter);
			ExportMeta(container, meta, filePath);

			string sceneName = Path.GetFileName(sceneSubPath);
			string subFolderPath = Path.Combine(folderPath, sceneName);
			if (OcclusionCullingData != null)
			{
				OcclusionCullingData.Initialize(container, m_occlusionCullingSettings);
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}

			return true;
		}

		public override bool IsContains(UnityObjectBase asset)
		{
			if (asset == OcclusionCullingData)
			{
				return true;
			}
			return m_cexportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(UnityObjectBase asset)
		{
			return IsComponent(asset) ? m_cexportIDs[asset.AssetInfo] : ExportIdHandler.GetMainExportID(asset);
		}

		public override MetaPtr CreateExportPointer(UnityObjectBase asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			if (isLocal && IsComponent(asset))
			{
				return new MetaPtr(exportID);
			}
			else
			{
				UnityGUID guid = IsComponent(asset) ? GUID : asset.GUID;
				return new MetaPtr(exportID, guid, AssetType.Serialized);
			}
		}

		public int Compare(UnityObjectBase obj1, UnityObjectBase obj2)
		{
			if (obj1.ClassID == obj2.ClassID)
			{
				return 0;
			}

			if (obj1.ClassID.IsSceneSettings())
			{
				if (obj2.ClassID.IsSceneSettings())
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (obj2.ClassID.IsSceneSettings())
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		private void ExportAsset(IProjectAssetContainer container, NamedObject asset, string path)
		{
			NativeFormatImporter importer = new NativeFormatImporter(container.ExportLayout);
			importer.MainObjectFileID = GetExportID(asset);
			ExportAsset(container, importer, asset, path, asset.Name);
		}

		private bool IsComponent(UnityObjectBase asset)
		{
			return asset != OcclusionCullingData;
		}

		private string GetSceneName(IExportContainer container)
		{
			if (IsSceneName)
			{
				int index = FileNameToSceneIndex(Name, File.Version);
				string scenePath = container.SceneIndexToName(index);
				if (scenePath.StartsWith(AssetsName, StringComparison.Ordinal))
				{
					string extension = Path.GetExtension(scenePath);
					return scenePath.Substring(AssetsName.Length, scenePath.Length - AssetsName.Length - extension.Length);
				}
				else if (Path.IsPathRooted(scenePath))
				{
					// pull/617
					// NOTE: absolute project path may contain Assets/ in its name so in this case we get incorrect scene path, but there is no way to bypass this issue
					int assetIndex = scenePath.IndexOf(AssetsName);
					string extension = Path.GetExtension(scenePath);
					return scenePath.Substring(assetIndex + AssetsName.Length, scenePath.Length - assetIndex - AssetsName.Length - extension.Length);
				}
				else if (scenePath.Length == 0)
				{
					// if you build a game without included scenes, Unity create one with empty name
					return Name;
				}
				else
				{
					return scenePath;
				}
			}
			return Name;
		}

		private bool IsDuplicate(IExportContainer container)
		{
			if (IsSceneName)
			{
				int index = FileNameToSceneIndex(Name, File.Version);
				return container.IsSceneDuplicate(index);
			}
			return false;
		}

		private bool IsSceneName => Name == MainSceneName || s_sceneNameFormat.IsMatch(Name);

		public override IAssetExporter AssetExporter { get; }
		public override IEnumerable<UnityObjectBase> Assets
		{
			get
			{
				foreach (UnityObjectBase asset in Components)
				{
					yield return asset;
				}
				if (OcclusionCullingData != null)
				{
					yield return OcclusionCullingData;
				}
			}
		}

		/// <summary>
		/// GameObject, Classes Inherited From Level Game Manager, Monobehaviours with GameObjects, Components
		/// </summary>
		public static bool IsSceneCompatible(UnityObjectBase asset)
		{
			if (asset is IGameObject)
			{
				return true;
			}
			if (asset is ILevelGameManager)
			{
				return true;
			}
			if (asset is IMonoBehaviour)
			{
				MonoBehaviour monoBeh = (MonoBehaviour)asset;
				if (!monoBeh.IsSceneObject)
				{
					return false;
				}
			}

			return asset is IComponent;
		}

		public override string Name { get; }
		public override ISerializedFile File => m_file;

		public OcclusionCullingData OcclusionCullingData { get; }
		public UnityGUID GUID { get; }

		private IEnumerable<UnityObjectBase> Components => m_components;

		private const string AssetsName = "Assets/";
		private const string LevelName = "level";
		private const string MainSceneName = "maindata";

		private static readonly Regex s_sceneNameFormat = new Regex($"^{LevelName}(0|[1-9][0-9]*)$");

		private readonly UnityObjectBase[] m_components;
		private readonly Dictionary<AssetInfo, long> m_cexportIDs = new Dictionary<AssetInfo, long>();
		private readonly ISerializedFile m_file;
		private readonly OcclusionCullingSettings m_occlusionCullingSettings;
	}
}
