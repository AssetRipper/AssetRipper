using AssetRipper.Core.Classes;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project.Exporters
{
	public class YAMLAssetExporter : IAssetExporter
	{
		public bool IsHandle(UnityObjectBase asset)
		{
			return true;
		}

		public bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (InvariantStreamWriter streamWriter = new InvariantStreamWriter(stream, UTF8))
					{
						YAMLWriter writer = new YAMLWriter();
						YAMLDocument doc = (asset as Object).ExportYAMLDocument(container);
						writer.AddDocument(doc);
						writer.Write(streamWriter);
					}
				}
			}
			return true;
		}

		public void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (InvariantStreamWriter streamWriter = new InvariantStreamWriter(stream, UTF8))
					{
						YAMLWriter writer = new YAMLWriter();
						writer.WriteHead(streamWriter);
						foreach (Object asset in assets)
						{
							YAMLDocument doc = asset.ExportYAMLDocument(container);
							writer.WriteDocument(doc);
						}
						writer.WriteTail(streamWriter);
					}
				}
			}
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			throw new NotSupportedException("YAML supports only single file export");
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			if (SceneExportCollection.IsSceneCompatible(asset))
			{
				if (asset.File.Collection.IsScene(asset.File))
				{
					return new SceneExportCollection(this, virtualFile, asset.File);
				}
				else if (PrefabExportCollection.IsValidAsset(asset))
				{
					return new PrefabExportCollection(this, virtualFile, asset);
				}
				else
				{
					return new FailExportCollection(this, asset);
				}
			}
			else
			{
				switch (asset.ClassID)
				{
					case ClassIDType.AnimatorController:
						return new AnimatorControllerExportCollection(this, virtualFile, asset);

					case ClassIDType.TimeManager:
					case ClassIDType.AudioManager:
					case ClassIDType.InputManager:
					case ClassIDType.Physics2DSettings:
					case ClassIDType.GraphicsSettings:
					case ClassIDType.QualitySettings:
					case ClassIDType.PhysicsManager:
					case ClassIDType.TagManager:
					case ClassIDType.NavMeshProjectSettings:
					case ClassIDType.NetworkManager:
					case ClassIDType.ClusterInputManager:
					case ClassIDType.UnityConnectSettings:
						return new ManagerExportCollection(this, asset);
					case ClassIDType.BuildSettings:
						return new BuildSettingsExportCollection(this, virtualFile, asset);

					case ClassIDType.MonoBehaviour:
						{
							MonoBehaviour monoBehaviour = (MonoBehaviour)asset;
							if (monoBehaviour.IsScriptableObject)
							{
								return new AssetExportCollection(this, asset);
							}
							else
							{
								// such MonoBehaviours as StateMachineBehaviour in AnimatorController
								return new EmptyExportCollection();
							}
						}

					default:
						return new AssetExportCollection(this, asset);
				}
			}
		}

		public AssetType ToExportType(UnityObjectBase asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Serialized;
			return true;
		}

		private static readonly Encoding UTF8 = new UTF8Encoding(false);
	}
}
