using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;

namespace AssetRipper.GUI.Electron.Pages.Assets;

internal static class YamlTabExtensions
{
	public static YamlDocument? GetYamlDocument(this IUnityObjectBase asset)
	{
		GameBundle gameBundle = Program.Ripper.GameStructure.FileCollection;
		TemporaryAssetCollection temporaryFile = gameBundle.AddNewTemporaryBundle().AddNew();
		UIAssetContainer container = new(asset, temporaryFile);
		YamlDocument? result;
		try
		{
			result = asset.ExportYamlDocument(container);
		}
		catch (NotSupportedException)
		{
			result = null;
		}
		gameBundle.ClearTemporaryBundles();
		return result;
	}

	public static string? GetYamlString(this IUnityObjectBase asset)
	{
		YamlDocument? doc = asset.GetYamlDocument();
		if (doc is null)
		{
			return null;
		}
		YamlWriter yamlWriter = new();
		yamlWriter.AddDocument(doc);
		using StringWriter stringWriter = new();
		stringWriter.NewLine = "\n";
		yamlWriter.Write(stringWriter);
		return stringWriter.ToString();
	}

	private sealed class UIAssetContainer : ProjectAssetContainer
	{
		public UIAssetContainer(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile) : base(
			new ProjectExporter(),
			temporaryFile,
			Enumerable.Empty<IUnityObjectBase>(),
			Array.Empty<IExportCollection>())
		{
			Asset = asset;
		}

		public override IReadOnlyList<AssetCollection?> Dependencies => File.Dependencies;

		public IUnityObjectBase Asset { get; }

		public override AssetCollection File => Asset.Collection;

		public override TransferInstructionFlags ExportFlags => File.Flags;
	}
}
