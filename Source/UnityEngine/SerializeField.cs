namespace UnityEngine;

/// <summary>
/// Instruct Unity to serialize a non-public field.
/// </summary>
/// <remarks>
/// Unity does not follow the convention that attribute type names end with the "Attribute" suffix.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class SerializeField : Attribute
{
}
