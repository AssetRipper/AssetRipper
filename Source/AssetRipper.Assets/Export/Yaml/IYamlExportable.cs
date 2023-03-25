using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.Export.Yaml
{
	public interface IYamlExportable
	{
		YamlNode ExportYamlEditor(IExportContainer container);
		YamlNode ExportYamlRelease(IExportContainer container);
	}
	public static class YamlExportableExtensions
	{
		public static YamlNode ExportYaml(this IYamlExportable @this, IExportContainer container)
		{
			return @this.ExportYaml(container, container.ExportFlags);
		}
		public static YamlNode ExportYaml(this IYamlExportable @this, IExportContainer container, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				return @this.ExportYamlRelease(container);
			}
			else
			{
				return @this.ExportYamlEditor(container);
			}
		}
	}
}
