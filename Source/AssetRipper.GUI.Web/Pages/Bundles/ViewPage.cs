using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.GUI.Web.Pages.Bundles;

public sealed class ViewPage : DefaultPage
{
	public required Bundle Bundle { get; init; }
	public required BundlePath Path { get; init; }

	public override string GetTitle() => Bundle.Name;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		if (Bundle.Parent is not null)
		{
			new H2(writer).Close(Localization.Parent);
			PathLinking.WriteLink(writer, Path.Parent, Bundle.Parent.Name);
		}

		if (Bundle.Bundles.Count > 0)
		{
			new H2(writer).Close(Localization.Bundles);
			using (new Ul(writer).End())
			{
				(int Index, Bundle Bundle)[] bundles = Bundle.Bundles.Select((bundle, index) => (index, bundle)).ToArray();
				Array.Sort(bundles, (a, b) => string.Compare(a.Bundle.Name, b.Bundle.Name, StringComparison.Ordinal));
				for (int i = 0; i < bundles.Length; i++)
				{
					using (new Li(writer).End())
					{
						PathLinking.WriteLink(writer, Path.GetChild(bundles[i].Index), bundles[i].Bundle.Name);
					}
				}
			}
		}

		if (Bundle.Collections.Count > 0)
		{
			new H2(writer).Close(Localization.Collections);
			using (new Ul(writer).End())
			{
				(int Index, AssetCollection Collection)[] collections = Bundle.Collections.Select((collection, index) => (index, collection)).ToArray();
				Array.Sort(collections, (a, b) => string.Compare(a.Collection.Name, b.Collection.Name, StringComparison.Ordinal));
				for (int i = 0; i < collections.Length; i++)
				{
					AssetCollection collection = collections[i].Collection;
					if (collection.Count > 0 || collection is SerializedAssetCollection)
					{
						using (new Li(writer).End())
						{
							PathLinking.WriteLink(writer, Path.GetCollection(collections[i].Index), collection.Name);
						}
					}
				}
			}
		}

		if (Bundle.Resources.Count > 0)
		{
			new H2(writer).Close(Localization.Resources);
			using (new Ul(writer).End())
			{
				(int Index, ResourceFile Resource)[] resources = Bundle.Resources.Select((resource, index) => (index, resource)).ToArray();
				Array.Sort(resources, (a, b) => string.Compare(a.Resource.Name, b.Resource.Name, StringComparison.Ordinal));
				for (int i = 0; i < resources.Length; i++)
				{
					using (new Li(writer).End())
					{
						PathLinking.WriteLink(writer, Path.GetResource(resources[i].Index), resources[i].Resource.Name);
					}
				}
			}
		}

		if (Bundle.FailedFiles.Count > 0)
		{
			new H2(writer).Close(Localization.FailedFiles);
			using (new Ul(writer).End())
			{
				(int Index, FailedFile FailedFile)[] failedFiles = Bundle.FailedFiles.Select((failedFile, index) => (index, failedFile)).ToArray();
				Array.Sort(failedFiles, (a, b) => string.Compare(a.FailedFile.NameFixed, b.FailedFile.NameFixed, StringComparison.Ordinal));
				for (int i = 0; i < failedFiles.Length; i++)
				{
					using (new Li(writer).End())
					{
						PathLinking.WriteLink(writer, Path.GetFailedFile(failedFiles[i].Index), failedFiles[i].FailedFile.NameFixed);
					}
				}
			}
		}
	}
}
