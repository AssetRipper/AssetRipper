using AssetRipper.Core.Converters.Game;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Classes.Misc.Serializable.Gradient;
using AssetRipper.Core.Classes.Misc.Serializable.GUIStyle;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.IO.Asset;
using System;
using System.Collections.Generic;
using static AssetRipper.Core.Structure.Assembly.Mono.MonoUtils;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Math;

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

		public static IAsset CreateEngineAsset(string name)
		{
			switch (name)
			{
				case Vector2Name:
					return new Vector2f();
				case Vector2IntName:
					return new Vector2i();
				case Vector3Name:
					return new Vector3f();
				case Vector3IntName:
					return new Vector3i();
				case Vector4Name:
					return new Vector4f();
				case RectName:
					return new Rectf();
				case BoundsName:
					return new AABB();
				case BoundsIntName:
					return new AABBi();
				case QuaternionName:
					return new Quaternionf();
				case Matrix4x4Name:
					return new Matrix4x4f();
				case ColorName:
					return new ColorRGBAf();
				case Color32Name:
					return new ColorRGBA32();
				case LayerMaskName:
					return new LayerMask();
				case AnimationCurveName:
					return new AnimationCurveTpl<Float>();
				case GradientName:
					return new Gradient();
				case RectOffsetName:
					return new RectOffset();
				case GUIStyleName:
					return new GUIStyle();

				case PropertyNameName:
					return new PropertyName();

				default:
					throw new NotImplementedException(name);
			}
		}

		public TypeTree GenerateTypeTree(AssetLayout layout)
		{
			return SerializableTypeConverter.GenerateTypeTree(layout, this);
		}

		public SerializableStructure CreateSerializableStructure()
		{
			return new SerializableStructure(this, 0);
		}

		public IAsset CreateInstance(int depth)
		{
			return CreateInstance(this, depth);
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

		private static IAsset CreateInstance(SerializableType type, int depth)
		{
			if (MonoUtils.IsEngineStruct(type.Namespace, type.Name))
			{
				return CreateEngineAsset(type.Name);
			}
			if (type.IsEnginePointer())
			{
				return new SerializablePointer();
			}
			return new SerializableStructure(type, depth);
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
