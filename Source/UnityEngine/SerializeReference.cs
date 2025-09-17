namespace UnityEngine;

/// <summary>
/// Instruct Unity to serialize the field as a reference.
/// </summary>
/// <remarks>
/// Unity does not follow the convention that attribute type names end with the "Attribute" suffix.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class SerializeReference : Attribute
{
}
