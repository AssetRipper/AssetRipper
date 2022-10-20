using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.Yaml;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes
/// </summary>
public abstract class UnityAssetBase : IUnityAssetBase
{
	public virtual void ReadEditor(AssetReader reader) => throw MethodNotSupported(nameof(ReadEditor));

	public virtual void ReadRelease(AssetReader reader) => throw MethodNotSupported(nameof(ReadRelease));

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

	public virtual void WriteEditor(AssetWriter writer) => throw MethodNotSupported(nameof(WriteEditor));

	public virtual void WriteRelease(AssetWriter writer) => throw MethodNotSupported(nameof(WriteRelease));

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

	public virtual YamlNode ExportYamlEditor(IExportContainer container) => throw MethodNotSupported(nameof(ExportYamlEditor));

	public virtual YamlNode ExportYamlRelease(IExportContainer container) => throw MethodNotSupported(nameof(ExportYamlRelease));

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

	public virtual List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex) => throw MethodNotSupported(nameof(MakeEditorTypeTreeNodes));

	public virtual List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex) => throw MethodNotSupported(nameof(MakeReleaseTypeTreeNodes));

	public override string? ToString()
	{
		return this is IHasNameString hasName ? hasName.NameString : base.ToString();
	}

	public virtual void Reset() => throw MethodNotSupported(nameof(Reset));

	public virtual void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
	}

	private Exception MethodNotSupported(string methodName)
	{
		return new NotSupportedException($"{methodName} is not supported for {GetType().FullName}");
	}
}
