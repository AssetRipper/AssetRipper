using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.Numerics;
using AssetRipper.Processing.PrefabOutlining;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace AssetRipper.Processing.StaticMeshes;

public sealed class StaticMeshProcessor : IAssetProcessor
{
	private const double MaxMeshDeviation = 0.1;

	private readonly Dictionary<IMesh, MeshData> combinedMeshDictionary = new();
	private readonly HashSet<IMesh> badMeshSet = new();
	private readonly Dictionary<string, LinkedList<IMesh>> meshNameDictionary = new(StringComparer.Ordinal);
	private readonly Dictionary<string, List<(IMesh Mesh, IRenderer Renderer, IMeshFilter MeshFilter, ITransform Transform)>> cleanNameToParts = new(StringComparer.Ordinal);

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Separate Static Meshes");
		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Separated Static Meshes");

		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			if (asset is IMesh mesh)
			{
				GetOrCreateMeshList(mesh.Name).AddLast(mesh);
			}
			else if (TryGetStaticMeshInformation(asset, out string? cleanName, out (IMesh, IRenderer, IMeshFilter, ITransform) parts))
			{
				GetOrCreatePartList(cleanName).Add(parts);
			}
		}

		Logger.Info(LogCategory.Processing, $"Found {cleanNameToParts.Count} meshes combined into static batches");
		foreach ((string cleanName, List<(IMesh Mesh, IRenderer Renderer, IMeshFilter MeshFilter, ITransform Transform)> parts) in cleanNameToParts)
		{
			Logger.Info(LogCategory.Processing, $"Attempting to recreate {cleanName} from {parts.Count} instances");
			LinkedList<LinkedList<(MeshData MeshData, IRenderer Renderer, IMeshFilter MeshFilter)>> boxes = new();

			foreach ((IMesh combinedMesh, IRenderer renderer, IMeshFilter meshFilter, ITransform transform) in parts)
			{
				if (!TryGetOrMakeCombinedMeshData(combinedMesh, out MeshData combinedMeshData))
				{
					continue;
				}

				MeshData instanceMeshData = MakeMeshDataFromSubMeshes(renderer, combinedMeshData);
				CreateTransformations(transform, out Transformation transformation, out Transformation inverseTransformation);
				instanceMeshData = ApplyTransformationToMeshData(instanceMeshData, transformation, inverseTransformation);

				LinkedList<(MeshData, IRenderer, IMeshFilter)>? matchingBox = null;
				foreach (LinkedList<(MeshData, IRenderer, IMeshFilter)> box in boxes)
				{
					if (box.All(entry => AreApproximatelyEqual(entry.Item1, instanceMeshData)))
					{
						matchingBox = box;
						break;
					}
				}

				if (matchingBox is null)
				{
					matchingBox = new LinkedList<(MeshData, IRenderer, IMeshFilter)>();
					boxes.AddLast(matchingBox);
				}

				matchingBox.AddLast((instanceMeshData, renderer, meshFilter));
			}

			Logger.Info(LogCategory.Processing, $"Separated instances of {cleanName} into {boxes.Count} boxes");
			foreach (LinkedList<(MeshData MeshData, IRenderer Renderer, IMeshFilter MeshFilter)> box in boxes)
			{
				IMesh mesh = FindExistingMesh(cleanName, box) ?? MakeMeshFromBox(cleanName, box.First!.Value.MeshData, processedCollection);
				foreach ((_, IRenderer renderer, IMeshFilter meshFilter) in box)
				{
					meshFilter.MeshP = mesh;
					renderer.ClearStaticBatchInfo();
					renderer.MarkGameObjectAsStatic();
				}
			}
		}

		combinedMeshDictionary.Clear();
		badMeshSet.Clear();
		meshNameDictionary.Clear();
		cleanNameToParts.Clear();
	}

	private LinkedList<IMesh> GetOrCreateMeshList(string meshName)
	{
		if (!meshNameDictionary.TryGetValue(meshName, out LinkedList<IMesh>? list))
		{
			list = new LinkedList<IMesh>();
			meshNameDictionary.Add(meshName, list);
		}
		return list;
	}

	private List<(IMesh Mesh, IRenderer Renderer, IMeshFilter MeshFilter, ITransform Transform)> GetOrCreatePartList(string cleanName)
	{
		if (!cleanNameToParts.TryGetValue(cleanName, out List<(IMesh, IRenderer, IMeshFilter, ITransform)>? list))
		{
			list = new List<(IMesh, IRenderer, IMeshFilter, ITransform)>();
			cleanNameToParts.Add(cleanName, list);
		}
		return list;
	}

	private IMesh? FindExistingMesh(string cleanName, IEnumerable<(MeshData MeshData, IRenderer Renderer, IMeshFilter MeshFilter)> box)
	{
		if (!meshNameDictionary.TryGetValue(cleanName, out LinkedList<IMesh>? originalMeshes))
		{
			return null;
		}

		foreach (IMesh existingMesh in originalMeshes)
		{
			if (!MeshData.TryMakeFromMesh(existingMesh, out MeshData existingMeshData))
			{
				continue;
			}

			MeshData comparableMesh = MakeComparableMeshData(existingMeshData);
			if (box.All(entry => AreApproximatelyEqual(entry.MeshData, comparableMesh)))
			{
				return existingMesh;
			}
		}

		return null;
	}

	private static IMesh MakeMeshFromBox(string cleanName, MeshData meshData, ProcessedAssetCollection processedCollection)
	{
		IMesh newMesh = processedCollection.CreateAsset((int)ClassIDType.Mesh, Mesh.Create);
		newMesh.Name = cleanName;
		newMesh.FillWithCompressedMeshData(CloneMeshData(meshData));
		return newMesh;
	}

	private bool TryGetOrMakeCombinedMeshData(IMesh combinedMesh, out MeshData combinedMeshData)
	{
		if (badMeshSet.Contains(combinedMesh))
		{
			combinedMeshData = default;
			return false;
		}

		if (combinedMeshDictionary.TryGetValue(combinedMesh, out combinedMeshData))
		{
			return true;
		}

		if (MeshData.TryMakeFromMesh(combinedMesh, out combinedMeshData))
		{
			combinedMeshDictionary.Add(combinedMesh, combinedMeshData);
			return true;
		}

		badMeshSet.Add(combinedMesh);
		combinedMeshData = default;
		return false;
	}

	private static bool TryGetStaticMeshInformation(
		IUnityObjectBase component,
		[NotNullWhen(true)] out string? cleanName,
		out (IMesh Mesh, IRenderer Renderer, IMeshFilter MeshFilter, ITransform Transform) parts)
	{
		if (!component.IsStaticMeshRenderer(out IRenderer? renderer))
		{
			cleanName = null;
			parts = default;
			return false;
		}

		IGameObject? gameObject = renderer.GameObject_C25P;
		string cleanedName = gameObject is null ? string.Empty : GameObjectNameCleaner.CleanName(gameObject.Name);
		IMeshFilter? meshFilter = gameObject?.TryGetComponent<IMeshFilter>();
		IMesh? mesh = meshFilter?.MeshP;
		ITransform? transform = gameObject?.TryGetComponent<ITransform>();
		if (string.IsNullOrEmpty(cleanedName) || meshFilter is null || mesh is null || transform is null)
		{
			cleanName = null;
			parts = default;
			return false;
		}

		cleanName = cleanedName;
		parts = (mesh, renderer, meshFilter, transform);
		return true;
	}

	private static MeshData MakeMeshDataFromSubMeshes(IRenderer renderer, MeshData combinedMeshData)
	{
		IReadOnlyList<uint> subMeshIndices = renderer.GetSubmeshIndices();
		int count = 0;
		for (int i = 0; i < subMeshIndices.Count; i++)
		{
			count += combinedMeshData.SubMeshes[(int)subMeshIndices[i]].IndexCount;
		}

		Vector3[] vertices = new Vector3[count];
		Vector3[]? normals = combinedMeshData.HasNormals ? new Vector3[count] : null;
		Vector4[]? tangents = combinedMeshData.HasTangents ? new Vector4[count] : null;
		ColorFloat[]? colors = combinedMeshData.HasColors ? new ColorFloat[count] : null;
		Vector2[]? uv0 = combinedMeshData.HasUV0 ? new Vector2[count] : null;
		Vector2[]? uv1 = combinedMeshData.HasUV1 ? new Vector2[count] : null;
		Vector2[]? uv2 = combinedMeshData.HasUV2 ? new Vector2[count] : null;
		Vector2[]? uv3 = combinedMeshData.HasUV3 ? new Vector2[count] : null;
		Vector2[]? uv4 = combinedMeshData.HasUV4 ? new Vector2[count] : null;
		Vector2[]? uv5 = combinedMeshData.HasUV5 ? new Vector2[count] : null;
		Vector2[]? uv6 = combinedMeshData.HasUV6 ? new Vector2[count] : null;
		Vector2[]? uv7 = combinedMeshData.HasUV7 ? new Vector2[count] : null;
		BoneWeight4[]? skin = combinedMeshData.HasSkin ? new BoneWeight4[count] : null;
		Matrix4x4[]? bindPose = combinedMeshData.BindPose is { Length: > 0 } ? new Matrix4x4[count] : null;
		uint[] processedIndexBuffer = new uint[count];
		SubMeshData[] subMeshes = new SubMeshData[subMeshIndices.Count];

		int offset = 0;
		for (int i = 0; i < subMeshIndices.Count; i++)
		{
			SubMeshData combinedSubMesh = combinedMeshData.SubMeshes[(int)subMeshIndices[i]];
			for (int j = 0; j < combinedSubMesh.IndexCount; j++)
			{
				int newIndex = offset + j;
				int sourceIndex = (int)combinedMeshData.ProcessedIndexBuffer[combinedSubMesh.FirstIndex + j];
				vertices[newIndex] = combinedMeshData.Vertices[sourceIndex];
				CopyIfPresent(normals, newIndex, combinedMeshData.Normals, sourceIndex);
				CopyIfPresent(tangents, newIndex, combinedMeshData.Tangents, sourceIndex);
				CopyIfPresent(colors, newIndex, combinedMeshData.Colors, sourceIndex);
				CopyIfPresent(uv0, newIndex, combinedMeshData.UV0, sourceIndex);
				CopyIfPresent(uv1, newIndex, combinedMeshData.UV1, sourceIndex);
				CopyIfPresent(uv2, newIndex, combinedMeshData.UV2, sourceIndex);
				CopyIfPresent(uv3, newIndex, combinedMeshData.UV3, sourceIndex);
				CopyIfPresent(uv4, newIndex, combinedMeshData.UV4, sourceIndex);
				CopyIfPresent(uv5, newIndex, combinedMeshData.UV5, sourceIndex);
				CopyIfPresent(uv6, newIndex, combinedMeshData.UV6, sourceIndex);
				CopyIfPresent(uv7, newIndex, combinedMeshData.UV7, sourceIndex);
				CopyIfPresent(skin, newIndex, combinedMeshData.Skin, sourceIndex);
				CopyIfPresent(bindPose, newIndex, combinedMeshData.BindPose, sourceIndex);
				processedIndexBuffer[newIndex] = (uint)newIndex;
			}

			subMeshes[i] = combinedSubMesh with
			{
				FirstIndex = offset,
				FirstVertex = offset,
				VertexCount = combinedSubMesh.IndexCount,
				LocalBounds = Bounds.CalculateFromVertexArray(new ReadOnlySpan<Vector3>(vertices, offset, combinedSubMesh.IndexCount)),
			};
			offset += combinedSubMesh.IndexCount;
		}

		return new MeshData(vertices, normals, tangents, colors, uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7, skin, bindPose, processedIndexBuffer, subMeshes);
	}

	private static MeshData ApplyTransformationToMeshData(MeshData meshData, Transformation transformation, Transformation inverseTransformation)
	{
		Vector3[] vertices = Duplicate(meshData.Vertices);
		Vector3[]? normals = DuplicateNullable(meshData.Normals);
		Vector4[]? tangents = DuplicateNullable(meshData.Tangents);
		Transformation positionTransform = inverseTransformation;
		Transformation tangentTransform = positionTransform.RemoveTranslation();
		Transformation normalTransform = transformation.Transpose();

		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] *= positionTransform;
		}

		if (normals is not null)
		{
			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] *= normalTransform;
			}
		}

		if (tangents is not null)
		{
			for (int i = 0; i < tangents.Length; i++)
			{
				Vector4 tangent = tangents[i];
				Vector3 transformed = Vector3.Normalize(tangent.AsVector3() * tangentTransform);
				tangents[i] = new Vector4(transformed, tangent.W < 0 ? -1 : 1);
			}
		}

		return new MeshData(vertices, normals, tangents, meshData.Colors, meshData.UV0, meshData.UV1, meshData.UV2, meshData.UV3, meshData.UV4, meshData.UV5, meshData.UV6, meshData.UV7, meshData.Skin, meshData.BindPose, meshData.ProcessedIndexBuffer, meshData.SubMeshes);
	}

	private static void CreateTransformations(ITransform transform, out Transformation globalTransform, out Transformation globalInverseTransform)
	{
		globalTransform = Transformation.Identity;
		globalInverseTransform = Transformation.Identity;

		for (ITransform? current = transform; current is not null; current = current.Father_C4P)
		{
			globalTransform *= current.ToTransformation();
			globalInverseTransform = current.ToInverseTransformation() * globalInverseTransform;
		}
	}

	private static MeshData CloneMeshData(MeshData meshData)
	{
		return new MeshData(Duplicate(meshData.Vertices), DuplicateNullable(meshData.Normals), DuplicateNullable(meshData.Tangents), DuplicateNullable(meshData.Colors), DuplicateNullable(meshData.UV0), DuplicateNullable(meshData.UV1), DuplicateNullable(meshData.UV2), DuplicateNullable(meshData.UV3), DuplicateNullable(meshData.UV4), DuplicateNullable(meshData.UV5), DuplicateNullable(meshData.UV6), DuplicateNullable(meshData.UV7), DuplicateNullable(meshData.Skin), DuplicateNullable(meshData.BindPose), Duplicate(meshData.ProcessedIndexBuffer), Duplicate(meshData.SubMeshes));
	}

	private static MeshData MakeComparableMeshData(MeshData meshData)
	{
		bool alreadyComparable = true;
		for (uint i = 0; i < meshData.ProcessedIndexBuffer.Length; i++)
		{
			if (meshData.ProcessedIndexBuffer[i] != i)
			{
				alreadyComparable = false;
				break;
			}
		}

		if (alreadyComparable)
		{
			return meshData;
		}

		int count = meshData.ProcessedIndexBuffer.Length;
		Vector3[] vertices = new Vector3[count];
		Vector3[]? normals = meshData.HasNormals ? new Vector3[count] : null;
		Vector4[]? tangents = meshData.HasTangents ? new Vector4[count] : null;
		ColorFloat[]? colors = meshData.HasColors ? new ColorFloat[count] : null;
		Vector2[]? uv0 = meshData.HasUV0 ? new Vector2[count] : null;
		Vector2[]? uv1 = meshData.HasUV1 ? new Vector2[count] : null;
		Vector2[]? uv2 = meshData.HasUV2 ? new Vector2[count] : null;
		Vector2[]? uv3 = meshData.HasUV3 ? new Vector2[count] : null;
		Vector2[]? uv4 = meshData.HasUV4 ? new Vector2[count] : null;
		Vector2[]? uv5 = meshData.HasUV5 ? new Vector2[count] : null;
		Vector2[]? uv6 = meshData.HasUV6 ? new Vector2[count] : null;
		Vector2[]? uv7 = meshData.HasUV7 ? new Vector2[count] : null;
		BoneWeight4[]? skin = meshData.HasSkin ? new BoneWeight4[count] : null;
		Matrix4x4[]? bindPose = meshData.BindPose is { Length: > 0 } ? new Matrix4x4[count] : null;
		uint[] processedIndexBuffer = new uint[count];

		for (int i = 0; i < count; i++)
		{
			int sourceIndex = (int)meshData.ProcessedIndexBuffer[i];
			vertices[i] = meshData.Vertices[sourceIndex];
			CopyIfPresent(normals, i, meshData.Normals, sourceIndex);
			CopyIfPresent(tangents, i, meshData.Tangents, sourceIndex);
			CopyIfPresent(colors, i, meshData.Colors, sourceIndex);
			CopyIfPresent(uv0, i, meshData.UV0, sourceIndex);
			CopyIfPresent(uv1, i, meshData.UV1, sourceIndex);
			CopyIfPresent(uv2, i, meshData.UV2, sourceIndex);
			CopyIfPresent(uv3, i, meshData.UV3, sourceIndex);
			CopyIfPresent(uv4, i, meshData.UV4, sourceIndex);
			CopyIfPresent(uv5, i, meshData.UV5, sourceIndex);
			CopyIfPresent(uv6, i, meshData.UV6, sourceIndex);
			CopyIfPresent(uv7, i, meshData.UV7, sourceIndex);
			CopyIfPresent(skin, i, meshData.Skin, sourceIndex);
			CopyIfPresent(bindPose, i, meshData.BindPose, sourceIndex);
			processedIndexBuffer[i] = (uint)i;
		}

		SubMeshData[] subMeshes = Duplicate(meshData.SubMeshes);
		for (int i = 0; i < subMeshes.Length; i++)
		{
			subMeshes[i] = subMeshes[i] with
			{
				VertexCount = subMeshes[i].IndexCount,
				FirstVertex = subMeshes[i].FirstIndex,
				BaseVertex = 0,
			};
		}

		return new MeshData(vertices, normals, tangents, colors, uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7, skin, bindPose, processedIndexBuffer, subMeshes);
	}

	private static bool AreApproximatelyEqual(MeshData meshData, MeshData otherMeshData)
	{
		if (!AreSameLength(meshData.Vertices, otherMeshData.Vertices)
			|| !AreSameLength(meshData.Normals, otherMeshData.Normals)
			|| !AreSameLength(meshData.Tangents, otherMeshData.Tangents)
			|| !AreSameLength(meshData.Colors, otherMeshData.Colors)
			|| !AreSameLength(meshData.UV0, otherMeshData.UV0)
			|| !AreSameLength(meshData.UV1, otherMeshData.UV1)
			|| !AreSameLength(meshData.UV2, otherMeshData.UV2)
			|| !AreSameLength(meshData.UV3, otherMeshData.UV3)
			|| !AreSameLength(meshData.UV4, otherMeshData.UV4)
			|| !AreSameLength(meshData.UV5, otherMeshData.UV5)
			|| !AreSameLength(meshData.UV6, otherMeshData.UV6)
			|| !AreSameLength(meshData.UV7, otherMeshData.UV7)
			|| !AreIndicesEqual(meshData.Skin, otherMeshData.Skin)
			|| !AreSameLength(meshData.BindPose, otherMeshData.BindPose)
			|| !meshData.ProcessedIndexBuffer.AsSpan().SequenceEqual(otherMeshData.ProcessedIndexBuffer))
		{
			return false;
		}

		double totalSum = 0;
		long totalCount = 0;
		AddDistance(meshData.Vertices, otherMeshData.Vertices, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.Normals, otherMeshData.Normals, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.Tangents, otherMeshData.Tangents, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.Colors, otherMeshData.Colors, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV0, otherMeshData.UV0, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV1, otherMeshData.UV1, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV2, otherMeshData.UV2, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV3, otherMeshData.UV3, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV4, otherMeshData.UV4, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV5, otherMeshData.UV5, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV6, otherMeshData.UV6, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.UV7, otherMeshData.UV7, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.Skin, otherMeshData.Skin, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		AddDistance(meshData.BindPose, otherMeshData.BindPose, ref totalSum, ref totalCount, RelativeDistanceMethods.RelativeDistance);
		return totalCount == 0 || totalSum / totalCount < MaxMeshDeviation / 2;
	}

	private delegate void DistanceAccumulator<T>(T[] first, T[] second, out float sum, out int count);

	private static void AddDistance<T>(T[]? first, T[]? second, ref double totalSum, ref long totalCount, DistanceAccumulator<T> distanceFunction)
	{
		if (first is null || second is null)
		{
			return;
		}

		distanceFunction(first, second, out float sum, out int count);
		totalSum += sum;
		totalCount += count;
	}

	private static bool AreSameLength<T>(T[]? first, T[]? second)
	{
		return first is null || second is null ? first is null && second is null : first.Length == second.Length;
	}

	private static bool AreIndicesEqual(BoneWeight4[]? first, BoneWeight4[]? second)
	{
		if (first is null || second is null)
		{
			return first is null && second is null;
		}

		if (first.Length != second.Length)
		{
			return false;
		}

		for (int i = 0; i < first.Length; i++)
		{
			if (first[i].Index0 != second[i].Index0
				|| first[i].Index1 != second[i].Index1
				|| first[i].Index2 != second[i].Index2
				|| first[i].Index3 != second[i].Index3)
			{
				return false;
			}
		}

		return true;
	}

	private static void CopyIfPresent<T>(T[]? destination, int destinationIndex, T[]? source, int sourceIndex)
	{
		if (destination is not null && source is not null)
		{
			destination[destinationIndex] = source[sourceIndex];
		}
	}

	private static T[] Duplicate<T>(T[] source)
	{
		T[] copy = new T[source.Length];
		Array.Copy(source, copy, source.Length);
		return copy;
	}

	private static T[]? DuplicateNullable<T>(T[]? source)
	{
		return source is null ? null : Duplicate(source);
	}
}
