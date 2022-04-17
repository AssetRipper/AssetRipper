using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Assembly.Mono.Extensions;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetRipper.Core.Structure.Assembly.Mono
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

		public static string ToFullName(string module, string fullname)
		{
			return $"[{module}]{fullname}";
		}

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
		public static bool IsCompilerGeneratedAttrribute(string @namespace, string name)
		{
			if (@namespace == CompilerServicesNamespace)
				return name == CompilerGeneratedName;
			else
				return false;
		}

		public static bool IsSerializeFieldAttrribute(string @namespace, string name)
		{
			if (@namespace == UnityEngineNamespace)
				return name == SerializeFieldName;
			else
				return false;
		}
		#endregion

		#region Naming
		public static string GetNestedName(TypeReference type)
		{
			string typeName = GetTypeName(type);
			return GetNestedName(type, typeName);
		}

		public static string GetNestedName(TypeReference type, string typeName)
		{
			if (type.IsGenericParameter)
			{
				return typeName;
			}
			if (type.IsArray)
			{
				return GetNestedName(type.GetElementType(), typeName);
			}
			if (type.IsNested)
			{
				string declaringName;
				if (type.IsGenericInstance)
				{
					GenericInstanceType generic = (GenericInstanceType)type;
					int argumentCount = MonoUtils.GetGenericArgumentCount(generic);
					List<TypeReference> genericArguments = new List<TypeReference>(generic.GenericArguments.Count - argumentCount);
					for (int i = 0; i < generic.GenericArguments.Count - argumentCount; i++)
					{
						genericArguments.Add(generic.GenericArguments[i]);
					}
					declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				}
				else if (type.HasGenericParameters)
				{
					List<TypeReference> genericArguments = new List<TypeReference>(type.GenericParameters);
					declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				}
				else
				{
					declaringName = GetNestedName(type.DeclaringType);
				}
				return $"{declaringName}.{typeName}";
			}
			return typeName;
		}

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

		public static string GetSimpleName(TypeReference type)
		{
			string name = type.Name;
			int index = name.IndexOf('`');
			if (index == -1)
			{
				return name;
			}

			bool strip = false;
			StringBuilder sb = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (c == '`')
				{
					strip = true;
				}
				else if (!char.IsDigit(c))
				{
					strip = false;
				}

				if (!strip)
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public static string GetTypeName(TypeReference type)
		{
			if (IsCPrimitive(type))
			{
				return ToCPrimitiveString(type.Name);
			}

			if (type.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				return GetGenericInstanceName(generic);
			}
			else if (type.HasGenericParameters)
			{
				return GetGenericTypeName(type);
			}
			else if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				return GetTypeName(array.ElementType) + $"[{new string(',', array.Dimensions.Count - 1)}]";
			}
			return type.Name;
		}

		public static string GetFullName(TypeReference type)
		{
			string module = GetModuleName(type);
			return GetFullName(type, module);
		}

		public static string GetFullName(TypeReference type, string module)
		{
			string name = GetNestedName(type);
			string fullName = $"{type.Namespace}.{name}";
			return ToFullName(module, fullName);
		}

		public static string GetModuleName(TypeReference type)
		{
			// reference and definition may has differrent module, so to avoid duplicates we need try to get defition
			TypeReference definition = type.ResolveOrDefault();
			definition = definition == null ? type : definition;
			return BaseManager.ToAssemblyName(definition.Scope.Name);
		}

		private static string GetNestedGenericName(TypeReference type, List<TypeReference> genericArguments)
		{
			string name = type.Name;
			if (type.HasGenericParameters)
			{
				name = GetGenericTypeName(type, genericArguments);
				int argumentCount = MonoUtils.GetGenericParameterCount(type);
				genericArguments.RemoveRange(genericArguments.Count - argumentCount, argumentCount);
			}
			if (type.IsNested)
			{
				string declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				return $"{declaringName}.{name}";
			}
			else
			{
				return name;
			}
		}

		public static bool HasMember(TypeReference type, string name)
		{
			if (type == null)
			{
				return false;
			}
			if (type.Module == null)
			{
				return false;
			}
			TypeDefinition definition = type.Resolve();
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

		public static string GetName(TypeReference type)
		{
			if (IsCPrimitive(type))
			{
				return ToCPrimitiveString(type.Name);
			}
			else if (type.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				return GetGenericInstanceName(generic);
			}
			else if (type.HasGenericParameters)
			{
				return GetGenericTypeName(type);
			}
			else if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				return GetName(array.ElementType) + $"[{new string(',', array.Dimensions.Count - 1)}]";
			}
			return type.Name;
		}

		internal static string GetGenericTypeName(TypeReference genericType)
		{
			// TypeReference contain parameters with "<!0,!1> (!index)" name but TypeDefinition's name is "<T1,T2> (RealParameterName)"
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericType.GenericParameters);
		}

		internal static string GetGenericTypeName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericArguments);
		}

		internal static string GetGenericInstanceName(GenericInstanceType genericInstance)
		{
			return GetGenericName(genericInstance.ElementType, genericInstance.GenericArguments);
		}

		internal static string GetGenericName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			string name = genericType.Name;
			int argumentCount = GetGenericParameterCount(genericType);
			int index = name.IndexOf('`');
			if (argumentCount == 0 || index < 0)
			{
				// nested class/enum (of generic class) is generic instance but it doesn't have '`' symbol in its name
				return name;
			}

			StringBuilder sb = new StringBuilder(genericType.Name, 0, index, 50 + index);
			sb.Append('<');
			for (int i = genericArguments.Count - argumentCount; i < genericArguments.Count; i++)
			{
				TypeReference arg = genericArguments[i];
				string argumentName = GetName(arg);
				sb.Append(argumentName);
				if (i < genericArguments.Count - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append('>');
			return sb.ToString();
		}

		internal static string GetGenericName<T>(TypeReference genericType, Collection<T> genericArguments) where T : TypeReference
		{
			string name = genericType.Name;
			int argumentCount = GetGenericParameterCount(genericType);
			int index = name.IndexOf('`');
			if (argumentCount == 0 || index < 0)
			{
				// nested class/enum (of generic class) is generic instance but it doesn't have '`' symbol in its name
				return name;
			}

			StringBuilder sb = new StringBuilder(genericType.Name, 0, index, 50 + index);
			sb.Append('<');
			for (int i = genericArguments.Count - argumentCount; i < genericArguments.Count; i++)
			{
				TypeReference arg = genericArguments[i];
				string argumentName = GetName(arg);
				sb.Append(argumentName);
				if (i < genericArguments.Count - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append('>');
			return sb.ToString();
		}
		#endregion

		#region Generics
		public static GenericInstanceType CreateGenericInstance(TypeReference genericTemplate, IEnumerable<TypeReference> arguments)
		{
			GenericInstanceType genericInstance = new GenericInstanceType(genericTemplate);
			foreach (TypeReference argument in arguments)
			{
				genericInstance.GenericArguments.Add(argument);
			}
			return genericInstance;
		}

		public static int GetGenericArgumentCount(GenericInstanceType genericInstance)
		{
			int count = genericInstance.GenericArguments.Count;
			if (genericInstance.IsNested)
			{
				TypeReference declaring = genericInstance.DeclaringType;
				if (declaring.HasGenericParameters)
				{
					count -= declaring.GenericParameters.Count;
				}
			}
			return count;
		}

		public static int GetGenericParameterCount(TypeReference genericType)
		{
			int count = genericType.GenericParameters.Count;
			if (genericType.IsNested)
			{
				TypeReference declaring = genericType.DeclaringType;
				if (declaring.HasGenericParameters)
				{
					count -= declaring.GenericParameters.Count;
				}
			}
			return count;
		}
		#endregion

		#region AreSame
		public static bool AreSame(TypeReference type, MonoTypeContext checkContext, TypeReference checkType)
		{
			if (ReferenceEquals(type, checkType))
				return true;
			else if (type == null || checkType == null)
				return false;

			MonoTypeContext context = new MonoTypeContext(checkType, checkContext);
			MonoTypeContext resolvedContext = context.Resolve();
			return MetadataResolverExtensions.AreSame(type, resolvedContext.Type);
		}

		public static bool AreSame(MethodDefinition method, MonoTypeContext checkContext, MethodDefinition checkMethod)
		{
			if (method.Name != checkMethod.Name)
			{
				return false;
			}
			if (method.HasGenericParameters)
			{
				if (!checkMethod.HasGenericParameters)
					return false;
				if (method.GenericParameters.Count != checkMethod.GenericParameters.Count)
					return false;
				checkContext = checkContext.Merge(checkMethod);
			}
			if (!AreSame(method.ReturnType, checkContext, checkMethod.ReturnType))
			{
				return false;
			}

			if (method.IsVarArg())
			{
				if (!checkMethod.IsVarArg())
					return false;
				if (method.Parameters.Count >= checkMethod.Parameters.Count)
					return false;
				if (checkMethod.GetSentinelPosition() != method.Parameters.Count)
					return false;
			}

			if (method.HasParameters)
			{
				if (!checkMethod.HasParameters)
					return false;
				if (method.Parameters.Count != checkMethod.Parameters.Count)
					return false;

				for (int i = 0; i < method.Parameters.Count; i++)
				{
					if (!AreSame(method.Parameters[i].ParameterType, checkContext, checkMethod.Parameters[i].ParameterType))
					{
						return false;
					}
				}
			}

			return true;
		}
		#endregion

		#region Boolean TypeReference Methods
		public static bool IsPrimitive(TypeReference type) => IsPrimitive(type.Namespace, type.Name);
		public static bool IsPrimitive(string @namespace, string name)
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

		public static bool IsCPrimitive(TypeReference type) => IsCPrimitive(type.Namespace, type.Name);
		public static bool IsCPrimitive(string @namespace, string name)
		{
			if (IsPrimitive(@namespace, name))
				return true;
			if (IsString(@namespace, name))
				return true;
			if (IsObject(@namespace, name))
				return true;
			return false;
		}

		public static bool IsBasic(TypeReference type) => IsBasic(type.Namespace, type.Name);
		public static bool IsBasic(string @namespace, string name)
		{
			if (IsObject(@namespace, name))
				return true;
			if (@namespace == SystemNamespace && name == ValueType)
				return true;
			return false;
		}

		public static bool IsDelegate(TypeReference type) => IsDelegate(type.Namespace, type.Name);
		public static bool IsDelegate(string @namespace, string name)
		{
			return @namespace == SystemNamespace && name == MulticastDelegateName;
		}

		public static bool IsObject(TypeReference type) => IsObject(type.Namespace, type.Name);
		public static bool IsObject(string @namespace, string name)
		{
			return @namespace == SystemNamespace && (name == ObjectName || name == CObjectName);
		}

		public static bool IsString(TypeReference type) => IsString(type.Namespace, type.Name);
		public static bool IsString(string @namespace, string name)
		{
			return @namespace == SystemNamespace && (name == StringName || name == CStringName);
		}

		public static bool IsList(TypeReference type) => IsList(type.Namespace, type.Name);
		public static bool IsList(string @namespace, string name)
		{
			return @namespace == SystemCollectionGenericNamespace && name == ListName;
		}

		public static bool IsEngineObject(TypeReference type) => IsEngineObject(type.Namespace, type.Name);
		public static bool IsEngineObject(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ObjectName;
		}

		public static bool IsScriptableObject(TypeReference type) => IsScriptableObject(type.Namespace, type.Name);
		public static bool IsScriptableObject(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ScriptableObjectName;
		}

		public static bool IsComponent(TypeReference type) => IsComponent(type.Namespace, type.Name);
		public static bool IsComponent(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ComponentName;
		}

		public static bool IsBehaviour(TypeReference type) => IsBehaviour(type.Namespace, type.Name);
		public static bool IsBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == BehaviourName;
		}

		public static bool IsMonoBehaviour(TypeReference type) => IsMonoBehaviour(type.Namespace, type.Name);
		public static bool IsMonoBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == MonoBehaviourName;
		}

		public static bool IsEngineStruct(TypeReference type) => IsEngineStruct(type.Namespace, type.Name);
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

		public static bool IsExposedReference(TypeReference type) => IsExposedReference(type.Namespace, type.Name);
		public static bool IsExposedReference(string @namespace, string name)
		{
			return @namespace == UnityEngineNamespace && name == ExposedReferenceName;
		}

		public static bool IsPrime(TypeReference type) => IsPrime(type.Namespace, type.Name);
		public static bool IsPrime(string @namespace, string name)
		{
			if (IsObject(@namespace, name))
				return true;
			if (IsMonoPrime(@namespace, name))
				return true;
			return false;
		}

		public static bool IsMonoPrime(TypeReference type) => IsMonoPrime(type.Namespace, type.Name);
		public static bool IsMonoPrime(string @namespace, string name)
		{
			if (IsMonoBehaviour(@namespace, name))
				return true;
			if (IsBehaviour(@namespace, name))
				return true;
			if (IsComponent(@namespace, name))
				return true;
			if (IsEngineObject(@namespace, name))
				return true;
			return false;
		}

		public static bool IsBuiltinGeneric(TypeReference type) => IsBuiltinGeneric(type.Namespace, type.Name);
		public static bool IsBuiltinGeneric(string @namespace, string name)
		{
			return IsList(@namespace, name) || IsExposedReference(@namespace, name);
		}
		#endregion

		#region Helpers
		public static bool IsSerializablePrimitive_EType(TypeReference type)
		{
			switch (type.GetEType())
			{
				case ElementType.Boolean:
				case ElementType.Char:
				case ElementType.I1:
				case ElementType.U1:
				case ElementType.I2:
				case ElementType.U2:
				case ElementType.I4:
				case ElementType.U4:
				case ElementType.I8:
				case ElementType.U8:
				case ElementType.R4:
				case ElementType.R8:
				case ElementType.String:
					return true;
				default:
					return false;
			}
		}

		public static bool IsCPrimitive_EType(TypeReference type)
		{
			switch (type.GetEType())
			{
				case ElementType.Boolean:
				case ElementType.Char:
				case ElementType.I:
				case ElementType.U:
				case ElementType.I1:
				case ElementType.U1:
				case ElementType.I2:
				case ElementType.U2:
				case ElementType.I4:
				case ElementType.U4:
				case ElementType.I8:
				case ElementType.U8:
				case ElementType.R4:
				case ElementType.R8:
				case ElementType.String:
				case ElementType.Object:
					return true;

				default:
					return false;
			}
		}

		public static bool IsSerializableArray(TypeReference type)
		{
			return type.IsArray || IsList(type);
		}

		public static bool IsSerializableGeneric(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if (type.IsGenericInstance)
			{
				if (IsBuiltinGeneric(type))
					return true;

				TypeDefinition definition = type.Resolve();
				if (definition.IsEnum)
					return true;

				if (definition.IsSerializable && type is GenericInstanceType git)
				{
					var allSerializableArgs = git.GenericArguments.All(t =>
					{
						if (t is GenericParameter p && arguments.TryGetValue(p, out TypeReference resolved))
							t = resolved;

						if (t.IsGenericInstance)
							return IsSerializableGeneric(t, arguments);

						var resolvedType = t.Resolve();

						if (resolvedType == null)
							return false;

						if (resolvedType.IsSerializable)
							return true;

						if (resolvedType.BaseType?.Resolve()?.IsSerializable == true)
							return true;

						return false;
					});

					return allSerializableArgs;
				}
			}
			return false;
		}

		public static bool IsMonoDerived(TypeReference type)
		{
			while (type != null)
			{
				if (IsMonoPrime(type))
					return true;

				TypeDefinition definition = type.Resolve();
				type = definition.BaseType;
			}
			return false;
		}

		public static bool HasSerializeFieldAttribute(FieldDefinition field)
		{
			foreach (CustomAttribute attribute in field.CustomAttributes)
			{
				TypeReference type = attribute.AttributeType;
				if (IsSerializeFieldAttrribute(type.Namespace, type.Name))
				{
					return true;
				}
			}
			return false;
		}

		public static PrimitiveType ToPrimitiveType(TypeReference type)
		{
			TypeDefinition definition = type.Resolve();
			if (definition.IsEnum)
			{
				foreach (FieldDefinition field in definition.Fields)
				{
					if (field.Name == EnumValueFieldName)
					{
						type = field.FieldType;
						break;
					}
				}
			}

			return ToPrimitiveType(type.Namespace, type.Name);
		}

		public static PrimitiveType ToPrimitiveType(string @namespace, string name)
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

		public static bool IsCompilerGenerated(TypeDefinition type)
		{
			foreach (CustomAttribute attr in type.CustomAttributes)
			{
				if (IsCompilerGeneratedAttrribute(attr.AttributeType.Namespace, attr.AttributeType.Name))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsCompilerGenerated(FieldDefinition field)
		{
			foreach (CustomAttribute attr in field.CustomAttributes)
			{
				if (IsCompilerGeneratedAttrribute(attr.AttributeType.Namespace, attr.AttributeType.Name))
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Serialization
		public static bool IsSerializable(in MonoFieldContext context)
		{
			if (IsSerializableModifier(context.Definition))
			{
				return IsFieldTypeSerializable(context);
			}
			return false;
		}

		public static bool IsSerializableModifier(FieldDefinition field)
		{
			if (field.HasConstant)
				return false;
			else if (field.IsStatic)
				return false;
			else if (field.IsInitOnly)
				return false;
			else if (IsCompilerGenerated(field))
				return false;
			else if (field.IsPublic)
			{
				if (field.IsNotSerialized)
				{
					return false;
				}
				return true;
			}
			else
			{
				return HasSerializeFieldAttribute(field);
			}
		}

		public static bool IsFieldTypeSerializable(in MonoFieldContext context)
		{
			TypeReference fieldType = context.ElementType;

			// if it's generic parameter then get its real type
			if (fieldType.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)fieldType;
				fieldType = context.Arguments[parameter];
			}

			if (fieldType.IsArray)
			{
				ArrayType array = (ArrayType)fieldType;
				// one dimention array only
				if (!array.IsVector)
				{
					return false;
				}

				// if it's generic parameter then get its real type
				TypeReference elementType = array.ElementType;
				if (elementType.IsGenericParameter)
				{
					GenericParameter parameter = (GenericParameter)elementType;
					elementType = context.Arguments[parameter];
				}

				// array of arrays isn't serializable
				if (elementType.IsArray)
				{
					return false;
				}
				// array of serializable generics is serializable
				if (IsSerializableGeneric(elementType, context.Arguments))
				{
					return true;
				}
				// check if array element is serializable
				MonoFieldContext elementScope = new MonoFieldContext(context, elementType, true);
				return IsFieldTypeSerializable(elementScope);
			}

			if (IsList(fieldType))
			{
				// list is serialized same way as array, so check its argument
				GenericInstanceType list = (GenericInstanceType)fieldType;
				TypeReference listElement = list.GenericArguments[0];

				// if it's generic parameter then get its real type
				if (listElement.IsGenericParameter)
				{
					GenericParameter parameter = (GenericParameter)listElement;
					listElement = context.Arguments[parameter];
				}

				// list of arrays isn't serializable
				if (listElement.IsArray)
				{
					return false;
				}
				// list of buildin generics isn't serializable
				if (IsBuiltinGeneric(listElement))
				{
					return false;
				}
				// check if list element is serializable
				MonoFieldContext elementScope = new MonoFieldContext(context, listElement, true);
				return IsFieldTypeSerializable(elementScope);
			}

			if (IsSerializablePrimitive_EType(fieldType))
			{
				return true;
			}
			if (IsObject(fieldType))
			{
				return false;
			}

			if (IsEngineStruct(fieldType))
			{
				return true;
			}
			if (fieldType.IsGenericInstance)
			{
				// even monobehaviour derived generic instances aren't serialiable
				return IsSerializableGeneric(fieldType, context.Arguments);
			}
			if (IsMonoDerived(fieldType))
			{
				if (fieldType.ContainsGenericParameter)
				{
					return false;
				}
				return true;
			}

			if (IsRecursive(context.DeclaringType, fieldType))
			{
				return context.IsArray;
			}

			TypeDefinition definition = fieldType.Resolve();
			if (definition.IsInterface)
			{
				return false;
			}
			if (definition.IsAbstract)
			{
				return false;
			}
			if (IsCompilerGenerated(definition))
			{
				return false;
			}
			if (definition.IsEnum)
			{
				return true;
			}
			if (definition.IsSerializable)
			{
				if (IsFrameworkLibrary(GetModuleName(definition)))
				{
					return false;
				}
				if (definition.IsValueType && !context.Layout.IsStructSerializable)
				{
					return false;
				}
				return true;
			}

			return false;
		}

		private static bool IsRecursive(TypeReference declaringType, TypeReference fieldType)
		{
			// "built in" primitive .NET types are placed into itself... it is so stupid
			// field.FieldType.IsPrimitive || MonoType.IsString(field.FieldType) || MonoType.IsEnginePointer(field.FieldType) => return false
			if (IsDelegate(fieldType))
			{
				return false;
			}
			if (declaringType == fieldType)
			{
				return true;
			}
			return false;
		}
		#endregion
	}
}
