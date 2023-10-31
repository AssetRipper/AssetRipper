﻿using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Mono;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Subclasses.AABB;
using AssetRipper.SourceGenerated.Subclasses.AABBInt;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Single;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;
using AssetRipper.SourceGenerated.Subclasses.Gradient;
using AssetRipper.SourceGenerated.Subclasses.GUIStyle;
using AssetRipper.SourceGenerated.Subclasses.LayerMask;
using AssetRipper.SourceGenerated.Subclasses.Matrix4x4f;
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
			if (assetInfo.Collection.Version.IsEqual(0, 0, 0))
			{
				//Assets with a stripped version can't be read.
				return new UnreadableObject(assetInfo, assetData.ToArray());
			}
			if (assetInfo.ClassID == (int)ClassIDType.MonoBehaviour)
			{
				return ReadMonoBehaviour(MonoBehaviour.Create(assetInfo), assetData, AssemblyManager, assetType);
			}
			IUnityObjectBase? asset = AssetFactory.Create(assetInfo);
			if (asset is null)
			{
				return new UnknownObject(assetInfo, assetData.ToArray());
			}
			else
			{
				IUnityObjectBase readAsset = ReadNormalObject(asset, assetData);
				if (readAsset is UnreadableObject && assetInfo.Collection.Version.Type == UnityVersionType.Patch)
				{
					UnityVersion oldVersion = assetInfo.Collection.Version;
					UnityVersion newVersion = new UnityVersion(oldVersion.Major, oldVersion.Minor, unchecked((ushort)(oldVersion.Build + 1u)));
					IUnityObjectBase? newAsset = AssetFactory.Create(assetInfo, newVersion);
					if (newAsset is not null)
					{
						IUnityObjectBase newReadAsset = ReadNormalObject(newAsset, assetData);
						if (newReadAsset is not UnreadableObject)
						{
							Logger.Warning(LogCategory.Import, $"The asset with a patch version could not be read. It was successfully read on subsequent build number.");
							return newReadAsset;
						}
					}
				}
				return readAsset;
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
					structure = SerializableTreeType.FromRootNode(rootNode).CreateSerializableStructure();
					if (structure.TryRead(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags))
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

		private static IUnityObjectBase ReadNormalObject(IUnityObjectBase asset, ReadOnlyArraySegment<byte> assetData)
		{
			EndianSpanReader reader = new EndianSpanReader(assetData, asset.Collection.EndianType);
			bool replaceWithUnreadableObject;
			try
			{
				asset.Read(ref reader);
				if (reader.Position != reader.Length)
				{
					//Some Chinese Unity versions have extra fields appended to the global type trees.
					if (reader.Length - reader.Position == 24 && asset is ITexture2D texture)
					{
						ReadExtraTextureFields(texture, ref reader);
						replaceWithUnreadableObject = false;
					}
					else
					{
						LogIncorrectNumberOfBytesRead(asset, ref reader);
						replaceWithUnreadableObject = true;
					}
				}
				else
				{
					replaceWithUnreadableObject = false;
				}
			}
			catch (Exception ex)
			{
				LogReadException(asset, ex);
				replaceWithUnreadableObject = true;
			}
			if (replaceWithUnreadableObject)
			{
				UnreadableObject unreadable = new UnreadableObject(asset.AssetInfo, assetData.ToArray());
				unreadable.Name = (asset as INamed)?.Name;
				return unreadable;
			}
			else
			{
				return asset;
			}
		}

		private static void LogIncorrectNumberOfBytesRead(IUnityObjectBase asset, ref EndianSpanReader reader)
		{
			Logger.Error($"Read {reader.Position} but expected {reader.Length} for asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}");
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
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			Logger.Warning(LogCategory.Import, $"Texture {texture.Name} had an extra 24 bytes, which were assumed to be non-standard Chinese fields.");
		}

		private static void LogMonoBehaviorReadException(IMonoBehaviour monoBehaviour, Exception ex)
		{
			Logger.Error(LogCategory.Import, $"Unable to read {monoBehaviour}, because script {monoBehaviour.Structure} layout mismatched binary content ({ex.GetType().Name}).");
		}

		private static void LogReadException(IUnityObjectBase asset, Exception ex)
		{
			Logger.Error(LogCategory.Import, $"Error during reading of asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}", ex);
		}

		public static IUnityAssetBase CreateEngineAsset(string name, UnityVersion version)
		{
			return name switch
			{
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
				MonoUtils.PropertyNameName => throw new NotSupportedException("PropertyName should be treated as a normal string elsewhere in the codebase, currently MonoType."),
				_ => throw new NotSupportedException(name),
			};
		}
	}
}
