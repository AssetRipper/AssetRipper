using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes
/// </summary>
public abstract class UnityAssetBase : IUnityAssetBase
{
	public virtual void ReadEditor(AssetReader reader) => throw new NotSupportedException();

	public virtual void ReadRelease(AssetReader reader) => throw new NotSupportedException();

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

	public virtual void WriteEditor(AssetWriter writer) => throw new NotSupportedException();

	public virtual void WriteRelease(AssetWriter writer) => throw new NotSupportedException();

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

	public virtual YamlNode ExportYamlEditor(AssetCollection container) => throw new NotSupportedException($"Editor yaml export is not supported for {GetType().FullName}");

	public virtual YamlNode ExportYamlRelease(AssetCollection container) => throw new NotSupportedException($"Release yaml export is not supported for {GetType().FullName}");

	public YamlNode ExportYaml(AssetCollection container)
	{
		if (container.Flags.IsRelease())
		{
			return ExportYamlRelease(container);
		}
		else
		{
			return ExportYamlEditor(container);
		}
	}
}
