using System;
using System.Collections.Generic;

namespace uTinyRipper.Assembly
{
#warning TODO: rename
	public abstract class ScriptType
	{
		public struct Field
		{
			public Field(ScriptType type, bool isArray, string name)
			{
				Type = type;
				IsArray = isArray;
				Name = name;
			}

			public override string ToString()
			{
				return Type == null ? base.ToString() : (IsArray ? $"{Type}[] {Name}" : $"{Type} {Name}");
			}

			public ScriptType Type { get; }
			public bool IsArray { get; }
			public string Name { get; }
		}

		public ScriptType(string @namespace, PrimitiveType type, string name, ScriptType @base)
		{
			Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
			Type = type;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Base = @base;
		}

		public static bool IsPrimitive(string @namespace, string name)
		{
			if(@namespace == SystemNamespace)
			{
				switch (name)
				{
					case VoidName:
					case CVoidName:
					case BooleanName:
					case BoolName:
					case ByteName:
					case CByteName:
					case Int16Name:
					case ShortName:
					case UInt16Name:
					case UShortName:
					case Int32Name:
					case IntName:
					case UInt32Name:
					case UIntName:
					case Int64Name:
					case LongName:
					case UInt64Name:
					case ULongName:
					case SingleName:
					case FloatName:
					case DoubleName:
					case CDoubleName:
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
			if(@namespace == SystemNamespace && name == ValueType)
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
			return @namespace == SystemNamespace && (name == ObjectName || name == CObjectName);
		}
		public static bool IsString(string @namespace, string name)
		{
			return @namespace == SystemNamespace && (name == StringName || name == CStringName);
		}
		public static bool IsList(string @namespace, string name)
		{
			return @namespace == SystemCollectionGenericNamespace && name == ListName;
		}
		
		public static bool IsEngineObject(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ObjectName;
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

		public static string ToPrimitiveString(string name)
		{
			switch(name)
			{
				case VoidName:
				case CVoidName:
					return CVoidName;
				case BooleanName:
				case BoolName:
					return BoolName;
				case CharName:
				case CCharName:
					return CCharName;
				case SByteName:
				case CSByteName:
					return CSByteName;
				case ByteName:
				case CByteName:
					return CByteName;
				case Int16Name:
				case ShortName:
					return ShortName;
				case UInt16Name:
				case UShortName:
					return UShortName;
				case Int32Name:
				case IntName:
					return IntName;
				case UInt32Name:
				case UIntName:
					return UIntName;
				case Int64Name:
				case LongName:
					return LongName;
				case UInt64Name:
				case ULongName:
					return ULongName;
				case SingleName:
				case FloatName:
					return FloatName;
				case DoubleName:
				case CDoubleName:
					return CDoubleName;
				default:
					throw new Exception(name);
			}
		}

		public static string ToCPrimitiveString(string name)
		{
			switch(name)
			{
				case StringName:
				case CStringName:
					return CStringName;

				case ObjectName:
				case CObjectName:
					return CObjectName;

				default:
					return ToPrimitiveString(name);
			}
		}

		protected static PrimitiveType ToPrimitiveType(string @namespace, string name)
		{
			if (@namespace == SystemNamespace)
			{
				switch (name)
				{
					case VoidName:
						return PrimitiveType.Void;
					case BooleanName:
						return PrimitiveType.Bool;
					case CharName:
						return PrimitiveType.Char;
					case SByteName:
						return PrimitiveType.SByte;
					case ByteName:
						return PrimitiveType.Byte;
					case Int16Name:
						return PrimitiveType.Short;
					case UInt16Name:
						return PrimitiveType.UShort;
					case Int32Name:
						return PrimitiveType.Int;
					case UInt32Name:
						return PrimitiveType.UInt;
					case Int64Name:
						return PrimitiveType.Long;
					case UInt64Name:
						return PrimitiveType.ULong;
					case SingleName:
						return PrimitiveType.Single;
					case DoubleName:
						return PrimitiveType.Double;
					case StringName:
						return PrimitiveType.String;

					default:
						return PrimitiveType.Complex;
				}
			}
			return PrimitiveType.Complex;
		}

		public ScriptStructure CreateBehaviourStructure()
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

		private static IScriptStructure CreateComplexStructure(ScriptType type, int depth)
		{
			if (IsEngineStruct(type.Namespace, type.Name))
			{
				return ScriptStructure.EngineTypeToScriptStructure(type.Name);
			}
			if (type.IsEnginePointer())
			{
				return new ScriptPointer(type);
			}
			return ForceCreateComplexStructure(type, depth);
		}

		private static ScriptStructure ForceCreateComplexStructure(ScriptType type, int depth)
		{
			ScriptStructure @base = type.Base == null ? null : ForceCreateComplexStructure(type.Base, depth);
			if (type.Fields.Count > 0 && depth <= MaxDepthLevel)
			{
				List<ScriptField> fields = new List<ScriptField>();
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

					IScriptStructure fieldStructure = field.Type.Type == PrimitiveType.Complex ? CreateComplexStructure(field.Type, depth + 1) : null;
					ScriptField sField = new ScriptField(field.Type.Type, fieldStructure, field.IsArray, field.Name);
					fields.Add(sField);
				}
				return new ScriptStructure(type, @base, fields);
			}
			else
			{
				return new ScriptStructure(type, @base, EmptyFields);
			}
		}

		public string Namespace { get; }
		public PrimitiveType Type { get; }
		public string Name { get; }
		public ScriptType Base { get; }
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

		private const string ObjectName = "Object";
		private const string CObjectName = "object";
		private const string ValueType = "ValueType";
		private const string VoidName = "Void";
		private const string CVoidName = "void";
		private const string BooleanName = "Boolean";
		private const string BoolName = "bool";
		private const string CharName = "Char";
		private const string CCharName = "char";
		private const string SByteName = "SByte";
		private const string CSByteName = "sbyte";
		private const string ByteName = "Byte";
		private const string CByteName = "byte";
		private const string Int16Name = "Int16";
		private const string ShortName = "short";
		private const string UInt16Name = "UInt16";
		private const string UShortName = "ushort";
		private const string Int32Name = "Int32";
		private const string IntName = "int";
		private const string UInt32Name = "UInt32";
		private const string UIntName = "uint";
		private const string Int64Name = "Int64";
		private const string LongName = "long";
		private const string UInt64Name = "UInt64";
		private const string ULongName = "ulong";
		private const string SingleName = "Single";
		private const string FloatName = "float";
		private const string DoubleName = "Double";
		private const string CDoubleName = "double";
		private const string StringName = "String";
		private const string CStringName = "string";
		private const string MulticastDelegateName = "MulticastDelegate";
		private const string ListName = "List`1";
		private const string ExposedReferenceName = "ExposedReference`1";

		private const string ScriptableObjectName = "ScriptableObject";
		private const string ComponentName = "Component";
		private const string BehaviourName = "Behaviour";
		private const string MonoBehaviourName = "MonoBehaviour";

		protected static readonly IReadOnlyList<ScriptField> EmptyFields = new ScriptField[0];
	}
}
