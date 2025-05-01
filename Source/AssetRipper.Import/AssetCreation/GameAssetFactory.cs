using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Subclasses.AABB;
using AssetRipper.SourceGenerated.Subclasses.AABBInt;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Single;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;
using AssetRipper.SourceGenerated.Subclasses.Gradient;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.GUIStyle;
using AssetRipper.SourceGenerated.Subclasses.Hash128;
using AssetRipper.SourceGenerated.Subclasses.LayerMask;
using AssetRipper.SourceGenerated.Subclasses.Matrix4x4f;
using AssetRipper.SourceGenerated.Subclasses.PropertyName;
using AssetRipper.SourceGenerated.Subclasses.Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.Rectf;
using AssetRipper.SourceGenerated.Subclasses.RectOffset;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector2Int;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using AssetRipper.SourceGenerated.Subclasses.Vector3Int;
using AssetRipper.SourceGenerated.Subclasses.Vector4f;

namespace AssetRipper.Import.AssetCreation
{
	public sealed class GameAssetFactory : AssetFactoryBase
	{
		public GameAssetFactory(IAssemblyManager assemblyManager)
		{
			AssemblyManager = assemblyManager ?? throw new ArgumentNullException(nameof(assemblyManager));
		}

		private IAssemblyManager AssemblyManager { get; }

		public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, ReadOnlyArraySegment<byte> assetData, SerializedType? assetType)
		{
			if (assetInfo.Collection.Version.LessThan(3, 5))
			{
				//Assets with a stripped version can't be read.
				//Similarly, Unity versions before 3.5 are not supported.
				//Most asset types changed in 3.5, so this has minimal impact.
				return new UnreadableObject(assetInfo, assetData.ToArray());
			}
			else if (assetInfo.ClassID == (int)ClassIDType.MonoBehaviour)
			{
				return ReadMonoBehaviour(MonoBehaviour.Create(assetInfo), assetData, AssemblyManager, assetType);
			}
			else
			{
				return ReadNormalObject(assetInfo, assetData);
			}
		}

		private static IMonoBehaviour ReadMonoBehaviour(IMonoBehaviour monoBehaviour, ReadOnlyArraySegment<byte> assetData, IAssemblyManager assemblyManager, SerializedType? type)
		{
			EndianSpanReader reader = new EndianSpanReader(assetData, monoBehaviour.Collection.EndianType);
			try
			{
				monoBehaviour.Read(ref reader);
				SerializableStructure? structure;
				if (type is not null && TypeTreeNodeStruct.TryMakeFromTypeTree(type.OldType, out TypeTreeNodeStruct rootNode))
				{
					structure = SerializableTreeType.FromRootNode(rootNode, true).CreateSerializableStructure();
					if (structure.TryRead(ref reader, monoBehaviour))
					{
						monoBehaviour.Structure = structure;
					}
					else
					{
						monoBehaviour.Structure = null;
					}
				}
				else
				{
					monoBehaviour.Structure = new UnloadedStructure(monoBehaviour, assemblyManager, assetData.Slice(reader.Position));
				}
			}
			catch (Exception ex)
			{
				LogMonoBehaviorReadException(monoBehaviour, ex);
			}
			return monoBehaviour;
		}

		private static IUnityObjectBase ReadNormalObject(AssetInfo assetInfo, ReadOnlyArraySegment<byte> assetData)
		{
			IUnityObjectBase asset = TryReadNormalObject(assetInfo, assetData, assetInfo.Collection.Version, out string? error);
			if (error is null)
			{
				return asset;
			}
			else if (SpecialFileNames.IsDefaultResourceOrBuiltinExtra(assetInfo.Collection.Name))
			{
				Logger.Warning(LogCategory.Import, error);
				return asset;
			}
			else if (assetInfo.Collection.Version.Type == UnityVersionType.Patch)
			{
				UnityVersion oldVersion = assetInfo.Collection.Version;
				UnityVersion newVersion = new UnityVersion(oldVersion.Major, oldVersion.Minor, unchecked((ushort)(oldVersion.Build + 1u)));
				IUnityObjectBase newAsset = TryReadNormalObject(assetInfo, assetData, newVersion, out string? newError);
				if (newError is null)
				{
					return newAsset;
				}
			}

			Logger.Error(LogCategory.Import, error);
			UnreadableObject unreadable = new UnreadableObject(asset.AssetInfo, assetData.ToArray());
			unreadable.Name = (asset as INamed)?.Name;
			return unreadable;
		}

		private static IUnityObjectBase TryReadNormalObject(AssetInfo assetInfo, ReadOnlyArraySegment<byte> assetData, UnityVersion version, out string? error)
		{
			IUnityObjectBase? asset = CreateAsset(assetInfo, version);
			if (asset is null)
			{
				error = null;
				return new UnknownObject(assetInfo, assetData.ToArray());
			}
			EndianSpanReader reader = new EndianSpanReader(assetData, asset.Collection.EndianType);
			try
			{
				asset.Read(ref reader);
				if (reader.Position != reader.Length)
				{
					//Some Chinese Unity versions have extra fields appended to the global type trees.
					if (reader.Length - reader.Position == 24 && asset is ITexture2D texture)
					{
						ReadExtraTextureFields(texture, ref reader);
						error = null;
					}
					else
					{
						error = MakeError_IncorrectNumberOfBytesRead(asset, ref reader);
					}
				}
				else
				{
					error = null;
				}
			}
			catch (Exception ex)
			{
				error = MakeError_ReadException(asset, ex);
			}
			return asset;
		}

		private static IUnityObjectBase? CreateAsset(AssetInfo assetInfo, UnityVersion version)
		{
			IUnityObjectBase? asset = AssetFactory.CreateSerialized(assetInfo, version);
			if (asset is null && TypeTreeNodeStruct.TryMakeFromTpk((ClassIDType)assetInfo.ClassID, version, out TypeTreeNodeStruct releaseRoot, out TypeTreeNodeStruct editorRoot))
			{
				return TypeTreeObject.Create(assetInfo, releaseRoot, editorRoot);
			}
			else
			{
				return asset;
			}
		}

		private static string MakeError_IncorrectNumberOfBytesRead(IUnityObjectBase asset, ref EndianSpanReader reader)
		{
			return $"Read {reader.Position} but expected {reader.Length} for asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}";
		}

		/// <summary>
		/// A special case for Chinese textures containing an extra 24 bytes at the end.
		/// </summary>
		/// <param name="reader"></param>
		private static void ReadExtraTextureFields(ITexture2D texture, ref EndianSpanReader reader)
		{
			//int m_OriginalWidth // ByteSize{4}, Index{26}, Version{1}, IsArray{0}, MetaFlag{10}
			//int m_OriginalHeight // ByteSize{4}, Index{27}, Version{1}, IsArray{0}, MetaFlag{10}
			//GUID m_OriginalAssetGuid // ByteSize{10}, Index{28}, Version{1}, IsArray{0}, MetaFlag{10}
			//unsigned int data[0] // ByteSize{4}, Index{29}, Version{1}, IsArray{0}, MetaFlag{10}
			//unsigned int data[1] // ByteSize{4}, Index{2a}, Version{1}, IsArray{0}, MetaFlag{10}
			//unsigned int data[2] // ByteSize{4}, Index{2b}, Version{1}, IsArray{0}, MetaFlag{10}
			//unsigned int data[3] // ByteSize{4}, Index{2c}, Version{1}, IsArray{0}, MetaFlag{10}
			texture.OriginalWidth_C28 = reader.ReadInt32();
			texture.OriginalHeight_C28 = reader.ReadInt32();
			texture.OriginalAssetGuid_C28.ReadRelease(ref reader);//Release and Editor are the same for GUID.
			Logger.Warning(LogCategory.Import, $"Texture {texture.Name} had an extra 24 bytes, which were assumed to be non-standard Chinese fields.");
		}

		private static void LogMonoBehaviorReadException(IMonoBehaviour monoBehaviour, Exception ex)
		{
			Logger.Error(LogCategory.Import, $"Unable to read {monoBehaviour}, because script {monoBehaviour.Structure} layout mismatched binary content ({ex.GetType().Name}).");
		}

		private static string MakeError_ReadException(IUnityObjectBase asset, Exception ex)
		{
			return $"Error during reading of asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}\n{ex}";
		}

		public static IUnityAssetBase CreateEngineAsset(string name, UnityVersion version)
		{
			return name switch
			{
				MonoUtils.GuidName => GUID.Create(),
				MonoUtils.Hash128Name => Hash128.Create(version),
				MonoUtils.Vector2Name => Vector2f.Create(),
				MonoUtils.Vector2IntName => Vector2Int.Create(),
				MonoUtils.Vector3Name => Vector3f.Create(),
				MonoUtils.Vector3IntName => Vector3Int.Create(),
				MonoUtils.Vector4Name => Vector4f.Create(),
				MonoUtils.RectName => Rectf.Create(),
				MonoUtils.BoundsName => AABB.Create(),
				MonoUtils.BoundsIntName => AABBInt.Create(),
				MonoUtils.QuaternionName => Quaternionf.Create(),
				MonoUtils.Matrix4x4Name => Matrix4x4f.Create(),
				MonoUtils.ColorName => ColorRGBAf.Create(),
				MonoUtils.Color32Name => ColorRGBA32.Create(),
				MonoUtils.LayerMaskName => LayerMask.Create(),
				MonoUtils.AnimationCurveName => AnimationCurve_Single.Create(version),
				//This is the new Gradient. The old one was (most likely) not exposed to user code.
				MonoUtils.GradientName => Gradient.Create(version),
				MonoUtils.RectOffsetName => RectOffset.Create(),
				MonoUtils.GUIStyleName => GUIStyle.Create(version),
				MonoUtils.PropertyNameName => PropertyName.Create(version),
				_ => throw new NotSupportedException(name),
			};
		}
	}
}
