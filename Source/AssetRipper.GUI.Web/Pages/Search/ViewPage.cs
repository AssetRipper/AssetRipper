using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Search;

public sealed class ViewPage : DefaultPage
{
	public required string SearchQuery { get; init; }

	public override string GetTitle() => $"{Localization.Search}: {SearchQuery}";

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(Localization.Search);

		// Search form
		using (new Form(writer).WithAction(SearchAPI.Urls.View).WithMethod("get").WithClass("d-flex align-items-center mb-3").End())
		{
			new Input(writer)
				.WithType("text")
				.WithId("searchQuery")
				.WithName(SearchAPI.Query)
				.WithValue(SearchQuery.ToHtml())
				.WithPlaceholder(Localization.SearchPlaceholder)
				.WithClass("form-control me-2")
				.WithStyle("max-width: 50%;")
				.Close();

			new Button(writer).WithType("submit").WithClass("btn btn-primary").Close(Localization.Search);
		}

		if (string.IsNullOrWhiteSpace(SearchQuery))
		{
			new P(writer).Close(Localization.EnterSearchQuery);
			return;
		}

		// Perform search
		var results = PerformSearch(SearchQuery);

		if (results.Count == 0)
		{
			new P(writer).Close(Localization.NoResultsFound);
			return;
		}

		new H2(writer).Close($"{Localization.Results}: {results.Count}");

		// Class filter - only show if there are multiple class types
		var availableClasses = results.Select(r => r.Asset.ClassName).Distinct().Order().ToList();
		if (availableClasses.Count > 1)
		{
			new Label(writer).WithFor("classFilter").WithClass("me-2").Close(Localization.Class);

			using (new Select(writer)
				.WithId("classFilter")
				.WithClass("me-2")
				.End())
			{
				new Option(writer)
					.WithValue(string.Empty)
					.Close(Localization.All);

				foreach (string cn in availableClasses)
				{
					new Option(writer)
						.WithValue(cn)
						.Close(cn);
				}
			}
		}

		// Results table
		using (new Table(writer).WithClass("table").End())
		{
			using (new Thead(writer).End())
			{
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.PathId);
					new Th(writer).Close(Localization.Class);
					new Th(writer).Close(Localization.Name);
					new Th(writer).Close(Localization.Collection);
				}
			}
			using (new Tbody(writer).WithId("resultsTable").End())
			{
				foreach (SearchResult result in results)
				{
					using (new Tr(writer).WithCustomAttribute("data-class", result.Asset.ClassName).End())
					{
						new Td(writer).Close(result.Asset.PathID.ToString());
						new Td(writer).Close(result.Asset.ClassName);
						using (new Td(writer).End())
						{
							PathLinking.WriteLink(writer, result.Path, result.Asset.GetBestName());
						}
						using (new Td(writer).End())
						{
							PathLinking.WriteLink(writer, result.CollectionPath, result.Collection.Name);
						}
					}
				}
			}
		}
	}

	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		
		// Add client-side filtering script
		using (new Script(writer).End())
		{
			writer.Write("""
				document.addEventListener('DOMContentLoaded', function() {
					const classFilter = document.getElementById('classFilter');
					if (classFilter) {
						classFilter.addEventListener('change', function() {
							const selectedClass = this.value;
							const rows = document.querySelectorAll('#resultsTable tr');
							
							rows.forEach(function(row) {
								const rowClass = row.getAttribute('data-class');
								if (selectedClass === '' || rowClass === selectedClass) {
									row.style.display = '';
								} else {
									row.style.display = 'none';
								}
							});
						});
					}
				});
				""");
		}
	}

	private List<SearchResult> PerformSearch(string query)
	{
		List<SearchResult> results = [];
		GameBundle bundle = GameFileLoader.GameBundle;

		foreach (AssetCollection collection in bundle.FetchAssetCollections())
		{
			CollectionPath collectionPath = collection.GetPath();

			foreach (IUnityObjectBase asset in collection)
			{
				if (Match(asset, query))
				{
					results.Add(new SearchResult
					{
						Asset = asset,
						Collection = collection,
						CollectionPath = collectionPath,
						Path = collectionPath.GetAsset(asset.PathID)
					});
				}
			}
		}

		return results;
	}

	private static bool Match(IUnityObjectBase asset, string query)
	{
		// Search in asset name
		string assetName = asset.GetBestName();
		if (assetName.Contains(query, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		// Search in class name
		if (asset.ClassName.Contains(query, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		return false;
	}

	private struct SearchResult
	{
		public required IUnityObjectBase Asset { get; init; }
		public required AssetCollection Collection { get; init; }
		public required CollectionPath CollectionPath { get; init; }
		public required AssetPath Path { get; init; }
	}
}
