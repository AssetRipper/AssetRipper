namespace AssetRipper.Export;

public sealed class ObjectHandlerStack<T>
{
	/// <summary>
	/// Exact type to the handlers that handle that type
	/// </summary>
	private readonly Dictionary<Type, Stack<T>> typeMap = new();

	/// <summary>
	/// List of type-handler-allow pairs<br/>
	/// Type: the object type<br/>
	/// T: the handler that can handle that object type<br/>
	/// Bool: allow the handler to apply on inherited object types?
	/// </summary>
	private readonly List<(Type, T, bool)> registeredHandlers = new();

	/// <summary>Adds a handler to the stack of handlers for this object type.</summary>
	/// <param name="type">The c sharp type of this object type. Any inherited types also get this handler.</param>
	/// <param name="handler">The new handler. If it doesn't work, the next one in the stack is used.</param>
	/// <param name="allowInheritance">Should types that inherit from this type also use the handler?</param>
	public void OverrideHandler(Type type, T handler, bool allowInheritance)
	{
		ArgumentNullException.ThrowIfNull(handler);

		registeredHandlers.Add((type, handler, allowInheritance));
		if (typeMap.Count > 0)
		{
			// clear the cache
			typeMap.Clear();
		}
	}

	public IEnumerable<T> GetHandlerStack(Type type)
	{
		if (!typeMap.TryGetValue(type, out Stack<T>? handlers))
		{
			handlers = CalculateAssetHandlerStack(type);
			typeMap.Add(type, handlers);
		}
		return handlers;
	}

	private Stack<T> CalculateAssetHandlerStack(Type type)
	{
		Stack<T> result = new();
		foreach ((Type baseType, T handler, bool allowInheritance) in registeredHandlers)
		{
			if (type == baseType || allowInheritance && type.IsAssignableTo(baseType))
			{
				result.Push(handler);
			}
		}
		return result;
	}
}
