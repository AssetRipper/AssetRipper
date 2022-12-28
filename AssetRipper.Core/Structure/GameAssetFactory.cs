using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Logging;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
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

namespace AssetRipper.Core.Structure
{
	public sealed class GameAssetFactory : AssetFactoryBase
	{
		public GameAssetFactory(IAssemblyManager assemblyManager)
		{
			AssemblyManager = assemblyManager ?? throw new ArgumentNullException(nameof(assemblyManager));
		}

		private IAssemblyManager AssemblyManager { get; }

		public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, AssetReader reader, int size, SerializedType? type)
		{
			IUnityObjectBase? asset = AssetFactory.CreateAsset(reader.Version, assetInfo);

			return asset switch
			{
				null => ReadUnknownObject(assetInfo, reader, size),
				IMonoBehaviour monoBehaviour => ReadMonoBehaviour(monoBehaviour, reader, size, AssemblyManager, type),
				_ => ReadNormalObject(asset, reader, size)
			};
		}

		private static IUnityObjectBase ReadUnknownObject(AssetInfo assetInfo, AssetReader reader, int size)
		{
			UnknownObject unknownObject = new UnknownObject(assetInfo);
			unknownObject.Read(reader, size);
			return unknownObject;
		}

		private static IMonoBehaviour? ReadMonoBehaviour(IMonoBehaviour monoBehaviour, AssetReader reader, int size, IAssemblyManager assemblyManager, SerializedType? type)
		{
			try
			{
				monoBehaviour.Read(reader);
				if (type is not null && TypeTreeNodeStruct.TryMakeFromTypeTree(type.OldType, out TypeTreeNodeStruct rootNode))
				{
					monoBehaviour.Structure = SerializableTreeType.FromRootNode(rootNode).CreateSerializableStructure();
				}
				else
				{
					IMonoScript? monoScript = GetMonoScript(monoBehaviour);
					monoBehaviour.Structure = monoScript?.GetBehaviourType(assemblyManager)?.CreateSerializableStructure();
				}
				monoBehaviour.Structure?.Read(reader);
				if (monoBehaviour.Structure is not null && reader.BaseStream.Position != size)
				{
					LogMonoBehaviourMismatch(monoBehaviour, reader.BaseStream.Position, size);
					monoBehaviour.Structure = null;
				}
			}
			catch (Exception ex)
			{
				LogMonoBehaviorReadException(monoBehaviour, reader, ex);
				monoBehaviour.Structure = null;
			}
			return monoBehaviour;
		}

		private static IMonoScript? GetMonoScript(IMonoBehaviour monoBehaviour)
		{
			PPtr<IMonoScript> monoScriptPointer = monoBehaviour.Script_C114.ToStruct();
			AssetCollection? monoScriptCollection;
			if (monoScriptPointer.FileID is 0)
			{
				monoScriptCollection = monoBehaviour.Collection;
			}
			else
			{
				SerializedAssetCollection collection = (SerializedAssetCollection)monoBehaviour.Collection;
				if (collection.DependencyIdentifiers is not null && collection.DependencyIdentifiers.Length > 0)
				{
					FileIdentifier identifier = collection.DependencyIdentifiers[monoScriptPointer.FileID - 1];
					monoScriptCollection = collection.Bundle.ResolveCollection(identifier);
				}
				else
				{
					monoScriptCollection = null;
				}
			}
			return monoScriptCollection?.TryGetAsset<IMonoScript>(monoScriptPointer.PathID);
		}

		private static IUnityObjectBase ReadNormalObject(IUnityObjectBase asset, AssetReader reader, int size)
		{
			bool replaceWithUnreadableObject;
			try
			{
				asset.Read(reader);
				if (reader.BaseStream.Position != size)
				{
					//Some Chinese Unity versions have extra fields appended to the global type trees.
					if (size - reader.BaseStream.Position == 24 && asset is ITexture2D texture)
					{
						ReadExtraTextureFields(texture, reader);
						replaceWithUnreadableObject = false;
					}
					else
					{
						LogIncorrectNumberOfBytesRead(asset, reader, size);
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
				replaceWithUnreadableObject = false;
				LogReadException(asset, reader, ex);
			}
			if (replaceWithUnreadableObject)
			{
				reader.BaseStream.Position = 0;
				UnreadableObject unreadable = new UnreadableObject(asset.AssetInfo);
				unreadable.Read(reader, size);
				unreadable.NameString = (asset as IHasNameString)?.NameString ?? "";
				return unreadable;
			}
			else
			{
				return asset;
			}
		}

		private static void LogIncorrectNumberOfBytesRead(IUnityObjectBase asset, AssetReader reader, int size)
		{
			Logger.Error($"Read {reader.BaseStream.Position} but expected {size} for asset type {(ClassIDType)asset.ClassID}. V: {reader.Version} P: {reader.Platform} N: {reader.AssetCollection.Name} Path: {reader.AssetCollection.FilePath}");
		}

		/// <summary>
		/// A special case for Chinese textures containing an extra 24 bytes at the end.
		/// </summary>
		/// <param name="reader"></param>
		private static void ReadExtraTextureFields(ITexture2D texture, AssetReader reader)
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
			Logger.Warning($"Texture {texture.Name} had an extra 24 bytes, which were assumed to be non-standard Chinese fields.");
		}

		private static void LogMonoBehaviourMismatch(IMonoBehaviour monoBehaviour, long actual, int expected)
		{
			Logger.Log(LogType.Error, LogCategory.Import, $"Unable to read {monoBehaviour}, because script {monoBehaviour.Structure} layout mismatched binary content (read {actual} bytes, expected {expected} bytes).");
		}
		
		private static void LogMonoBehaviorReadException(IMonoBehaviour asset, AssetReader reader, Exception ex)
		{
			Logger.Error($"Error during reading of MonoBehavior {GetMonoScript(asset)}. V: {reader.Version} P: {reader.Platform} N: {reader.AssetCollection.Name} Path: {reader.AssetCollection.FilePath}", ex);
		}

		private static void LogReadException(IUnityObjectBase asset, AssetReader reader, Exception ex)
		{
			Logger.Error($"Error during reading of asset type {(ClassIDType)asset.ClassID}. V: {reader.Version} P: {reader.Platform} N: {reader.AssetCollection.Name} Path: {reader.AssetCollection.FilePath}", ex);
		}

		public static IAsset CreateEngineAsset(string name, UnityVersion version)
		{
			return name switch
			{
				MonoUtils.Vector2Name => Vector2fFactory.CreateAsset(version),
				MonoUtils.Vector2IntName => Vector2IntFactory.CreateAsset(),
				MonoUtils.Vector3Name => Vector3fFactory.CreateAsset(version),
				MonoUtils.Vector3IntName => Vector3IntFactory.CreateAsset(),
				MonoUtils.Vector4Name => Vector4fFactory.CreateAsset(version),
				MonoUtils.RectName => RectfFactory.CreateAsset(),
				MonoUtils.BoundsName => AABBFactory.CreateAsset(version),
				MonoUtils.BoundsIntName => AABBIntFactory.CreateAsset(),
				MonoUtils.QuaternionName => QuaternionfFactory.CreateAsset(version),
				MonoUtils.Matrix4x4Name => Matrix4x4fFactory.CreateAsset(),
				MonoUtils.ColorName => ColorRGBAfFactory.CreateAsset(version),
				MonoUtils.Color32Name => ColorRGBA32Factory.CreateAsset(),
				MonoUtils.LayerMaskName => LayerMaskFactory.CreateAsset(),
				MonoUtils.AnimationCurveName => AnimationCurve_SingleFactory.CreateAsset(version),
				MonoUtils.GradientName => GradientFactory.CreateAsset(version),//This is the new Gradient. The old one was (most likely) not exposed to user code.
				MonoUtils.RectOffsetName => RectOffsetFactory.CreateAsset(),
				MonoUtils.GUIStyleName => GUIStyleFactory.CreateAsset(version),
				MonoUtils.PropertyNameName => new PropertyName(),
				_ => throw new NotImplementedException(name),
			};
		}
	}
}
