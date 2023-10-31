using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Logging;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.CompressedMesh;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Processing.StaticMeshes
{
	public class StaticMeshProcessor : IAssetProcessor
	{
		private const double MaxMeshDeviation = 0.1;
		private readonly Dictionary<IMesh, MeshData> combinedMeshDictionary = new();
		private readonly HashSet<IMesh> badMeshSet = new();
		private readonly Dictionary<string, LinkedList<IMesh>> meshNameDictionary = new();
		private readonly Dictionary<string, List<(IMesh, IRenderer, IMeshFilter, ITransform)>> cleanNameToParts = new();

		public void Process(GameData gameData)
		{
			Logger.Info(LogCategory.Processing, "Separate Static Meshes");
			ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Separated Static Meshes");

			foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
			{
				if (asset is IMesh mesh)
				{
					LinkedList<IMesh> meshList = meshNameDictionary.GetOrAdd(mesh.Name);
					meshList.AddLast(mesh);
				}
				else if (TryGetStaticMeshInformation(asset, out string? cleanName, out (IMesh, IRenderer, IMeshFilter, ITransform) parts))
				{
					cleanNameToParts.GetOrAdd(cleanName).Add(parts);
				}
			}

			Logger.Info(LogCategory.Processing, $"Found {cleanNameToParts.Count} meshes combined into static batches");
			foreach ((string cleanName, List<(IMesh, IRenderer, IMeshFilter, ITransform)> list) in cleanNameToParts)
			{
				Logger.Info(LogCategory.Processing, $"Attempting to recreate {cleanName} from {list.Count} instances");
				LinkedList<LinkedList<(MeshData, IRenderer, IMeshFilter)>> boxes = new();
				foreach ((IMesh combinedMesh, IRenderer renderer, IMeshFilter meshFilter, ITransform transform) in list)
				{
					if (TryGetOrMakeCombinedMeshData(combinedMesh, out MeshData combinedMeshData))
					{
						MeshData instanceMeshData = MakeMeshDataFromSubMeshes(renderer, combinedMeshData);
						CreateTranformations(transform, out Transformation transformation, out Transformation inverseTransformation);
						ApplyTransformationToMeshData(instanceMeshData, transformation, inverseTransformation);
						bool addedToABox = false;
						foreach (LinkedList<(MeshData, IRenderer, IMeshFilter)> box in boxes)
						{
							bool shouldGoInTheBox = true;
							foreach ((MeshData otherMeshData, _, _) in box)
							{
								if (!AreApproximatelyEqual(instanceMeshData, otherMeshData))
								{
									shouldGoInTheBox = false;
									break;
								}
							}
							if (shouldGoInTheBox)
							{
								box.AddFirst((instanceMeshData, renderer, meshFilter));
								addedToABox = true;
								break;
							}
						}
						if (!addedToABox)
						{
							LinkedList<(MeshData, IRenderer, IMeshFilter)> box = new();
							box.AddFirst((instanceMeshData, renderer, meshFilter));
							boxes.AddLast(box);
						}
					}
				}
				Logger.Info(LogCategory.Processing, $"Separated instances of {cleanName} into {boxes.Count} boxes");
				foreach (LinkedList<(MeshData, IRenderer, IMeshFilter)> box in boxes)
				{
					IMesh? separatedMesh = null;
					if (meshNameDictionary.TryGetValue(cleanName, out LinkedList<IMesh>? originalMeshes))
					{
						foreach (IMesh existingMesh in originalMeshes)
						{
							if (MeshData.TryMakeFromMesh(existingMesh, out MeshData existingMeshData))
							{
								existingMeshData = existingMeshData.MakeComparableMeshData();
								if (box.Select(t => t.Item1).All(meshData => AreApproximatelyEqual(meshData, existingMeshData)))
								{
									separatedMesh = existingMesh;
									break;
								}
							}
						}
					}
					if (separatedMesh is null)
					{
						MeshData averageMeshData = box.Count == 1 ? box.First!.Value.Item1 : Average(box.Select(t => t.Item1));
						separatedMesh = MakeMeshFromData(cleanName, averageMeshData, processedCollection);
					}

					foreach ((_, IRenderer renderer, IMeshFilter meshFilter) in box)
					{
						ApplyMeshToRendererAndFilter(separatedMesh, renderer, meshFilter);
					}
				}
			}

			meshNameDictionary.Clear();
			cleanNameToParts.Clear();
		}

		#region Average MeshData
		private static MeshData Average(IEnumerable<MeshData> enumerable)
		{
			MeshData result = default;
			int count = 0;
			foreach (MeshData meshData in enumerable)
			{
				if (count is 0)
				{
					result = meshData.DeepClone();
				}
				else
				{
					double proportion = (double)count / (count + 1);
					UpdateAverage(result, meshData, proportion);
				}
				count++;
			}
			return result;
		}

		private static void UpdateAverage(MeshData previousAverage, MeshData data, double proportion)
		{
			double otherProportion = 1 - proportion;

			UpdateArray(previousAverage.Vertices, proportion, data.Vertices, otherProportion);
			if (previousAverage.Normals is not null)
			{
				UpdateArray(previousAverage.Normals, proportion, data.Normals!, otherProportion);
			}
			if (previousAverage.Tangents is not null)
			{
				UpdateArray(previousAverage.Tangents, proportion, data.Tangents!, otherProportion);
			}
			if (previousAverage.Colors is not null)
			{
				UpdateArray(previousAverage.Colors, proportion, data.Colors!, otherProportion);
			}
			if (previousAverage.Skin is not null)
			{
				UpdateArray(previousAverage.Skin, proportion, data.Skin!, otherProportion);
			}
			if (previousAverage.UV0 is not null)
			{
				UpdateArray(previousAverage.UV0, proportion, data.UV0!, otherProportion);
			}
			if (previousAverage.UV1 is not null)
			{
				UpdateArray(previousAverage.UV1, proportion, data.UV1!, otherProportion);
			}
			if (previousAverage.UV2 is not null)
			{
				UpdateArray(previousAverage.UV2, proportion, data.UV2!, otherProportion);
			}
			if (previousAverage.UV3 is not null)
			{
				UpdateArray(previousAverage.UV3, proportion, data.UV3!, otherProportion);
			}
			if (previousAverage.UV4 is not null)
			{
				UpdateArray(previousAverage.UV4, proportion, data.UV4!, otherProportion);
			}
			if (previousAverage.UV5 is not null)
			{
				UpdateArray(previousAverage.UV5, proportion, data.UV5!, otherProportion);
			}
			if (previousAverage.UV6 is not null)
			{
				UpdateArray(previousAverage.UV6, proportion, data.UV6!, otherProportion);
			}
			if (previousAverage.UV7 is not null)
			{
				UpdateArray(previousAverage.UV7, proportion, data.UV7!, otherProportion);
			}
			if (previousAverage.BindPose is not null)
			{
				UpdateArray(previousAverage.BindPose, proportion, data.BindPose!, otherProportion);
			}
		}

		private static void UpdateArray(Vector2[] array1, double proportion1, Vector2[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = WeightedSum(array1[i], proportion1, array2[i], proportion2);
			}
		}

		private static Vector2 WeightedSum(Vector2 vector1, double proportion1, Vector2 vector2, double proportion2)
		{
			return new Vector2(
				WeightedSum(vector1.X, proportion1, vector2.X, proportion2),
				WeightedSum(vector1.Y, proportion1, vector2.Y, proportion2));
		}

		private static void UpdateArray(Vector3[] array1, double proportion1, Vector3[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = WeightedSum(array1[i], proportion1, array2[i], proportion2);
			}
		}

		private static Vector3 WeightedSum(Vector3 vector1, double proportion1, Vector3 vector2, double proportion2)
		{
			return new Vector3(
				WeightedSum(vector1.X, proportion1, vector2.X, proportion2),
				WeightedSum(vector1.Y, proportion1, vector2.Y, proportion2),
				WeightedSum(vector1.Z, proportion1, vector2.Z, proportion2));
		}

		private static void UpdateArray(Vector4[] array1, double proportion1, Vector4[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = WeightedSum(array1[i], proportion1, array2[i], proportion2);
			}
		}

		private static Vector4 WeightedSum(Vector4 vector1, double proportion1, Vector4 vector2, double proportion2)
		{
			return new Vector4(
				WeightedSum(vector1.X, proportion1, vector2.X, proportion2),
				WeightedSum(vector1.Y, proportion1, vector2.Y, proportion2),
				WeightedSum(vector1.Z, proportion1, vector2.Z, proportion2),
				WeightedSum(vector1.W, proportion1, vector2.W, proportion2));
		}

		private static void UpdateArray(ColorFloat[] array1, double proportion1, ColorFloat[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = new ColorFloat(WeightedSum(array1[i].Vector, proportion1, array2[i].Vector, proportion2));
			}
		}

		private static void UpdateArray(BoneWeight4[] array1, double proportion1, BoneWeight4[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = WeightedSum(array1[i], proportion1, array2[i], proportion2);
			}
		}

		private static BoneWeight4 WeightedSum(BoneWeight4 vector1, double proportion1, BoneWeight4 vector2, double proportion2)
		{
			return new BoneWeight4(
				WeightedSum(vector1.Weight0, proportion1, vector2.Weight0, proportion2),
				WeightedSum(vector1.Weight1, proportion1, vector2.Weight1, proportion2),
				WeightedSum(vector1.Weight2, proportion1, vector2.Weight2, proportion2),
				WeightedSum(vector1.Weight3, proportion1, vector2.Weight3, proportion2),
				vector1.Index0,
				vector1.Index1,
				vector1.Index2,
				vector1.Index3);
		}

		private static void UpdateArray(Matrix4x4[] array1, double proportion1, Matrix4x4[] array2, double proportion2)
		{
			for (int i = array1.Length - 1; i >= 0; i--)
			{
				array1[i] = WeightedSum(array1[i], proportion1, array2[i], proportion2);
			}
		}

		private static Matrix4x4 WeightedSum(Matrix4x4 matrix1, double proportion1, Matrix4x4 matrix2, double proportion2)
		{
			return new Matrix4x4(
				WeightedSum(matrix1.M11, proportion1, matrix2.M11, proportion2),
				WeightedSum(matrix1.M12, proportion1, matrix2.M12, proportion2),
				WeightedSum(matrix1.M13, proportion1, matrix2.M13, proportion2),
				WeightedSum(matrix1.M14, proportion1, matrix2.M14, proportion2),
				WeightedSum(matrix1.M21, proportion1, matrix2.M21, proportion2),
				WeightedSum(matrix1.M22, proportion1, matrix2.M22, proportion2),
				WeightedSum(matrix1.M23, proportion1, matrix2.M23, proportion2),
				WeightedSum(matrix1.M24, proportion1, matrix2.M24, proportion2),
				WeightedSum(matrix1.M31, proportion1, matrix2.M31, proportion2),
				WeightedSum(matrix1.M32, proportion1, matrix2.M32, proportion2),
				WeightedSum(matrix1.M33, proportion1, matrix2.M33, proportion2),
				WeightedSum(matrix1.M34, proportion1, matrix2.M34, proportion2),
				WeightedSum(matrix1.M41, proportion1, matrix2.M41, proportion2),
				WeightedSum(matrix1.M42, proportion1, matrix2.M42, proportion2),
				WeightedSum(matrix1.M43, proportion1, matrix2.M43, proportion2),
				WeightedSum(matrix1.M44, proportion1, matrix2.M44, proportion2));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static float WeightedSum(float value1, double proportion1, float value2, double proportion2)
		{
			return (float)(value1 * proportion1 + value2 * proportion2);
		}
		#endregion

		private static void ApplyMeshToRendererAndFilter(IMesh mesh, IRenderer renderer, IMeshFilter meshFilter)
		{
			meshFilter.MeshP = mesh;
			renderer.ClearStaticBatchInfo();
			renderer.MarkGameObjectAsStatic();
		}

		private static IMesh MakeMeshFromData(string cleanName, MeshData instanceMeshData, ProcessedAssetCollection processedCollection)
		{
			IMesh newMesh = CreateMesh(processedCollection);
			newMesh.Name = cleanName;

			newMesh.SetIndexFormat(instanceMeshData.GetIndexFormat());

			ICompressedMesh compressedMesh = newMesh.CompressedMesh;
			compressedMesh.SetVertices(instanceMeshData.Vertices);
			compressedMesh.SetNormals(instanceMeshData.Normals);
			compressedMesh.SetTangents(instanceMeshData.Tangents);
			compressedMesh.SetFloatColors(instanceMeshData.Colors);
			compressedMesh.SetWeights(instanceMeshData.Skin);
			compressedMesh.SetUV(
				instanceMeshData.UV0,
				instanceMeshData.UV1,
				instanceMeshData.UV2,
				instanceMeshData.UV3,
				instanceMeshData.UV4,
				instanceMeshData.UV5,
				instanceMeshData.UV6,
				instanceMeshData.UV7);
			compressedMesh.SetBindPoses(instanceMeshData.BindPose);
			compressedMesh.SetTriangles(instanceMeshData.ProcessedIndexBuffer);

			newMesh.KeepIndices = true;//Not sure about this. Seems to be for animated meshes
			newMesh.KeepVertices = true;//Not sure about this. Seems to be for animated meshes
			newMesh.MeshMetrics_0_ = CalculateMeshMetric(instanceMeshData.Vertices, instanceMeshData.UV0, instanceMeshData.ProcessedIndexBuffer, instanceMeshData.SubMeshes, 0);
			newMesh.MeshMetrics_1_ = CalculateMeshMetric(instanceMeshData.Vertices, instanceMeshData.UV1, instanceMeshData.ProcessedIndexBuffer, instanceMeshData.SubMeshes, 1);
			newMesh.MeshUsageFlags = (int)SourceGenerated.NativeEnums.Global.MeshUsageFlags.MeshUsageFlagNone;
			newMesh.CookingOptions = (int)SourceGenerated.NativeEnums.Global.MeshColliderCookingOptions.DefaultCookingFlags;
			//I copied 30 from a vanilla compressed mesh (with MeshCompression.Low), and it aligned with this enum.
			newMesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
			newMesh.SetMeshCompression(ModelImporterMeshCompression.Low);

			AccessListBase<ISubMesh> subMeshList = newMesh.SubMeshes;
			foreach (SubMeshData subMesh in instanceMeshData.SubMeshes)
			{
				subMesh.CopyTo(subMeshList.AddNew(), newMesh.GetIndexFormat());
			}

			newMesh.LocalAABB.CalculateFromVertexArray(instanceMeshData.Vertices);

			return newMesh;
		}

		private static float CalculateMeshMetric(ReadOnlySpan<Vector3> vertexBuffer, ReadOnlySpan<Vector2> uvBuffer, uint[] indexBuffer, IReadOnlyList<SubMeshData> subMeshList, int uvSetIndex, float uvAreaThreshold = 1e-9f)
		{
			//https://docs.unity3d.com/ScriptReference/Mesh.GetUVDistributionMetric.html
			//https://docs.unity3d.com/ScriptReference/Mesh.RecalculateUVDistributionMetric.html
			//https://docs.unity3d.com/ScriptReference/Mesh.RecalculateUVDistributionMetrics.html

			const float DefaultMetric = 1.0f;
			if (vertexBuffer.Length == 0 || uvBuffer.Length == 0 || uvSetIndex >= subMeshList.Count)
			{
				return DefaultMetric;
			}

			int n = 0;
			float vertexAreaSum = 0.0f;
			float uvAreaSum = 0.0f;
			foreach ((uint ia, uint ib, uint ic) in new TriangleEnumerable(subMeshList[uvSetIndex], indexBuffer))
			{
				(Vector2 uva, Vector2 uvb, Vector2 uvc) = (uvBuffer[(int)ia], uvBuffer[(int)ib], uvBuffer[(int)ic]);
				float uvArea = TriangleArea(uva, uvb, uvc);
				if (uvArea < uvAreaThreshold)
				{
					continue;
				}

				(Vector3 va, Vector3 vb, Vector3 vc) = (vertexBuffer[(int)ia], vertexBuffer[(int)ib], vertexBuffer[(int)ic]);
				float vertexArea = TriangleArea(va, vb, vc);
				vertexAreaSum += vertexArea;
				uvAreaSum += uvArea;
				n++;
			}

			if (n is 0 || uvAreaSum == 0.0f)
			{
				return DefaultMetric;
			}
			else
			{
				//Average of triangle area divided by uv area.
				return vertexAreaSum / n / uvAreaSum;
			}
		}

		private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
		{
			return TriangleArea(a.AsVector3(), b.AsVector3(), c.AsVector3());
		}

		private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
		{
			return Vector3.Cross(b - a, c - a).Length() * 0.5f;
		}

		private static void ApplyTransformationToMeshData(MeshData instanceMeshData, Transformation transformation, Transformation inverseTransformation)
		{
			//We need to apply the inverse transform to reverse the static batching.
			Transformation positionTransform = inverseTransformation;
			Transformation tangentTransform = positionTransform.RemoveTranslation();
			Transformation normalTransform = transformation.Transpose();

			for (int i = instanceMeshData.Vertices.Length - 1; i >= 0; i--)
			{
				instanceMeshData.Vertices[i] = instanceMeshData.Vertices[i] * positionTransform;
			}

			if (instanceMeshData.Normals is not null)
			{
				for (int i = instanceMeshData.Normals.Length - 1; i >= 0; i--)
				{
					instanceMeshData.Normals[i] = instanceMeshData.Normals[i] * normalTransform;
				}
			}

			if (instanceMeshData.Tangents is not null)
			{
				for (int i = instanceMeshData.Tangents.Length - 1; i >= 0; i--)
				{
					Vector4 originalTangent = instanceMeshData.Tangents[i];
					Vector3 transformedTangent = Vector3.Normalize(originalTangent.AsVector3() * tangentTransform);
					//Unity documentation claims W should always be 1 or -1, but it's not always the case.
					float w = originalTangent.W < 0 ? -1 : 1;
					instanceMeshData.Tangents[i] = new Vector4(transformedTangent, w);
				}
			}
		}

		private static void CreateTranformations(ITransform transform, out Transformation globalTransform, out Transformation globalInverseTransform)
		{
			globalTransform = Transformation.Identity;
			globalInverseTransform = Transformation.Identity;

			ITransform? currentTransform = transform;
			while (currentTransform is not null)
			{
				Transformation localTransform = currentTransform.ToTransformation();
				Transformation localInverseTransform = currentTransform.ToInverseTransformation();

				//GlbModelExporter uses a top->down calculation of these transforms.
				//Because this is bottom->up, the multiplication order is reversed.
#pragma warning disable IDE0054 // Use compound assignment
				globalTransform = globalTransform * localTransform;
#pragma warning restore IDE0054 // Use compound assignment
				globalInverseTransform = localInverseTransform * globalInverseTransform;

				currentTransform = currentTransform.Father_C4P;
			}
		}

		private static MeshData MakeMeshDataFromSubMeshes(IRenderer renderer, MeshData combinedMeshData)
		{
			IReadOnlyList<uint> subMeshIndicies = renderer.GetSubmeshIndices();

			int count = 0;
			for (int i = 0; i < subMeshIndicies.Count; i++)
			{
				count += combinedMeshData.SubMeshes[(int)subMeshIndicies[i]].IndexCount;
			}

			Vector3[] vertices = new Vector3[count];
			Vector3[]? normals = combinedMeshData.Normals.IsNullOrEmpty() ? null : new Vector3[count];
			Vector4[]? tangents = combinedMeshData.Tangents.IsNullOrEmpty() ? null : new Vector4[count];
			ColorFloat[]? colors = combinedMeshData.Colors.IsNullOrEmpty() ? null : new ColorFloat[count];
			BoneWeight4[]? skin = combinedMeshData.Skin.IsNullOrEmpty() ? null : new BoneWeight4[count];
			Vector2[]? uv0 = combinedMeshData.UV0.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv1 = combinedMeshData.UV1.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv2 = combinedMeshData.UV2.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv3 = combinedMeshData.UV3.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv4 = combinedMeshData.UV4.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv5 = combinedMeshData.UV5.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv6 = combinedMeshData.UV6.IsNullOrEmpty() ? null : new Vector2[count];
			Vector2[]? uv7 = combinedMeshData.UV7.IsNullOrEmpty() ? null : new Vector2[count];
			Matrix4x4[]? bindpose = combinedMeshData.BindPose.IsNullOrEmpty() ? null : new Matrix4x4[count];
			uint[] processedIndexBuffer = new uint[count];
			SubMeshData[] subMeshes = new SubMeshData[subMeshIndicies.Count];

			int offset = 0;
			for (int k = 0; k < subMeshIndicies.Count; k++)
			{
				SubMeshData combinedSubMesh = combinedMeshData.SubMeshes[(int)subMeshIndicies[k]];
				int combinedIndexStart = combinedSubMesh.FirstIndex;
				for (int j = 0; j < combinedSubMesh.IndexCount; j++)
				{
					int newIndex = j + offset;
					int combinedIndex = (int)combinedMeshData.ProcessedIndexBuffer[j + combinedIndexStart];
					vertices[newIndex] = combinedMeshData.Vertices[combinedIndex];
					SetUnlessNull(normals, newIndex, combinedMeshData.Normals, combinedIndex);
					SetUnlessNull(tangents, newIndex, combinedMeshData.Tangents, combinedIndex);
					SetUnlessNull(colors, newIndex, combinedMeshData.Colors, combinedIndex);
					SetUnlessNull(skin, newIndex, combinedMeshData.Skin, combinedIndex);
					SetUnlessNull(uv0, newIndex, combinedMeshData.UV0, combinedIndex);
					SetUnlessNull(uv1, newIndex, combinedMeshData.UV1, combinedIndex);
					SetUnlessNull(uv2, newIndex, combinedMeshData.UV2, combinedIndex);
					SetUnlessNull(uv3, newIndex, combinedMeshData.UV3, combinedIndex);
					SetUnlessNull(uv4, newIndex, combinedMeshData.UV4, combinedIndex);
					SetUnlessNull(uv5, newIndex, combinedMeshData.UV5, combinedIndex);
					SetUnlessNull(uv6, newIndex, combinedMeshData.UV6, combinedIndex);
					SetUnlessNull(uv7, newIndex, combinedMeshData.UV7, combinedIndex);
					SetUnlessNull(bindpose, newIndex, combinedMeshData.BindPose, combinedIndex);
					processedIndexBuffer[newIndex] = (uint)newIndex;
				}
				SubMeshData newSubMesh = combinedSubMesh;
				newSubMesh.FirstIndex = offset;
				newSubMesh.FirstVertex = offset;
				newSubMesh.VertexCount = newSubMesh.IndexCount;
				//newSubMesh.BaseVertex //Might need set
				newSubMesh.LocalBounds = Bounds.CalculateFromVertexArray(new ReadOnlySpan<Vector3>(vertices, (int)offset, (int)newSubMesh.IndexCount));
				subMeshes[k] = newSubMesh;

				offset += combinedSubMesh.IndexCount;
			}

			return new MeshData(vertices, normals, tangents, colors, skin, uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7, bindpose, processedIndexBuffer, subMeshes);
		}

		private static void SetUnlessNull<T>(T[]? array1, int index1, T[]? array2, int index2)
		{
			if (array1 is not null)
			{
				array1[index1] = array2![index2];//array2 must have the same nullability as array1
			}
		}

		private bool TryGetOrMakeCombinedMeshData(IMesh combinedMesh, out MeshData combinedMeshData)
		{
			if (badMeshSet.Contains(combinedMesh))
			{
				combinedMeshData = default;
				return false;
			}
			else if (combinedMeshDictionary.TryGetValue(combinedMesh, out combinedMeshData)
				|| MeshData.TryMakeFromMesh(combinedMesh, out combinedMeshData))
			{
				return true;
			}
			else
			{
				badMeshSet.Add(combinedMesh);
				combinedMeshData = default;
				return false;
			}
		}

		private static bool TryGetStaticMeshInformation(
			IUnityObjectBase component,
			[NotNullWhen(true)] out string? cleanName,
			out (IMesh, IRenderer, IMeshFilter, ITransform) parts)
		{
			if (!component.IsStaticMeshRenderer(out IRenderer? renderer))
			{
				cleanName = null;
				parts = default;
				return false;
			}

			IGameObject? gameObject = renderer.GameObject_C2P;
			if (gameObject is null)
			{
				cleanName = null;
				parts = default;
				return false;
			}

			string cleanedName = GameObjectNameCleaner.CleanName(gameObject.Name);
			if (string.IsNullOrEmpty(cleanedName))
			{
				cleanName = null;
				parts = default;
				return false;
			}

			IMeshFilter? meshFilter = gameObject.TryGetComponent<IMeshFilter>();
			if (meshFilter is null)
			{
				cleanName = null;
				parts = default;
				return false;
			}

			IMesh? mesh = meshFilter.MeshP;
			if (mesh is null)
			{
				cleanName = null;
				parts = default;
				return false;
			}

			ITransform? transform = gameObject.TryGetComponent<ITransform>();
			if (transform is null)
			{
				cleanName = null;
				parts = default;
				return false;
			}

			cleanName = cleanedName;
			parts = (mesh, renderer, meshFilter, transform);
			return true;
		}

		private static IMesh CreateMesh(ProcessedAssetCollection processedCollection)
		{
			return processedCollection.CreateAsset((int)ClassIDType.Mesh, Mesh.Create);
		}

		#region Approximate Equality
		private static bool AreApproximatelyEqual(MeshData instanceMeshData, MeshData otherMeshData)
		{
			if (!AreSameLength(instanceMeshData.Vertices, otherMeshData.Vertices)
				|| !AreSameLength(instanceMeshData.Normals, otherMeshData.Normals)
				|| !AreSameLength(instanceMeshData.Tangents, otherMeshData.Tangents)
				|| !AreSameLength(instanceMeshData.Colors, otherMeshData.Colors)
				|| !AreIndicesEqual(instanceMeshData.Skin, otherMeshData.Skin)
				|| !AreSameLength(instanceMeshData.UV0, otherMeshData.UV0)
				|| !AreSameLength(instanceMeshData.UV1, otherMeshData.UV1)
				|| !AreSameLength(instanceMeshData.UV2, otherMeshData.UV2)
				|| !AreSameLength(instanceMeshData.UV3, otherMeshData.UV3)
				|| !AreSameLength(instanceMeshData.UV4, otherMeshData.UV4)
				|| !AreSameLength(instanceMeshData.UV5, otherMeshData.UV5)
				|| !AreSameLength(instanceMeshData.UV6, otherMeshData.UV6)
				|| !AreSameLength(instanceMeshData.UV7, otherMeshData.UV7)
				|| !AreSameLength(instanceMeshData.BindPose, otherMeshData.BindPose)
				|| !AreSequenceEqual(instanceMeshData.ProcessedIndexBuffer, otherMeshData.ProcessedIndexBuffer))
			{
				return false;
			}

			double totalSum = 0;
			long totalCount = 0;

			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.Vertices, otherMeshData.Vertices, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.Normals is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.Normals, otherMeshData.Normals!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.Tangents is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.Tangents, otherMeshData.Tangents!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.Colors is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.Colors, otherMeshData.Colors!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.Skin is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.Skin, otherMeshData.Skin!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV0 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV0, otherMeshData.UV0!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV1 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV1, otherMeshData.UV1!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV2 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV2, otherMeshData.UV2!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV3 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV3, otherMeshData.UV3!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV4 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV4, otherMeshData.UV4!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV5 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV5, otherMeshData.UV5!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV6 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV6, otherMeshData.UV6!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.UV7 is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.UV7, otherMeshData.UV7!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			if (instanceMeshData.BindPose is not null)
			{
				RelativeDistanceMethods.RelativeDistance(instanceMeshData.BindPose, otherMeshData.BindPose!, out float sum, out int count);
				totalSum += sum;
				totalCount += count;
			}

			//lots of distance calculations
			double averageRelativeDistance = totalSum / totalCount;
			return averageRelativeDistance < MaxMeshDeviation / 2;
		}

		private static bool AreSameLength<T>(T[]? array1, T[]? array2)
		{
			if (array1 is null || array2 is null)
			{
				return array1 is null && array2 is null;
			}
			else
			{
				return array1.Length == array2.Length;
			}
		}

		private static bool AreSequenceEqual<T>(T[]? array1, T[]? array2)
		{
			if (array1 is null || array2 is null)
			{
				return array1 is null && array2 is null;
			}
			else
			{
				return ((ReadOnlySpan<T>)array1).SequenceEqual(array2);
			}
		}

		private static bool AreIndicesEqual(BoneWeight4[]? array1, BoneWeight4[]? array2)
		{
			if (array1 is null || array2 is null)
			{
				return array1 is null && array2 is null;
			}
			else if (array1.Length != array2.Length)
			{
				return false;
			}
			else
			{
				for (int i = array1.Length - 1; i >= 0; i--)
				{
					if (!AreIndicesEqual(array1[i], array2[i]))
					{
						return false;
					}
				}
				return true;
			}
		}

		private static bool AreIndicesEqual(BoneWeight4 value1, BoneWeight4 value2)
		{
			return value1.Index0 == value2.Index0
				&& value1.Index1 == value2.Index1
				&& value1.Index2 == value2.Index2
				&& value1.Index3 == value2.Index3;
		}
		#endregion
	}
}
