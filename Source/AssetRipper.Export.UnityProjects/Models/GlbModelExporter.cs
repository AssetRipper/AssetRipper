using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.Import.Logging;
using AssetRipper.Numerics;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_23;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Material;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using Microsoft.Win32.SafeHandles;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Buffers;

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

		private static IGameObject GetRoot(IUnityObjectBase asset)
		{
			return asset switch
			{
				IGameObject gameObject => gameObject.GetRoot(),
				IComponent component => component.GameObject_C2P!.GetRoot(),
				_ => throw new InvalidOperationException()
			};
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
			SceneBuilder sceneBuilder = new();
			BuildParameters parameters = new BuildParameters(isScene);

			HashSet<IUnityObjectBase> exportedAssets = new();

			foreach (IUnityObjectBase asset in assets)
			{
				if (!exportedAssets.Contains(asset) && asset is IGameObject or IComponent)
				{
					IGameObject root = GetRoot(asset);

					AddGameObjectToScene(sceneBuilder, parameters, null, Transformation.Identity, Transformation.Identity, root.GetTransform());

					foreach (IEditorExtension exportedAsset in root.FetchHierarchy())
					{
						exportedAssets.Add(exportedAsset);
					}
				}
			}

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

		private static void AddGameObjectToScene(SceneBuilder sceneBuilder, BuildParameters parameters, NodeBuilder? parentNode, Transformation parentGlobalTransform, Transformation parentGlobalInverseTransform, ITransform transform)
		{
			IGameObject? gameObject = transform.GameObject_C4P;
			if (gameObject is null)
			{
				return;
			}

			Transformation localTransform = transform.ToTransformation();
			Transformation localInverseTransform = transform.ToInverseTransformation();
			Transformation globalTransform = localTransform * parentGlobalTransform;
			Transformation globalInverseTransform = parentGlobalInverseTransform * localInverseTransform;

			NodeBuilder node = parentNode is null ? new NodeBuilder(gameObject.Name) : parentNode.CreateNode(gameObject.Name);
			if (parentNode is not null || parameters.IsScene)
			{
				node.LocalTransform = new SharpGLTF.Transforms.AffineTransform(
					transform.LocalScale_C4.CastToStruct(),//Scaling is the same in both coordinate systems
					GlbConversion.ToGltfQuaternionConvert(transform.LocalRotation_C4),
					GlbConversion.ToGltfVector3Convert(transform.LocalPosition_C4));
			}
			sceneBuilder.AddNode(node);

			if (gameObject.TryGetComponent(out IMeshFilter? meshFilter)
				&& meshFilter.TryGetMesh(out IMesh? mesh)
				&& mesh.IsSet()
				&& parameters.TryGetOrMakeMeshData(mesh, out MeshData meshData))
			{
				if (gameObject.TryGetComponent(out IMeshRenderer? meshRenderer))
				{
					if (ReferencesDynamicMesh(meshRenderer))
					{
						
						AddDynamicMeshToScene(sceneBuilder, parameters, node, meshData, new MaterialList(meshRenderer));
					}
					else
					{
						int[] subsetIndices = GetSubsetIndices(meshRenderer);
						AddStaticMeshToScene(sceneBuilder, parameters, node, meshData, subsetIndices, new MaterialList(meshRenderer), globalTransform, globalInverseTransform);
					}
				}
				else if (gameObject.TryGetComponent(out ISkinnedMeshRenderer? skinnedMeshRenderer))
				{
					if (ReferencesDynamicMesh(skinnedMeshRenderer))
					{
						AddDynamicMeshToScene(sceneBuilder, parameters, node, meshData, new MaterialList(skinnedMeshRenderer));
					}
					else
					{
						int[] subsetIndices = GetSubsetIndices(skinnedMeshRenderer);
						AddStaticMeshToScene(sceneBuilder, parameters, node, meshData, subsetIndices, new MaterialList(skinnedMeshRenderer), globalTransform, globalInverseTransform);
					}
				}
				else if (gameObject.TryGetComponent(out IRenderer? renderer))
				{
					Logger.Warning(LogCategory.Export, $"Renderer type {renderer.GetType()} not supported in {nameof(GlbModelExporter)}");
				}
			}

			foreach (ITransform childTransform in transform.Children_C4P.WhereNotNull())
			{
				AddGameObjectToScene(sceneBuilder, parameters, node, localTransform * parentGlobalTransform, parentGlobalInverseTransform * localInverseTransform, childTransform);
			}
		}

		private static void AddDynamicMeshToScene(SceneBuilder sceneBuilder, BuildParameters parameters, NodeBuilder node, MeshData meshData, MaterialList materialList)
		{
			AccessListBase<ISubMesh> subMeshes = meshData.Mesh.SubMeshes;
			(ISubMesh, MaterialBuilder)[] subMeshArray = ArrayPool<(ISubMesh, MaterialBuilder)>.Shared.Rent(subMeshes.Count);
			for (int i = 0; i < subMeshes.Count; i++)
			{
				MaterialBuilder materialBuilder = parameters.GetOrMakeMaterial(materialList[i]);
				subMeshArray[i] = (subMeshes[i], materialBuilder);
			}
			ArraySegment<(ISubMesh, MaterialBuilder)> arraySegment = new ArraySegment<(ISubMesh, MaterialBuilder)>(subMeshArray, 0, subMeshes.Count);
			IMeshBuilder<MaterialBuilder> subMeshBuilder = GlbSubMeshBuilder.BuildSubMeshes(arraySegment, meshData, Transformation.Identity, Transformation.Identity);
			sceneBuilder.AddRigidMesh(subMeshBuilder, node);
			ArrayPool<(ISubMesh, MaterialBuilder)>.Shared.Return(subMeshArray);
		}

		private static void AddStaticMeshToScene(SceneBuilder sceneBuilder, BuildParameters parameters, NodeBuilder node, MeshData meshData, int[] subsetIndices, MaterialList materialList, Transformation globalTransform, Transformation globalInverseTransform)
		{
			(ISubMesh, MaterialBuilder)[] subMeshArray = ArrayPool<(ISubMesh, MaterialBuilder)>.Shared.Rent(subsetIndices.Length);
			AccessListBase<ISubMesh> subMeshes = meshData.Mesh.SubMeshes;
			for (int i = 0; i < subsetIndices.Length; i++)
			{
				ISubMesh subMesh = subMeshes[subsetIndices[i]];
				MaterialBuilder materialBuilder = parameters.GetOrMakeMaterial(materialList[i]);
				subMeshArray[i] = (subMesh, materialBuilder);
			}
			ArraySegment<(ISubMesh, MaterialBuilder)> arraySegment = new ArraySegment<(ISubMesh, MaterialBuilder)>(subMeshArray, 0, subsetIndices.Length);
			IMeshBuilder<MaterialBuilder> subMeshBuilder = GlbSubMeshBuilder.BuildSubMeshes(arraySegment, meshData, globalInverseTransform, globalTransform);
			sceneBuilder.AddRigidMesh(subMeshBuilder, node);
			ArrayPool<(ISubMesh, MaterialBuilder)>.Shared.Return(subMeshArray);
		}

		private static bool ReferencesDynamicMesh(IMeshRenderer renderer)
		{
			return (renderer.Has_StaticBatchInfo() && renderer.StaticBatchInfo.SubMeshCount == 0)
				|| (renderer.Has_SubsetIndices() && renderer.SubsetIndices.Count == 0);
		}

		private static bool ReferencesDynamicMesh(ISkinnedMeshRenderer renderer)
		{
			return (renderer.Has_StaticBatchInfo() && renderer.StaticBatchInfo.SubMeshCount == 0)
				|| (renderer.Has_SubsetIndices() && renderer.SubsetIndices.Count == 0);
		}

		private static int[] GetSubsetIndices(IMeshRenderer renderer)
		{
			AccessListBase<IPPtr_Material> materials = renderer.Materials;
			if (renderer.Has_SubsetIndices())
			{
				return renderer.SubsetIndices.Select(i => (int)i).ToArray();
			}
			else if (renderer.Has_StaticBatchInfo())
			{
				return Enumerable.Range(renderer.StaticBatchInfo.FirstSubMesh, renderer.StaticBatchInfo.SubMeshCount).ToArray();
			}
			else
			{
				return Array.Empty<int>();
			}
		}

		private static int[] GetSubsetIndices(ISkinnedMeshRenderer renderer)
		{
			if (renderer.Has_SubsetIndices())
			{
				return renderer.SubsetIndices.Select(i => (int)i).ToArray();
			}
			else if (renderer.Has_StaticBatchInfo())
			{
				return Enumerable.Range(renderer.StaticBatchInfo.FirstSubMesh, renderer.StaticBatchInfo.SubMeshCount).ToArray();
			}
			else
			{
				return Array.Empty<int>();
			}
		}
	}
}
