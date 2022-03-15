using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Renderer;
using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core
{
	public static class StaticMeshConverter
	{
		public static void MaybeReplaceStaticMesh(IUnityObjectBase unityObjectBase, SerializedFile serializedFile, VirtualSerializedFile virtualSerializedFile)
		{
			if (unityObjectBase is not MeshRenderer meshRenderer)
				return;

			IStaticBatchInfo staticBatchInfo = meshRenderer.GetStaticBatchInfo(serializedFile.Version);
			if (staticBatchInfo.SubMeshCount == 0)
				return;

			IGameObject gameObject = meshRenderer.TryGetGameObject();
			if (gameObject == null || string.IsNullOrEmpty(gameObject.Name))
				return;

			IMeshFilter meshFilter = gameObject.FindComponent<IMeshFilter>();
			if (meshFilter == null)
				return;

			Mesh mesh = (Mesh)meshFilter.Mesh.FindAsset(serializedFile);
			if (mesh == null)
				return;

			IExportContainer container = new DummyExportContainer(serializedFile, virtualSerializedFile);

			Mesh newMesh = (Mesh)mesh.ConvertLegacy(container);
			virtualSerializedFile.AddAsset(newMesh, ClassIDType.Mesh);
			newMesh.Name = new string(gameObject.Name);

			Logging.Logger.Info($"Created {newMesh.Name} from static mesh {mesh.Name}");

			SubMesh[] subMeshes = mesh.SubMeshes.AsSpan().Slice(staticBatchInfo.FirstSubMesh, staticBatchInfo.SubMeshCount).ToArray();
			newMesh.SubMeshes = SubMeshConverter.Convert(container, newMesh, subMeshes);

			meshFilter.Mesh.CopyValues(virtualSerializedFile.CreatePPtr(newMesh));
			meshRenderer.StaticBatchInfo.FirstSubMesh = 0;
			meshRenderer.StaticBatchInfo.SubMeshCount = 0;
		}

		private class DummyExportContainer : IExportContainer
		{
			public DummyExportContainer(SerializedFile serializedFile, VirtualSerializedFile virtualSerializedFile)
			{
				ExportLayout = virtualSerializedFile.Layout;
				Layout = serializedFile.Layout;
			}

			public IExportCollection CurrentCollection => throw new NotImplementedException();

			public LayoutInfo ExportLayout { get; }

			public UnityVersion ExportVersion => ExportLayout.Version;

			public Platform ExportPlatform => ExportLayout.Platform;

			public TransferInstructionFlags ExportFlags => ExportLayout.Flags;

			public string Name => throw new NotImplementedException();

			public LayoutInfo Layout { get; }

			public UnityVersion Version => Layout.Version;

			public Platform Platform => Layout.Platform;

			public TransferInstructionFlags Flags => Layout.Flags;

			public IReadOnlyList<FileIdentifier> Dependencies => throw new NotImplementedException();

			public MetaPtr CreateExportPointer(IUnityObjectBase asset)
			{
				throw new NotImplementedException();
			}

			public IUnityObjectBase FindAsset(int fileIndex, long pathID)
			{
				throw new NotImplementedException();
			}

			public IUnityObjectBase FindAsset(ClassIDType classID)
			{
				throw new NotImplementedException();
			}

			public IUnityObjectBase FindAsset(ClassIDType classID, string name)
			{
				throw new NotImplementedException();
			}

			public IUnityObjectBase GetAsset(long pathID)
			{
				throw new NotImplementedException();
			}

			public IUnityObjectBase GetAsset(int fileIndex, long pathID)
			{
				throw new NotImplementedException();
			}

			public ClassIDType GetAssetType(long pathID)
			{
				throw new NotImplementedException();
			}

			public long GetExportID(IUnityObjectBase asset)
			{
				throw new NotImplementedException();
			}

			public bool IsSceneDuplicate(int sceneID)
			{
				throw new NotImplementedException();
			}

			public string SceneIndexToName(int sceneID)
			{
				throw new NotImplementedException();
			}

			public string TagIDToName(int tagID)
			{
				throw new NotImplementedException();
			}

			public ushort TagNameToID(string tagName)
			{
				throw new NotImplementedException();
			}

			public AssetType ToExportType(ClassIDType classID)
			{
				throw new NotImplementedException();
			}
		}
	}
}
