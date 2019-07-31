using Mono.Cecil;
using Mono.Cecil.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace uTinyRipper
{
	public static class MonoUtils
	{
		#region Naming
		public static bool IsSerializablePrimitive(TypeReference type)
		{
			switch (type.etype)
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
					return true;
				default:
					return false;
			}
		}

		public static bool IsCPrimitive(TypeReference type)
		{
			switch (type.etype)
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

		public static string ToCPrimitiveString(string name)
		{
			switch (name)
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

		public static string ToPrimitiveString(string name)
		{
			switch (name)
			{
				case VoidName:
				case CVoidName:
					return CVoidName;
				case BooleanName:
				case BoolName:
					return BoolName;
				case IntPtrName:
					return IntPtrName;
				case UIntPtrName:
					return UIntPtrName;
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

		public static string GetName(TypeReference type)
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
				return GetName(array.ElementType) + $"[{new string(',', array.Dimensions.Count - 1)}]";
			}
			return type.Name;
		}

		private static string GetGenericTypeName(TypeReference genericType)
		{
			// TypeReference contain parameters with "<!0,!1> (!index)" name but TypeDefinition's name is "<T1,T2> (RealParameterName)"
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericType.GenericParameters);
		}

		private static string GetGenericTypeName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericArguments);
		}

		private static string GetGenericInstanceName(GenericInstanceType genericInstance)
		{
			return GetGenericName(genericInstance.ElementType, genericInstance.GenericArguments);
		}

		private static string GetGenericName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			string name = genericType.Name;
			int argumentCount = GetGenericParameterCount(genericType);
			if (argumentCount == 0)
			{
				// nested class/enum (of generic class) is generic instance but it doesn't has '`' symbol in its name
				return name;
			}

			int index = name.IndexOf('`');
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

		public static ArrayType CreateArrayFrom(ArrayType source, TypeReference newType)
		{
			ArrayType newArray = new ArrayType(newType, source.Rank);
			if (source.Rank > 1)
			{
				for (int i = 0; i < source.Rank; i++)
				{
					newArray.Dimensions[i] = source.Dimensions[i];
				}
			}
			return newArray;
		}

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

		/// <summary>
		/// Replace generic parameters with arguments
		/// </summary>
		public static TypeReference ResolveGenericParameter(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if (type.IsGenericParameter)
			{
				GenericParameter generic = (GenericParameter)type;
				TypeReference resolvedType = arguments[generic];
				return resolvedType.ContainsGenericParameter ? ResolveGenericParameter(resolvedType, arguments) : resolvedType;
			}
			if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				TypeReference arrayElement = array.ElementType;
				if (arrayElement.ContainsGenericParameter)
				{
					TypeReference resolvedElement = ResolveGenericParameter(arrayElement, arguments);
					return CreateArrayFrom(array, resolvedElement);
				}
				return array;
			}
			if (type.IsGenericInstance)
			{
				GenericInstanceType genericInstance = (GenericInstanceType)type;
				if (genericInstance.ContainsGenericParameter)
				{
					return ResolveGenericInstanceParameters(genericInstance, arguments);
				}
				return genericInstance;
			}
			throw new Exception($"Unknown generic parameter container {type}");
		}

		/// <summary>
		/// Replace generic parameters in generic instance with arguments
		/// </summary>
		public static GenericInstanceType ResolveGenericInstanceParameters(GenericInstanceType genericInstance, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			GenericInstanceType newInstance = new GenericInstanceType(genericInstance.ElementType);
			foreach (TypeReference argument in genericInstance.GenericArguments)
			{
				TypeReference newArgument = argument.ContainsGenericParameter ? ResolveGenericParameter(argument, arguments) : argument;
				newInstance.GenericArguments.Add(newArgument);
			}
			return newInstance;
		}

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
		public const string SingleName = "Single";
		public const string FloatName = "float";
		public const string DoubleName = "Double";
		public const string CDoubleName = "double";
		public const string StringName = "String";
		public const string CStringName = "string";
	}
}
