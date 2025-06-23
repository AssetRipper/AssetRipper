using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.Export.UnityProjects.Shaders;

public class RequiredProperty
{
	public RequiredProperty() { }
	public RequiredProperty(string name, PropertyType type)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		PropertyName = name;
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
		return PropertyName == property.Name && PropertyType.IsMatch(property.Type);
	}
}
