using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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
		public override YamlNode ExportYamlEditor(IExportContainer container) => ExportYaml(container);
		public override YamlNode ExportYamlRelease(IExportContainer container) => ExportYaml(container);

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					foreach (PPtr<IUnityObjectBase> asset in Fields[i].FetchDependencies(context, etalon))
					{
						yield return asset;
					}
				}
			}
		}

		public override string ToString()
		{
			if (Type.Namespace.Length == 0)
			{
				return $"{Type.Name}";
			}
			else
			{
				return $"{Type.Namespace}.{Type.Name}";
			}
		}

		private bool IsAvailable(in SerializableType.Field field)
		{
			if (Depth < MaxDepthLevel)
			{
				return true;
			}
			if (field.IsArray)
			{
				return false;
			}
			if (field.Type.Type == PrimitiveType.Complex)
			{
				if (MonoUtils.IsEngineStruct(field.Type.Namespace, field.Type.Name))
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public int Depth { get; }
		public SerializableType Type { get; }
		public SerializableField[] Fields { get; }

		public const int MaxDepthLevel = 8;
	}
}
