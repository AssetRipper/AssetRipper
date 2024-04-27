using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Mono;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.Yaml;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	public sealed class SerializableStructure : UnityAssetBase
	{
		public override int SerializedVersion => Type.Version;

		internal SerializableStructure(SerializableType type, int depth)
		{
			Depth = depth;
			Type = type ?? throw new ArgumentNullException(nameof(type));
			Fields = new SerializableField[type.FieldCount];
		}

		public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					Fields[i].Read(ref reader, version, flags, Depth, etalon);
				}
			}
		}

		public void Write(AssetWriter writer)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					Fields[i].Write(writer, etalon);
				}
			}
		}
		public override void WriteEditor(AssetWriter writer) => Write(writer);
		public override void WriteRelease(AssetWriter writer) => Write(writer);

		public override void WalkEditor(AssetWalker walker)
		{
			if (walker.EnterAsset(this))
			{
				bool hasSerializedVersion = SerializedVersion > 1;
				if (hasSerializedVersion)
				{
					if (walker.EnterField(this, "serializedVersion"))
					{
						walker.VisitPrimitive(SerializedVersion);
						walker.ExitField(this, "serializedVersion");
					}
				}
				bool hasEmittedFirstField = hasSerializedVersion;
				for (int i = 0; i < Fields.Length; i++)
				{
					SerializableType.Field etalon = Type.GetField(i);
					if (IsAvailable(etalon))
					{
						if (hasEmittedFirstField)
						{
							walker.DivideAsset(this);
						}
						else
						{
							hasEmittedFirstField = true;
						}
						if (walker.EnterField(this, etalon.Name))
						{
							Fields[i].WalkEditor(walker, etalon);
							walker.ExitField(this, etalon.Name);
						}
					}
				}
				walker.ExitAsset(this);
			}
		}
		//For now, only the editor version is implemented.
		public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
		public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

		public override IEnumerable<(string, PPtr)> FetchDependencies()
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					foreach ((string, PPtr) pair in Fields[i].FetchDependencies(etalon))
					{
						yield return pair;
					}
				}
			}
		}

		public override string ToString() => Type.FullName;

		private bool IsAvailable(in SerializableType.Field field)
		{
			if (Depth < GetMaxDepthLevel())
			{
				return true;
			}
			if (field.IsArray)
			{
				return false;
			}
			if (field.Type.Type == PrimitiveType.Complex)
			{
				return MonoUtils.IsEngineStruct(field.Type.Namespace, field.Type.Name);
			}
			return true;
		}

		public bool TryRead(ref EndianSpanReader reader, IMonoBehaviour monoBehaviour)
		{
			try
			{
				Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags);
			}
			catch (Exception ex)
			{
				LogMonoBehaviorReadException(this, ex);
				return false;
			}
			if (reader.Position != reader.Length)
			{
				LogMonoBehaviourMismatch(this, reader.Position, reader.Length);
				return false;
			}
			return true;
		}

		private static void LogMonoBehaviourMismatch(SerializableStructure structure, int actual, int expected)
		{
			Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content (read {actual} bytes, expected {expected} bytes).");
		}

		private static void LogMonoBehaviorReadException(SerializableStructure structure, Exception ex)
		{
			Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content ({ex.GetType().Name}).");
		}

		public int Depth { get; }
		public SerializableType Type { get; }
		public SerializableField[] Fields { get; }

		public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
		{
			if (source is null)
			{
				Reset();
			}
			else
			{
				CopyValues((SerializableStructure)source, converter);
			}
		}

		public void CopyValues(SerializableStructure source, PPtrConverter converter)
		{
			if (source.Type != Type)
			{
				throw new ArgumentException($"Type {source.Type} doesn't match with {Type}", nameof(source));
			}
			if (source.Depth != Depth)
			{
				throw new ArgumentException($"Depth {source.Depth} doesn't match with {Depth}", nameof(source));
			}
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableField sourceField = source.Fields[i];
				if (sourceField.CValue is null)
				{
					Fields[i] = sourceField;
				}
				else
				{
					Fields[i].CopyValues(sourceField, converter.TargetCollection.Version, Depth, Type.Fields[i], converter);
				}
			}
		}

		public SerializableStructure DeepClone(PPtrConverter converter)
		{
			SerializableStructure clone = new(Type, Depth);
			clone.CopyValues(this, converter);
			return clone;
		}

		public override void Reset()
		{
			((Span<SerializableField>)Fields).Clear();
		}

		/// <summary>
		/// 8 might have been an arbitrarily chosen number, but I think it's because the limit was supposedly 7 when mafaca made uTinyRipper.
		/// An official source is required, but forum posts suggest 7 and later 10 as the limits.
		/// It may be desirable to increase this number or do a Unity version check.
		/// </summary>
		private static int GetMaxDepthLevel() => 8;
		//https://forum.unity.com/threads/serialization-depth-limit-and-recursive-serialization.1263599/
		//https://forum.unity.com/threads/getting-a-serialization-depth-limit-7-error-for-no-reason.529850/
		//https://forum.unity.com/threads/4-5-serialization-depth.248321/
	}
}
