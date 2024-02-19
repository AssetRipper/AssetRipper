using AssetRipper.Primitives;
using System.Reflection;

namespace AssetRipper.GUI.SourceGenerator;

internal sealed class PropertyData(PropertyInfo info)
{
	public PropertyInfo Info { get; } = info;
	public string Name => Info.Name;
	public Type PropertyType => Info.PropertyType;
	public Type? DeclaringType => Info.DeclaringType;
	public bool IsBoolean => PropertyType == typeof(bool);
	public bool IsUnityVersion => PropertyType == typeof(UnityVersion);
	public bool IsString => PropertyType == typeof(string) || PropertyType == typeof(Utf8String);
	public bool IsEnum => PropertyType.IsAssignableTo(typeof(Enum));
	public string NameOfString => $"nameof({DeclaringType?.Name}.{Name})";
}
