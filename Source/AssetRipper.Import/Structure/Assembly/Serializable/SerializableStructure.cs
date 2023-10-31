using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Mono;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	public sealed class SerializableStructure : UnityAssetBase
	{
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

		public YamlMappingNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new();
			node.AddSerializedVersion(Type.Version);
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					node.Add(etalon.Name, Fields[i].ExportYaml(container, etalon));
				}
			}
			return node;
		}
		public override YamlMappingNode ExportYamlEditor(IExportContainer container) => ExportYaml(container);
		public override YamlMappingNode ExportYamlRelease(IExportContainer container) => ExportYaml(container);

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

		public bool TryRead(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
		{
			try
			{
				Read(ref reader, version, flags);
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
