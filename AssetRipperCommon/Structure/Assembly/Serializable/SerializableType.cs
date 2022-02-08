using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.VersionHandling;
using System;
using System.Collections.Generic;
using static AssetRipper.Core.Structure.Assembly.Mono.MonoUtils;

namespace AssetRipper.Core.Structure.Assembly.Serializable
{
	public abstract class SerializableType
	{
		public readonly struct Field
		{
			public Field(SerializableType type, bool isArray, string name)
			{
				Type = type;
				IsArray = isArray;
				Name = name;
			}

			public override string ToString()
			{
				return Type == null ? base.ToString() : IsArray ? $"{Type}[] {Name}" : $"{Type} {Name}";
			}

			public SerializableType Type { get; }
			public bool IsArray { get; }
			public string Name { get; }
		}

		protected SerializableType(string @namespace, PrimitiveType type, string name)
		{
			Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
			Type = type;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public SerializableStructure CreateSerializableStructure()
		{
			return new SerializableStructure(this, 0);
		}

		public IAsset CreateInstance(int depth, UnityVersion version)
		{
			if (MonoUtils.IsEngineStruct(this.Namespace, this.Name))
			{
				return VersionManager.GetHandler(version).AssetFactory.CreateEngineAsset(this.Name);
			}
			if (this.IsEnginePointer())
			{
				return new SerializablePointer();
			}
			return new SerializableStructure(this, depth);
		}

		public Field GetField(int index)
		{
			if (index < BaseFieldCount)
			{
				return Base.GetField(index);
			}
			return Fields[index - BaseFieldCount];
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
			if (IsObject(Namespace, Name))
			{
				return false;
			}
			if (IsMonoPrime(Namespace, Name))
			{
				return true;
			}
			if (Base == null)
			{
				return false;
			}
			return Base.IsEnginePointer();
		}

		public override string ToString()
		{
			return Namespace.Length == 0 ? Name : $"{Namespace}.{Name}";
		}

		public string Namespace { get; }
		public PrimitiveType Type { get; }
		public string Name { get; }
		public SerializableType Base { get; protected set; }
		public IReadOnlyList<Field> Fields { get; protected set; }
		public int FieldCount => BaseFieldCount + Fields.Count;

		internal int BaseFieldCount
		{
			get
			{
				if (m_baseFieldCount < 0)
				{
					m_baseFieldCount = Base == null ? 0 : Base.FieldCount;
				}
				return m_baseFieldCount;
			}
		}

		private int m_baseFieldCount = -1;
	}
}
