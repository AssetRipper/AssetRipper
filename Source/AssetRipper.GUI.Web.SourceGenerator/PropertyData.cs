using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Primitives;
using System.Reflection;

namespace AssetRipper.GUI.Web.SourceGenerator;

internal sealed class PropertyData(string name)
{
	public string Name { get; } = name;
	public PropertyInfo Info { get; } = GetProperty(name);
	public Type DeclaringType => Info.DeclaringType!;
	public Type PropertyType => Info.PropertyType;
	public bool IsBoolean => PropertyType == typeof(bool);
	public bool IsUnityVersion => PropertyType == typeof(UnityVersion);
	public bool IsString => PropertyType == typeof(string) || PropertyType == typeof(Utf8String);
	public bool IsEnum => PropertyType.IsAssignableTo(typeof(Enum));
	public string NameOfString => $"nameof(Configuration.{Name})";

	private static PropertyInfo GetProperty(string name)
	{
		return typeof(LibraryConfiguration).GetProperty(name) ?? throw new NullReferenceException(name);
	}
}
