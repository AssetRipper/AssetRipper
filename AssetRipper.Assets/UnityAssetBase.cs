using AssetRipper.Assets.Export;
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
	public virtual void ReadEditor(AssetReader reader)
	{
		throw new NotSupportedException($"Editor reading is not supported for {GetType().FullName}");
	}

	public virtual void ReadRelease(AssetReader reader)
	{
		throw new NotSupportedException($"Release reading is not supported for {GetType().FullName}");
	}

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

	public virtual void WriteEditor(AssetWriter writer)
	{
		throw new NotSupportedException($"Editor writing is not supported for {GetType().FullName}");
	}

	public virtual void WriteRelease(AssetWriter writer)
	{
		throw new NotSupportedException($"Release writing is not supported for {GetType().FullName}");
	}

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

	public virtual YamlNode ExportYamlEditor(IExportContainer container)
	{
		throw new NotSupportedException($"Editor yaml export is not supported for {GetType().FullName}");
	}

	public virtual YamlNode ExportYamlRelease(IExportContainer container)
	{
		throw new NotSupportedException($"Release yaml export is not supported for {GetType().FullName}");
	}

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
}
