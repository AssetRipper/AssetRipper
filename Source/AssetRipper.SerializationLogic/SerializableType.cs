namespace AssetRipper.SerializationLogic;

public abstract class SerializableType
{
	public readonly record struct Field(SerializableType Type, int ArrayDepth, string Name, bool Align)
	{
		public override string? ToString()
		{
			if (Type == null)
			{
				return base.ToString();
			}

			return $"{Type}{string.Concat(Enumerable.Repeat("[]", ArrayDepth))} {Name}";
		}

		public bool IsArray => ArrayDepth == 1;
	}

	protected SerializableType(string? @namespace, PrimitiveType type, string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		Namespace = @namespace;
		Type = type;
		Name = name;
	}

	public bool IsPrimitive()
	{
		return Type.IsCSharpPrimitive();
	}

	public bool IsEngineStruct()
	{
		return MonoUtils.IsEngineStruct(Namespace, Name);
	}

	public bool IsEnginePointer()
	{
		return MonoUtils.IsObject(Namespace, Name) || MonoUtils.IsMonoPrime(Namespace, Name);
	}

	public override string ToString() => FullName;

	public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

	public string? Namespace { get; }
	public PrimitiveType Type { get; }
	public string Name { get; }
	public IReadOnlyList<Field> Fields { get; protected set; } = [];
	public virtual int Version => 1;
	public virtual bool FlowMappedInYaml => false;
	/// <summary>
	/// The maximum depth of the structure.
	/// </summary>
	/// <remarks>
	/// A type with no fields has a depth of 0, such as a primitive type, including strings.<br/>
	/// A type with a single field has a depth of 1 + the depth of that field.<br/>
	/// Arrays do not increase depth. For example, a type with a string[] field has a depth of 1, not 2.<br/>
	/// Despite technically having two numeric fields, PPtrs are treated as primitive types with a depth of 0.<br/>
	/// A negative value means that the depth is not yet known.
	/// </remarks>
	public int MaxDepth { get; protected set; } = -1;
	public bool IsMaxDepthKnown => MaxDepth >= 0;
	private HashSet<SerializableType>? _cyclicReferences;

	internal protected void AddCyclicReference(SerializableType other)
	{
		_cyclicReferences ??= [];
		_cyclicReferences.Add(other);
	}

	internal protected bool IsCyclicReference(SerializableType other)
	{
		return _cyclicReferences is not null && _cyclicReferences.Contains(other);
	}
}
