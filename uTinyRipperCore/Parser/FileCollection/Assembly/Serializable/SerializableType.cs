using System;
using System.Collections.Generic;

namespace uTinyRipper.Assembly
{
	public abstract class SerializableType
	{
		public struct Field
		{
			public Field(SerializableType type, bool isArray, string name)
			{
				Type = type;
				IsArray = isArray;
				Name = name;
			}

			public override string ToString()
			{
				return Type == null ? base.ToString() : (IsArray ? $"{Type}[] {Name}" : $"{Type} {Name}");
			}

			public SerializableType Type { get; }
			public bool IsArray { get; }
			public string Name { get; }
		}

		public SerializableType(string @namespace, PrimitiveType type, string name)
		{
			Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
			Type = type;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public static bool IsPrimitive(string @namespace, string name)
		{
			if(@namespace == SystemNamespace)
			{
				switch (name)
				{
					case MonoUtils.VoidName:
					case MonoUtils.CVoidName:
					case MonoUtils.BooleanName:
					case MonoUtils.BoolName:
					case MonoUtils.ByteName:
					case MonoUtils.CByteName:
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
			if(IsObject(@namespace, name))
			{
				return true;
			}
			if(@namespace == SystemNamespace && name == MonoUtils.ValueType)
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
			if(@namespace == UnityEngineNamespace)
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

		public SerializableStructure CreateBehaviourStructure()
		{
			return ForceCreateComplexStructure(this, 0);
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
			return Namespace ==	string.Empty ? Name : $"{Namespace}.{Name}";
		}

		private static ISerializableStructure CreateComplexStructure(SerializableType type, int depth)
		{
			if (IsEngineStruct(type.Namespace, type.Name))
			{
				return SerializableStructure.EngineTypeToScriptStructure(type.Name);
			}
			if (type.IsEnginePointer())
			{
				return new SerializablePointer(type);
			}
			return ForceCreateComplexStructure(type, depth);
		}

		private static SerializableStructure ForceCreateComplexStructure(SerializableType type, int depth)
		{
			SerializableStructure @base = type.Base == null ? null : ForceCreateComplexStructure(type.Base, depth);
			if (type.Fields.Count > 0 && depth <= MaxDepthLevel)
			{
				List<SerializableField> fields = new List<SerializableField>();
				foreach (Field field in type.Fields)
				{
					if (depth == MaxDepthLevel)
					{
						if (field.Type.Type == PrimitiveType.Complex)
						{
							continue;
						}
						if (field.IsArray)
						{
							continue;
						}
					}

					ISerializableStructure fieldStructure = field.Type.Type == PrimitiveType.Complex ? CreateComplexStructure(field.Type, depth + 1) : null;
					SerializableField sField = new SerializableField(field.Type.Type, fieldStructure, field.IsArray, field.Name);
					fields.Add(sField);
				}
				return new SerializableStructure(type, @base, fields);
			}
			else
			{
				return new SerializableStructure(type, @base, EmptyFields);
			}
		}

		public string Namespace { get; }
		public PrimitiveType Type { get; }
		public string Name { get; }
		public SerializableType Base { get; protected set; }
		public IReadOnlyList<Field> Fields { get; protected set; }

		public const int MaxDepthLevel = 8;

		public const string SystemNamespace = "System";
		public const string SystemCollectionGenericNamespace = "System.Collections.Generic";
		public const string UnityEngineNamespace = "UnityEngine";
		public const string CompilerServicesNamespace = "System.Runtime.CompilerServices";
		
		public const string CompilerGeneratedName = "CompilerGeneratedAttribute";

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

		protected static readonly IReadOnlyList<SerializableField> EmptyFields = new SerializableField[0];
	}
}
