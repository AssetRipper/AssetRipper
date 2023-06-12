using AsmResolver.DotNet;

namespace AssetRipper.Import.Structure.Assembly.Mono
{
	public static class MonoUtils
	{
		#region Constants
		public const string ObjectName = "Object";
		public const string CObjectName = "object";
		public const string ValueType = "ValueType";
		public const string VoidName = "Void";
		public const string CVoidName = "void";
		public const string BooleanName = "Boolean";
		public const string BoolName = "bool";
		public const string IntPtrName = "IntPtr";
		public const string UIntPtrName = "UIntPtr";
		public const string CharName = "Char";
		public const string CCharName = "char";
		public const string SByteName = "SByte";
		public const string CSByteName = "sbyte";
		public const string ByteName = "Byte";
		public const string CByteName = "byte";
		public const string Int16Name = "Int16";
		public const string ShortName = "short";
		public const string UInt16Name = "UInt16";
		public const string UShortName = "ushort";
		public const string Int32Name = "Int32";
		public const string IntName = "int";
		public const string UInt32Name = "UInt32";
		public const string UIntName = "uint";
		public const string Int64Name = "Int64";
		public const string LongName = "long";
		public const string UInt64Name = "UInt64";
		public const string ULongName = "ulong";
		public const string HalfName = "Half";
		public const string SingleName = "Single";
		public const string FloatName = "float";
		public const string DoubleName = "Double";
		public const string CDoubleName = "double";
		public const string StringName = "String";
		public const string CStringName = "string";

		public const string SystemNamespace = "System";
		public const string SystemCollectionGenericNamespace = "System.Collections.Generic";
		public const string UnityEngineNamespace = "UnityEngine";
		public const string CompilerServicesNamespace = "System.Runtime.CompilerServices";

		public const string CompilerGeneratedName = "CompilerGeneratedAttribute";
		private const string SerializeFieldName = "SerializeField";
		private const string EnumValueFieldName = "value__";


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

		private const string MSCoreLibName = "mscorlib";
		private const string NetStandardName = "netstandard";
		private const string SystemName = "System";
		private const string CLRName = "CommonLanguageRuntimeLibrary";
		private const string UnityEngineName = "UnityEngine";
		private const string BooName = "Boo";
		private const string BooLangName = "Boo.Lang";
		private const string UnityScriptName = "UnityScript";
		private const string UnityScriptLangName = "UnityScript.Lang";
		private const string MonoName = "Mono";

		#endregion

		#region Assemblies
		public static bool IsBuiltinLibrary(string module)
		{
			if (IsFrameworkLibrary(module))
			{
				return true;
			}
			if (IsUnityLibrary(module))
			{
				return true;
			}

			return false;
		}

		public static bool IsFrameworkLibrary(string module)
		{
			return module switch
			{
				MSCoreLibName or NetStandardName or SystemName or CLRName => true,
				_ => module.StartsWith($"{SystemName}.", StringComparison.Ordinal),
			};
		}

		public static bool IsUnityLibrary(string module)
		{
			switch (module)
			{
				case UnityEngineName:
				case BooName:
				case BooLangName:
				case UnityScriptName:
				case UnityScriptLangName:
					return true;

				default:
					{
						if (module.StartsWith($"{UnityEngineName}.", StringComparison.Ordinal))
						{
							return true;
						}
						if (module.StartsWith($"{MonoName}.", StringComparison.Ordinal))
						{
							return true;
						}
						return false;
					}
			}
		}

		#endregion

		#region Attributes
		public static bool IsCompilerGeneratedAttrribute(string? @namespace, string? name)
		{
			if (@namespace == CompilerServicesNamespace)
			{
				return name == CompilerGeneratedName;
			}
			else
			{
				return false;
			}
		}

		public static bool IsSerializeFieldAttrribute(string? @namespace, string? name)
		{
			if (@namespace == UnityEngineNamespace)
			{
				return name == SerializeFieldName;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region Naming
		public static string ToCleanName(string name)
		{
			int openIndex = name.IndexOf('<');
			if (openIndex == -1)
			{
				return name;
			}
			string firstPart = name.Substring(0, openIndex);
			int closeIndex = name.IndexOf('>');
			string secondPart = name.Substring(closeIndex + 1, name.Length - (closeIndex + 1));
			return firstPart + ToCleanName(secondPart);
		}

		public static bool HasMember(ITypeDefOrRef? type, string name)
		{
			if (type == null)
			{
				return false;
			}
			if (type.Module == null)
			{
				return false;
			}
			TypeDefinition? definition = type.Resolve();
			if (definition == null)
			{
				return false;
			}

			foreach (FieldDefinition field in definition.Fields)
			{
				if (field.Name == name)
				{
					return true;
				}
			}
			foreach (PropertyDefinition property in definition.Properties)
			{
				if (property.Name == name)
				{
					return true;
				}
			}
			return HasMember(definition.BaseType, name);
		}

		public static string ToCPrimitiveString(string name)
		{
			return name switch
			{
				StringName or CStringName => CStringName,
				ObjectName or CObjectName => CObjectName,
				_ => ToPrimitiveString(name),
			};
		}

		public static string ToPrimitiveString(string name)
		{
			return name switch
			{
				VoidName or CVoidName => CVoidName,
				BooleanName or BoolName => BoolName,
				IntPtrName => IntPtrName,
				UIntPtrName => UIntPtrName,
				CharName or CCharName => CCharName,
				SByteName or CSByteName => CSByteName,
				ByteName or CByteName => CByteName,
				Int16Name or ShortName => ShortName,
				UInt16Name or UShortName => UShortName,
				Int32Name or IntName => IntName,
				UInt32Name or UIntName => UIntName,
				Int64Name or LongName => LongName,
				UInt64Name or ULongName => ULongName,
				HalfName => HalfName,
				SingleName or FloatName => FloatName,
				DoubleName or CDoubleName => CDoubleName,
				_ => throw new Exception(name),
			};
		}

		#endregion

		#region Boolean ITypeDefOrRef Methods
		public static bool IsPrimitive(ITypeDefOrRef type) => IsPrimitive(type.Namespace, type.Name);
		public static bool IsPrimitive(string? @namespace, string? name)
		{
			if (@namespace == SystemNamespace)
			{
				switch (name)
				{
					case VoidName:
					case CVoidName:
					case BooleanName:
					case BoolName:
					case SByteName:
					case CSByteName:
					case ByteName:
					case CByteName:
					case CharName:
					case CCharName:
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

		public static bool IsCPrimitive(ITypeDefOrRef type) => IsCPrimitive(type.Namespace, type.Name);
		public static bool IsCPrimitive(string? @namespace, string? name)
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

		public static bool IsDelegate(ITypeDefOrRef type) => IsDelegate(type.Namespace, type.Name);
		public static bool IsDelegate(string? @namespace, string? name)
		{
			return @namespace == SystemNamespace && name == MulticastDelegateName;
		}

		public static bool IsObject(ITypeDefOrRef type) => IsObject(type.Namespace, type.Name);
		public static bool IsObject(string? @namespace, string? name)
		{
			return @namespace == SystemNamespace && (name == ObjectName || name == CObjectName);
		}

		public static bool IsString(ITypeDefOrRef type) => IsString(type.Namespace, type.Name);
		public static bool IsString(string? @namespace, string? name)
		{
			return @namespace == SystemNamespace && (name == StringName || name == CStringName);
		}

		public static bool IsList(ITypeDefOrRef type) => IsList(type.Namespace, type.Name);
		public static bool IsList(string? @namespace, string? name)
		{
			return @namespace == SystemCollectionGenericNamespace && name == ListName;
		}

		public static bool IsEngineObject(ITypeDefOrRef type) => IsEngineObject(type.Namespace, type.Name);
		public static bool IsEngineObject(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == ObjectName;
		}

		public static bool IsScriptableObject(ITypeDefOrRef type) => IsScriptableObject(type.Namespace, type.Name);
		public static bool IsScriptableObject(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == ScriptableObjectName;
		}

		public static bool IsComponent(ITypeDefOrRef type) => IsComponent(type.Namespace, type.Name);
		public static bool IsComponent(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == ComponentName;
		}

		public static bool IsBehaviour(ITypeDefOrRef type) => IsBehaviour(type.Namespace, type.Name);
		public static bool IsBehaviour(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == BehaviourName;
		}

		public static bool IsMonoBehaviour(ITypeDefOrRef type) => IsMonoBehaviour(type.Namespace, type.Name);
		public static bool IsMonoBehaviour(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == MonoBehaviourName;
		}

		public static bool IsEngineStruct(ITypeDefOrRef type) => IsEngineStruct(type.Namespace, type.Name);
		public static bool IsEngineStruct(string? @namespace, string? name)
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


		public static bool IsPropertyName(ITypeDefOrRef type) => IsPropertyName(type.Namespace, type.Name);
		public static bool IsPropertyName(string? @namespace, string? name)
		{
			return @namespace is UnityEngineNamespace && name is PropertyNameName;
		}

		public static bool IsExposedReference(ITypeDefOrRef type) => IsExposedReference(type.Namespace, type.Name);
		public static bool IsExposedReference(string? @namespace, string? name)
		{
			return @namespace == UnityEngineNamespace && name == ExposedReferenceName;
		}

		public static bool IsPrime(ITypeDefOrRef type) => IsPrime(type.Namespace, type.Name);
		public static bool IsPrime(string? @namespace, string? name)
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

		public static bool IsMonoPrime(ITypeDefOrRef type) => IsMonoPrime(type.Namespace, type.Name);
		public static bool IsMonoPrime(string? @namespace, string? name)
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

		public static bool IsBuiltinGeneric(ITypeDefOrRef type) => IsBuiltinGeneric(type.Namespace, type.Name);
		public static bool IsBuiltinGeneric(string? @namespace, string? name)
		{
			return IsList(@namespace, name) || IsExposedReference(@namespace, name);
		}
		#endregion

		#region Helpers
		public static PrimitiveType ToPrimitiveType(ITypeDefOrRef? type)
		{
			TypeDefinition? definition = type?.Resolve();
			if (definition?.IsEnum ?? false)
			{
				foreach (FieldDefinition field in definition.Fields)
				{
					if (field.Name == EnumValueFieldName)
					{
						type = field.Signature?.FieldType.ToTypeDefOrRef().Resolve();
						break;
					}
				}
			}

			return ToPrimitiveType(type?.Namespace, type?.Name);
		}

		public static PrimitiveType ToPrimitiveType(string? @namespace, string? name)
		{
			if (@namespace == SystemNamespace)
			{
				return name switch
				{
					VoidName => PrimitiveType.Void,
					BooleanName => PrimitiveType.Bool,
					CharName => PrimitiveType.Char,
					SByteName => PrimitiveType.SByte,
					ByteName => PrimitiveType.Byte,
					Int16Name => PrimitiveType.Short,
					UInt16Name => PrimitiveType.UShort,
					Int32Name => PrimitiveType.Int,
					UInt32Name => PrimitiveType.UInt,
					Int64Name => PrimitiveType.Long,
					UInt64Name => PrimitiveType.ULong,
					HalfName => PrimitiveType.Half,
					SingleName => PrimitiveType.Single,
					DoubleName => PrimitiveType.Double,
					StringName => PrimitiveType.String,
					_ => PrimitiveType.Complex,
				};
			}
			return PrimitiveType.Complex;
		}
		#endregion
	}
}
