using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.SerializationLogic;

internal static class EngineTypePredicates
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
	private const string UnityEngineObject = "UnityEngine.Object";

	public const string MonoBehaviour = "MonoBehaviour";
	public const string ScriptableObject = "ScriptableObject";

	public const string UnityEngineNamespace = "UnityEngine";
	public const string SerializeFieldAttribute = "SerializeField";
	public const string SerializeReferenceAttribute = "SerializeReference";

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
}
