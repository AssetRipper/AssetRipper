using AssetRipper.Assets;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Mono;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	public abstract class SerializableType
	{
		public readonly record struct Field(SerializableType Type, int ArrayDepth, string Name, bool Align)
		{
			public override string? ToString()
			{
				if (Type == null)
				{
					return base.ToString();
				}

				return $"{Type}{string.Concat(Enumerable.Repeat("[]", ArrayDepth))} {Name}";
			}

			public bool IsArray => ArrayDepth == 1;
		}

		protected SerializableType(string? @namespace, PrimitiveType type, string name)
		{
			Namespace = @namespace;
			Type = type;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			// is a placeholder - Is assigned by inheriting types.
			Fields = new List<Field>();
		}

		public SerializableStructure CreateSerializableStructure()
		{
			return new SerializableStructure(this, 0);
		}

		public IUnityAssetBase CreateInstance(int depth, UnityVersion version)
		{
			if (MonoUtils.IsEngineStruct(Namespace, Name))
			{
				return GameAssetFactory.CreateEngineAsset(Name, version);
			}
			if (IsEnginePointer())
			{
				return PPtr_Object.Create(version);
			}
			SerializableStructure structure = new(this, depth);
			structure.InitializeFields(version);
			return structure;
		}

		public bool IsPrimitive()
		{
			return MonoUtils.IsPrimitive(Namespace, Name);
		}

		public bool IsString()
		{
			return MonoUtils.IsString(Namespace, Name);
		}

		public bool IsEngineStruct()
		{
			return MonoUtils.IsEngineStruct(Namespace, Name);
		}

		public bool IsEnginePointer()
		{
			return MonoUtils.IsObject(Namespace, Name) || MonoUtils.IsMonoPrime(Namespace, Name);
		}

		public override string ToString() => FullName;

		public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

		public string? Namespace { get; }
		public PrimitiveType Type { get; }
		public string Name { get; }
		public IReadOnlyList<Field> Fields { get; protected set; }
		public virtual int Version => 1;
		public virtual bool FlowMappedInYaml => false;
	}
}
