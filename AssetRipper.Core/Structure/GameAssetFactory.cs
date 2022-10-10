using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Classes.Misc.Serializable.GUIStyle;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Subclasses.AABB;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Single;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;
using AssetRipper.SourceGenerated.Subclasses.Gradient;
using AssetRipper.SourceGenerated.Subclasses.Matrix4x4f;
using AssetRipper.SourceGenerated.Subclasses.Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.Rectf;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector2Int;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using AssetRipper.SourceGenerated.Subclasses.Vector3Int;
using AssetRipper.SourceGenerated.Subclasses.Vector4f;

namespace AssetRipper.Core.Structure
{
	public sealed class GameAssetFactory : AssetFactoryBase
	{
		public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, AssetReader reader, int size, SerializedType? type)
		{
			IUnityObjectBase? asset = SourceGenerated.AssetFactory.CreateAsset(reader.Version, assetInfo);

			return asset switch
			{
				null => ReadUnknownObject(assetInfo, reader, size),
				IMonoBehaviour monoBehaviour => ReadMonoBehaviour(monoBehaviour, reader, size),
				_ => ReadNormalObject(asset, reader, size)
			};
		}

		private static IUnityObjectBase ReadUnknownObject(AssetInfo assetInfo, AssetReader reader, int size)
		{
			UnknownObject unknownObject = new UnknownObject(assetInfo);
			unknownObject.Read(reader, size);
			return unknownObject;
		}

		private static IMonoBehaviour ReadMonoBehaviour(IMonoBehaviour monoBehaviour, AssetReader reader, int size)
		{
			try
			{
				monoBehaviour.Read(reader);
				//SerializableStructure assignment
				if (monoBehaviour.Structure is not null && reader.BaseStream.Position != size)
				{
					monoBehaviour.Structure = null;
					LogMonoBehaviourMismatch(monoBehaviour);
				}
			}
			catch (Exception ex)
			{
				monoBehaviour.Structure = null;
				LogReadException(monoBehaviour, reader, ex);
			}
			return monoBehaviour;
		}

		private static IUnityObjectBase ReadNormalObject(IUnityObjectBase asset, AssetReader reader, int size)
		{
			bool replaceWithUnreadableObject;
			try
			{
				asset.Read(reader);
				if (reader.BaseStream.Position != size)
				{
					Logger.Error($"Read {reader.BaseStream.Position} but expected {size} for asset type {asset.ClassID}. V: {reader.Version} P: {reader.Platform} N: {reader.AssetCollection.Name} Path: {reader.AssetCollection.FilePath}");
					replaceWithUnreadableObject = true;
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

		private static void LogMonoBehaviourMismatch(IMonoBehaviour monoBehaviour)
		{
			Logger.Log(LogType.Error, LogCategory.Import, $"Unable to read {monoBehaviour}, because script layout mismatched binary content.");
		}

		private static void LogReadException(IUnityObjectBase asset, AssetReader reader, Exception ex)
		{
			Logger.Error($"Error during reading of asset type {asset.ClassID}. V: {reader.Version} P: {reader.Platform} N: {reader.AssetCollection.Name} Path: {reader.AssetCollection.FilePath}", ex);
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
				MonoUtils.BoundsIntName => new AABBi(),
				MonoUtils.QuaternionName => QuaternionfFactory.CreateAsset(version),
				MonoUtils.Matrix4x4Name => Matrix4x4fFactory.CreateAsset(),
				MonoUtils.ColorName => ColorRGBAfFactory.CreateAsset(version),
				MonoUtils.Color32Name => ColorRGBA32Factory.CreateAsset(),
				MonoUtils.LayerMaskName => new LayerMask(),
				MonoUtils.AnimationCurveName => AnimationCurve_SingleFactory.CreateAsset(version),
				MonoUtils.GradientName => GradientFactory.CreateAsset(version),//This is the new Gradient. The old one was (most likely) not exposed to user code.
				MonoUtils.RectOffsetName => new RectOffset(),
				MonoUtils.GUIStyleName => new GUIStyle(),
				MonoUtils.PropertyNameName => new PropertyName(),
				_ => throw new NotImplementedException(name),
			};
		}
	}
}
