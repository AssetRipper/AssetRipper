using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Game.Assembly
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

#region Naming
		public static bool IsPrimitive(string @namespace, string name)
		{
			if (@namespace == SystemNamespace)
			{
				switch (name)
				{
					case MonoUtils.VoidName:
					case MonoUtils.CVoidName:
					case MonoUtils.BooleanName:
					case MonoUtils.BoolName:
					case MonoUtils.SByteName:
					case MonoUtils.CSByteName:
					case MonoUtils.ByteName:
					case MonoUtils.CByteName:
					case MonoUtils.CharName:
					case MonoUtils.CCharName:
					case MonoUtils.Int16Name:
					case MonoUtils.ShortName:
					case MonoUtils.UInt16Name:
					case MonoUtils.UShortName:
					case MonoUtils.Int32Name:
					case MonoUtils.IntName:
					case MonoUtils.UInt32Name:
					case MonoUtils.UIntName:
					case MonoUtils.Int64Name:
					case MonoUtils.LongName:
					case MonoUtils.UInt64Name:
					case MonoUtils.ULongName:
					case MonoUtils.SingleName:
					case MonoUtils.FloatName:
					case MonoUtils.DoubleName:
					case MonoUtils.CDoubleName:
						return true;
				}
			}
			return false;
		}

		public static bool IsCPrimitive(string @namespace, string name)
		{
			if (IsPrimitive(@namespace, name))
			{
				return true;
			}
			if (IsString(@namespace, name))
			{
				return true;
			}
			if (IsObject(@namespace, name))
			{
				return true;
			}
			return false;
		}

		public static bool IsBasic(string @namespace, string name)
		{
			if (IsObject(@namespace, name))
			{
				return true;
			}
			if (@namespace == SystemNamespace && name == MonoUtils.ValueType)
			{
				return true;
			}
			return false;
		}

		public static bool IsDelegate(string @namespace, string name)
		{
			return @namespace == SystemNamespace && name == MulticastDelegateName;
		}
		public static bool IsObject(string @namespace, string name)
		{
			return @namespace == SystemNamespace && (name == MonoUtils.ObjectName || name == MonoUtils.CObjectName);
		}
		public static bool IsString(string @namespace, string name)
		{
			return @namespace == SystemNamespace && (name == MonoUtils.StringName || name == MonoUtils.CStringName);
		}
		public static bool IsList(string @namespace, string name)
		{
			return @namespace == SystemCollectionGenericNamespace && name == ListName;
		}

		public static bool IsEngineObject(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == MonoUtils.ObjectName;
		}
		public static bool IsScriptableObject(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ScriptableObjectName;
		}
		public static bool IsComponent(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ComponentName;
		}
		public static bool IsBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == BehaviourName;
		}
		public static bool IsMonoBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == MonoBehaviourName;
		}
		public static bool IsEngineStruct(string @namespace, string name)
		{
			if (@namespace == UnityEngineNamespace)
			{
				switch (name)
				{
					case Vector2Name:
					case Vector2IntName:
					case Vector3Name:
					case Vector3IntName:
					case Vector4Name:
					case RectName:
					case BoundsName:
					case BoundsIntName:
					case QuaternionName:
					case Matrix4x4Name:
					case ColorName:
					case Color32Name:
					case LayerMaskName:
					case AnimationCurveName:
					case GradientName:
					case RectOffsetName:
					case GUIStyleName:
						return true;

					case PropertyNameName:
						return true;
				}
			}
			return false;
		}
		public static bool IsExposedReference(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ExposedReferenceName;
		}

		public static bool IsPrime(string @namespace, string name)
		{
			if (IsObject(@namespace, name))
			{
				return true;
			}
			if (IsMonoPrime(@namespace, name))
			{
				return true;
			}
			return false;
		}
		public static bool IsMonoPrime(string @namespace, string name)
		{
			if (IsMonoBehaviour(@namespace, name))
			{
				return true;
			}
			if (IsBehaviour(@namespace, name))
			{
				return true;
			}
			if (IsComponent(@namespace, name))
			{
				return true;
			}
			if (IsEngineObject(@namespace, name))
			{
				return true;
			}

			return false;
		}

		protected static PrimitiveType ToPrimitiveType(string @namespace, string name)
		{
			if (@namespace == SystemNamespace)
			{
				switch (name)
				{
					case MonoUtils.VoidName:
						return PrimitiveType.Void;
					case MonoUtils.BooleanName:
						return PrimitiveType.Bool;
					case MonoUtils.CharName:
						return PrimitiveType.Char;
					case MonoUtils.SByteName:
						return PrimitiveType.SByte;
					case MonoUtils.ByteName:
						return PrimitiveType.Byte;
					case MonoUtils.Int16Name:
						return PrimitiveType.Short;
					case MonoUtils.UInt16Name:
						return PrimitiveType.UShort;
					case MonoUtils.Int32Name:
						return PrimitiveType.Int;
					case MonoUtils.UInt32Name:
						return PrimitiveType.UInt;
					case MonoUtils.Int64Name:
						return PrimitiveType.Long;
					case MonoUtils.UInt64Name:
						return PrimitiveType.ULong;
					case MonoUtils.SingleName:
						return PrimitiveType.Single;
					case MonoUtils.DoubleName:
						return PrimitiveType.Double;
					case MonoUtils.StringName:
						return PrimitiveType.String;

					default:
						return PrimitiveType.Complex;
				}
			}
			return PrimitiveType.Complex;
		}
		#endregion
#region Attributes
		public static bool IsCompilerGeneratedAttrribute(string @namespace, string name)
		{
			if (@namespace == CompilerServicesNamespace)
			{
				return name == CompilerGeneratedName;
			}
			return false;
		}

		public static bool IsSerializeFieldAttrribute(string @namespace, string name)
		{
			if (@namespace == UnityEngineNamespace)
			{
				return name == SerializeFieldName;
			}
			return false;
		}
#endregion

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
			return IsPrimitive(Namespace, Name);
		}

		public bool IsString()
		{
			return IsString(Namespace, Name);
		}

		public bool IsEngineStruct()
		{
			return IsEngineStruct(Namespace, Name);
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
			if (IsEngineStruct(type.Namespace, type.Name))
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

		public const string SystemNamespace = "System";
		public const string SystemCollectionGenericNamespace = "System.Collections.Generic";
		public const string UnityEngineNamespace = "UnityEngine";
		public const string CompilerServicesNamespace = "System.Runtime.CompilerServices";

		public const string CompilerGeneratedName = "CompilerGeneratedAttribute";
		private const string SerializeFieldName = "SerializeField";

		public const string Vector2Name = "Vector2";
		public const string Vector2IntName = "Vector2Int";
		public const string Vector3Name = "Vector3";
		public const string Vector3IntName = "Vector3Int";
		public const string Vector4Name = "Vector4";
		public const string RectName = "Rect";
		public const string BoundsName = "Bounds";
		public const string BoundsIntName = "BoundsInt";
		public const string QuaternionName = "Quaternion";
		public const string Matrix4x4Name = "Matrix4x4";
		public const string ColorName = "Color";
		public const string Color32Name = "Color32";
		public const string LayerMaskName = "LayerMask";
		public const string FloatCurveName = "FloatCurve";
		public const string Vector3CurveName = "Vector3Curve";
		public const string QuaternionCurveName = "QuaternionCurve";
		public const string PPtrCurveName = "PPtrCurve";
		public const string AnimationCurveName = "AnimationCurve";
		public const string GradientName = "Gradient";
		public const string RectOffsetName = "RectOffset";
		public const string GUIStyleName = "GUIStyle";
		public const string PropertyNameName = "PropertyName";

		private const string MulticastDelegateName = "MulticastDelegate";
		private const string ListName = "List`1";
		private const string ExposedReferenceName = "ExposedReference`1";

		private const string ScriptableObjectName = "ScriptableObject";
		private const string ComponentName = "Component";
		private const string BehaviourName = "Behaviour";
		private const string MonoBehaviourName = "MonoBehaviour";

		private int m_baseFieldCount = -1;
	}
}
