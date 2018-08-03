using System;

namespace UtinyRipper.AssetExporters
{
	public class ScriptType
	{
		public ScriptType(PrimitiveType type):
			this(type, null)
		{
		}

		public ScriptType(PrimitiveType type, IScriptStructure complexType)
		{
			if(Type == PrimitiveType.Complex && complexType == null)
			{
				throw new ArgumentNullException(nameof(complexType));
			}
			Type = type;
			ComplexType = complexType;
		}

		public static bool IsDelegate(string @namespace, string name)
		{
			return @namespace == SystemName && name == MulticastDelegateName;
		}
		public static bool IsObject(string @namespace, string name)
		{
			return @namespace == SystemName && name == ObjectName;
		}
		public static bool IsString(string @namespace, string name)
		{
			return @namespace == SystemName && name == StringName;
		}
		public static bool IsList(string @namespace, string name)
		{
			return @namespace == SystemCollectionGenericName && name == ListName;
		}
		
		public static bool IsEngineObject(string @namespace, string name)
		{
			return @namespace == UnityEngineName && name == ObjectName;
		}
		public static bool IsScriptableObject(string @namespace, string name)
		{
			return @namespace == UnityEngineName && name == ScriptableObjectName;
		}
		public static bool IsComponent(string @namespace, string name)
		{
			return @namespace == UnityEngineName && name == ComponentName;
		}
		public static bool IsBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineName && name == BehaviourName;
		}
		public static bool IsMonoBehaviour(string @namespace, string name)
		{
			return @namespace == UnityEngineName && name == MonoBehaviourName;
		}
		public static bool IsEngineStruct(string @namespace, string name)
		{
			if(@namespace == UnityEngineName)
			{
				switch (name)
				{
					case Vector2Name:
					case Vector3Name:
					case Vector4Name:
					case RectName:
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
				}
			}
			return false;
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
			if (IsScriptableObject(@namespace, name))
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
			if (@namespace == SystemName)
			{
				switch (name)
				{
					case VoidName:
						return PrimitiveType.Void;
					case BooleanName:
						return PrimitiveType.Bool;
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

		public override string ToString()
		{
			if (Type == PrimitiveType.Complex)
			{
				return ComplexType.ToString();
			}
			else
			{
				return Type.ToString();
			}
		}

		public PrimitiveType Type { get; }
		public IScriptStructure ComplexType { get; }

		public const string SystemName = "System";
		public const string SystemCollectionGenericName = "System.Collections.Generic";
		public const string UnityEngineName = "UnityEngine";

		private const string ObjectName = "Object";
		private const string VoidName = "Void";
		private const string BooleanName = "Boolean";
		private const string ByteName = "Byte";
		private const string Int16Name = "Int16";
		private const string UInt16Name = "UInt16";
		private const string Int32Name = "Int32";
		private const string UInt32Name = "UInt32";
		private const string Int64Name = "Int64";
		private const string UInt64Name = "UInt64";
		private const string SingleName = "Single";
		private const string DoubleName = "Double";
		private const string StringName = "String";
		private const string MulticastDelegateName = "MulticastDelegate";
		private const string ListName = "List`1";

		public const string Vector2Name = "Vector2";
		public const string Vector3Name = "Vector3";
		public const string Vector4Name = "Vector4";
		public const string RectName = "Rect";
		public const string QuaternionName = "Quaternion";
		public const string Matrix4x4Name = "Matrix4x4";
		public const string ColorName = "Color";
		public const string Color32Name = "Color32";
		public const string LayerMaskName = "LayerMask";
		public const string AnimationCurveName = "AnimationCurve";
		public const string GradientName = "Gradient";
		public const string RectOffsetName = "RectOffset";
		public const string GUIStyleName = "GUIStyle";

		private const string ScriptableObjectName = "ScriptableObject";
		private const string ComponentName = "Component";
		private const string BehaviourName = "Behaviour";
		private const string MonoBehaviourName = "MonoBehaviour";
	}
}
