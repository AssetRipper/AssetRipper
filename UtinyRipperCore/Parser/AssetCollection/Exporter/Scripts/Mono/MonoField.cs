using Mono.Cecil;

namespace UtinyRipper.AssetExporters.Mono
{
	public class MonoField : ScriptField
	{
		public MonoField(FieldDefinition field):
			base(new MonoType(field.FieldType), IsArrayType(field.FieldType), field.Name)
		{
		}
		
		protected MonoField(MonoField copy):
			base(copy)
		{
		}
		
		public static bool IsSerializableField(FieldDefinition field)
		{
			if (field.IsPublic)
			{
				if(field.IsNotSerialized)
				{
					return false;
				}
				return MonoType.IsSerializableType(field.FieldType);
			}
			else
			{
				foreach (CustomAttribute attr in field.CustomAttributes)
				{
					if (IsSerializeFieldAttrribute(attr))
					{
						return MonoType.IsSerializableType(field.FieldType);
					}
				}
			}
			return false;
		}
		
		private static bool IsSerializeFieldAttrribute(CustomAttribute attribute)
		{
			TypeReference type = attribute.AttributeType;
			return IsSerializeFieldAttrribute(type.Namespace, type.Name);
		}

		private static bool IsArrayType(TypeReference type)
		{
			return type.IsArray || MonoType.IsList(type);
		}

		public override IScriptField CreateCopy()
		{
			return new MonoField(this);
		}
	}
}
