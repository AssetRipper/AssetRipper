using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class YamlTab(IUnityObjectBase asset) : AssetTab
{
	public string Text { get; } = GetYamlString(asset);
	public string FileName { get; } = $"{asset.GetBestName()}.asset";
	public override string DisplayName => Localization.Yaml;
	public override string HtmlName => "yaml";
	public override bool Enabled => !string.IsNullOrEmpty(Text);

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Text);
		using (new Div(writer).WithClass("text-center").End())
		{
			TextSaveButton.Write(writer, FileName, Text);
		}
	}

	private static string GetYamlString(IUnityObjectBase asset)
	{
		YamlDocument? doc = GetYamlDocument(asset);
		if (doc is null)
		{
			return "";
		}
		YamlWriter yamlWriter = new();
		yamlWriter.AddDocument(doc);
		using StringWriter stringWriter = new();
		stringWriter.NewLine = "\n";
		yamlWriter.Write(stringWriter);
		return stringWriter.ToString();
	}

	private static YamlDocument? GetYamlDocument(IUnityObjectBase asset)
	{
		GameBundle gameBundle = GameFileLoader.GameBundle;
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

	private sealed class UIAssetContainer : ProjectAssetContainer
	{
		public UIAssetContainer(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile) : base(
			new ProjectExporter(new(), new BaseManager((a) => { })),
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
