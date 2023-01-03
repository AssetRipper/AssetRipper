using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class RequiredProperty
	{
		public RequiredProperty() { }
		public RequiredProperty(string name, PropertyType type)
		{
			PropertyName = name ?? throw new ArgumentNullException(nameof(name));
			PropertyType = type;
		}

		public string PropertyName { get; set; } = "";
		public string PropertyTypeName
		{
			get => PropertyType.ToString();
			set => PropertyType = Enum.Parse<PropertyType>(value);
		}
		public PropertyType PropertyType { get; set; }

		public bool IsMatch(ISerializedProperty property)
		{
			return this.PropertyName == property.NameString && PropertyType.IsMatch(property.Type);
		}
	}
}
