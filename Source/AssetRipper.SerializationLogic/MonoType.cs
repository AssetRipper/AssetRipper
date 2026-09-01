using System.Diagnostics;

namespace AssetRipper.SerializationLogic;

internal sealed class MonoType : SerializableType
{
	private MonoType(ITypeDefOrRef type) : base(type.Namespace ?? "", PrimitiveType.Complex, type.Name ?? "")
	{
	}

	internal MonoType(ITypeDefOrRef type, IReadOnlyList<Field> fields) : this(type)
	{
		Fields = fields;
	}

	/// <summary>
	/// True if this type, or any type it contains, has a field with the [SerializeReference] attribute.
	/// </summary>
	/// <remarks>
	/// This is accumulated while the fields are being created, so that it is available on cached types.
	/// </remarks>
	internal bool ContainsSerializeReference { get; set; }

	internal void SetDepth()
	{
		Debug.Assert(IsMaxDepthKnown == false, "The depth of this type is already known.");
		int maxDepth = 0;
		foreach (Field field in Fields)
		{
			if (field.Type.IsMaxDepthKnown)
			{
				maxDepth = Math.Max(maxDepth, field.Type.MaxDepth + 1);
			}
			else
			{
				maxDepth = -1;
				break;
			}
		}
		MaxDepth = maxDepth;
	}
}
