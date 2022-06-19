using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Assembly.Serializable;
using Mono.Cecil;
using System.Collections.Generic;
using static AssetRipper.Core.Structure.Assembly.Mono.MonoUtils;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	public sealed class MonoType : SerializableType
	{
		internal MonoType(BaseManager manager, TypeReference type) : this(manager, new MonoTypeContext(type)) { }

		internal MonoType(BaseManager manager, MonoTypeContext context) : base(context.Type.Namespace, ToPrimitiveType(context.Type), MonoUtils.GetName(context.Type))
		{
			if (context.Type.ContainsGenericParameter)
			{
				throw new ArgumentException("Context type contains a generic parameter", nameof(context));
			}
			if (IsSerializableArray(context.Type))
			{
				throw new ArgumentException("Arrays are not valid Mono Types", nameof(context));
			}

			manager.AddSerializableType(context.Type, this);
			Base = GetBaseType(manager, context);
			Fields = CreateFields(manager, context);
		}

		private static SerializableType? GetBaseType(BaseManager manager, MonoTypeContext context)
		{
			MonoTypeContext baseContext = context.GetBase();
			MonoTypeContext resolvedContext = baseContext.Resolve();
			if (IsObject(resolvedContext.Type))
			{
				return null;
			}
			return manager.GetSerializableType(resolvedContext);
		}

		private static Field[] CreateFields(BaseManager manager, MonoTypeContext context)
		{
			List<Field> fields = new List<Field>();
			TypeDefinition definition = context.Type.Resolve();
			IReadOnlyDictionary<GenericParameter, TypeReference> arguments = context.GetContextArguments();
			foreach (FieldDefinition field in definition.Fields)
			{
				MonoFieldContext fieldContext = new MonoFieldContext(field, arguments, manager.Layout);
				if (IsSerializable(fieldContext))
				{
					MonoTypeContext typeContext = new MonoTypeContext(field.FieldType, arguments);
					MonoTypeContext resolvedContext = typeContext.Resolve();
					int depth = 0;
					MonoTypeContext serFieldContext = GetSerializedElementContext(resolvedContext, ref depth);
					if (depth > 1)
					{
						continue;
					}
					SerializableType scriptType = manager.GetSerializableType(serFieldContext);
					Field fieldStruc = new Field(scriptType, depth, field.Name);
					fields.Add(fieldStruc);
				}
			}
			return fields.ToArray();
		}

		private static MonoTypeContext GetSerializedElementContext(MonoTypeContext context, ref int depth)
		{
			if (context.Type.IsArray)
			{
				ArrayType array = (ArrayType)context.Type;
				depth++;
				return GetSerializedElementContext(new MonoTypeContext(array.ElementType), ref depth);
			}
			if (IsList(context.Type))
			{
				GenericInstanceType generic = (GenericInstanceType)context.Type;
				depth++;
				return GetSerializedElementContext(new MonoTypeContext(generic.GenericArguments[0]), ref depth);
			}
			return context;
		}
	}
}
