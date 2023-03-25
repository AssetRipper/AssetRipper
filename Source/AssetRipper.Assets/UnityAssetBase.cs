using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;
using System.Runtime.CompilerServices;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes
/// </summary>
public abstract class UnityAssetBase : IUnityAssetBase
{
	public virtual void ReadEditor(AssetReader reader) => throw MethodNotSupported();

	public virtual void ReadRelease(AssetReader reader) => throw MethodNotSupported();

	public void Read(AssetReader reader)
	{
		if (reader.Flags.IsRelease())
		{
			ReadRelease(reader);
		}
		else
		{
			ReadEditor(reader);
		}
	}

	public virtual void WriteEditor(AssetWriter writer) => throw MethodNotSupported();

	public virtual void WriteRelease(AssetWriter writer) => throw MethodNotSupported();

	public void Write(AssetWriter writer)
	{
		if (writer.Flags.IsRelease())
		{
			WriteRelease(writer);
		}
		else
		{
			WriteEditor(writer);
		}
	}

	public virtual YamlNode ExportYamlEditor(IExportContainer container) => throw MethodNotSupported();

	public virtual YamlNode ExportYamlRelease(IExportContainer container) => throw MethodNotSupported();

	public YamlNode ExportYaml(IExportContainer container)
	{
		if (container.ExportFlags.IsRelease())
		{
			return ExportYamlRelease(container);
		}
		else
		{
			return ExportYamlEditor(container);
		}
	}

	public virtual IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
	{
		return Enumerable.Empty<PPtr<IUnityObjectBase>>();
	}

	public virtual IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies(FieldName? parent)
	{
		return Enumerable.Empty<(FieldName, PPtr<IUnityObjectBase>)>();
	}

	public override string ToString()
	{
		string? name = (this as IHasNameString)?.NameString;
		return string.IsNullOrEmpty(name) ? GetType().Name : name;
	}

	public virtual void Reset() => throw MethodNotSupported();

	public virtual void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
	}

	private Exception MethodNotSupported([CallerMemberName] string? methodName = null)
	{
		return new NotSupportedException($"{methodName} is not supported for {GetType().FullName}");
	}
}
