using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.SerializationLogic;

public class EngineTypePredicates
{
	private static readonly HashSet<string> TypesThatShouldHaveHadSerializableAttribute = new HashSet<string>
	{
		"Vector3",
		"Vector2",
		"Vector4",
		"Rect",
		"RectInt",
		"Quaternion",
		"Matrix4x4",
		"Color",
		"Color32",
		"LayerMask",
		"Bounds",
		"BoundsInt",
		"Vector3Int",
		"Vector2Int",
	};

	private const string Gradient = "UnityEngine.Gradient";
	private const string GUIStyle = "UnityEngine.GUIStyle";
	private const string RectOffset = "UnityEngine.RectOffset";
	protected const string UnityEngineObject = "UnityEngine.Object";

	public const string MonoBehaviour = "MonoBehaviour";
	public const string ScriptableObject = "ScriptableObject";

	public const string MonoBehaviourFullName = $"{UnityEngineNamespace}.{MonoBehaviour}";
	public const string ScriptableObjectFullName = $"{UnityEngineNamespace}.{ScriptableObject}";
	protected const string Matrix4x4 = "UnityEngine.Matrix4x4";
	protected const string Color32 = "UnityEngine.Color32";

	public const string UnityEngineNamespace = "UnityEngine";
	private const string SerializeFieldAttribute = "SerializeField";
	private const string SerializeReferenceAttribute = "SerializeReference";

	private static readonly string[] serializableClasses =
	[
		"AnimationCurve",
		"Gradient",
		"GUIStyle",
		"RectOffset"
	];

	private static readonly string[] serializableStructs =
	[
		// NOTE: assumes all types here are NOT interfaces
		"UnityEngine.Color32",
		"UnityEngine.Matrix4x4",
		"UnityEngine.Rendering.SphericalHarmonicsL2",
		"UnityEngine.PropertyName",
	];

	public static bool IsMonoBehaviour(ITypeDescriptor type)
	{
		return IsMonoBehaviour(type.CheckedResolve());
	}

	private static bool IsMonoBehaviour(TypeDefinition typeDefinition)
	{
		return typeDefinition.IsSubclassOf(MonoBehaviourFullName);
	}

	public static bool IsScriptableObject(ITypeDescriptor type)
	{
		return IsScriptableObject(type.CheckedResolve());
	}

	private static bool IsScriptableObject(TypeDefinition temp)
	{
		return temp.IsSubclassOf(ScriptableObjectFullName);
	}

	public static bool IsColor32(ITypeDescriptor type)
	{
		return type.IsAssignableTo(Color32);
	}

	//Do NOT remove these, cil2as still depends on these in 4.x
	public static bool IsMatrix4x4(ITypeDescriptor type)
	{
		return type.IsAssignableTo(Matrix4x4);
	}

	public static bool IsGradient(ITypeDescriptor type)
	{
		return type.IsAssignableTo(Gradient);
	}

	public static bool IsGUIStyle(ITypeDescriptor type)
	{
		return type.IsAssignableTo(GUIStyle);
	}

	public static bool IsRectOffset(ITypeDescriptor type)
	{
		return type.IsAssignableTo(RectOffset);
	}

	public static bool IsSerializableUnityClass(ITypeDescriptor type)
	{
		foreach (string unityClasses in serializableClasses)
		{
			if (type.IsAssignableTo(UnityEngineNamespace, unityClasses))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsSerializableUnityStruct(ITypeDescriptor type)
	{
		foreach (string unityStruct in serializableStructs)
		{
			// NOTE: structs cannot inherit from structs, and can only inherit from interfaces
			//	   since we know all types in serializableStructs are not interfaces,
			//	   we can just do a direct comparison.
			if (type.FullName == unityStruct)
			{
				return true;
			}
		}

		if (type.FullName.StartsWith("UnityEngine.LazyLoadReference`1", StringComparison.Ordinal))
		{
			return true;
		}

		return false;
	}

	public static bool IsUnityEngineObject(ITypeDescriptor type)
	{
		//todo: somehow solve this elegantly. CheckedResolve() drops the [] of a type.
		if (type.IsArray())
		{
			return false;
		}

		if (type is { Namespace: UnityEngineNamespace, Name: nameof(Object) })
		{
			return true;
		}

		TypeDefinition? typeDefinition = type.Resolve();
		if (typeDefinition == null)
		{
			return false;
		}

		return typeDefinition.IsSubclassOf(UnityEngineNamespace, nameof(Object));
	}

	public static bool ShouldHaveHadSerializableAttribute(ITypeDescriptor type)
	{
		return IsUnityEngineValueType(type);
	}

	public static bool IsUnityEngineValueType(ITypeDescriptor type)
	{
		return type.SafeNamespace() == "UnityEngine" && TypesThatShouldHaveHadSerializableAttribute.Contains(type.Name ?? "");
	}

	public static bool IsSerializeFieldAttribute(ITypeDescriptor attributeType)
	{
		return attributeType is { Namespace: UnityEngineNamespace, Name: SerializeFieldAttribute };
	}

	public static bool IsSerializeReferenceAttribute(ITypeDescriptor attributeType)
	{
		return attributeType is { Namespace: UnityEngineNamespace, Name: SerializeReferenceAttribute };
	}
}
