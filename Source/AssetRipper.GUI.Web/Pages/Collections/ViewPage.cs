using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Collections;

public sealed class ViewPage : DefaultPage
{
	public required AssetCollection Collection { get; init; }
	public required CollectionPath Path { get; init; }

	public override string GetTitle() => Collection.Name;

	private readonly List<int> assetsPerPage = new() {500, 1000, 2000, 3000, 5000};// Max 5k per page?

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		new H2(writer).Close(Localization.Bundle);
		PathLinking.WriteLink(writer, Path.BundlePath, Collection.Bundle.Name);

		if (Collection.IsScene)
		{
			new H2(writer).Close(Localization.Scene);
			PathLinking.WriteLink(writer, (ScenePath)Path, Collection.Scene.Name);
		}

		if (Collection.Count > 0)
		{
			new H2(writer).Close(Localization.Assets);
			
			var availableClasses = Collection.Select(a => a.ClassName).Distinct().Order().ToList();
			if (availableClasses.Count > 1)
			{
				//i: set the filter items and UI? (since this doesn't change for a collection
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

			CreatePageOption(writer, Collection.Count);
				
			using (new Table(writer).WithClass("table").End())
			{
				using (new Thead(writer).End())
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.PathId);
						new Th(writer).Close(Localization.Class);
						new Th(writer).Close(Localization.Name);
					}
				}
				using (new Tbody(writer).WithId("assetsTable").End())
				{
					foreach (IUnityObjectBase asset in Collection)
					{
						using (new Tr(writer).WithCustomAttribute("data-class", asset.ClassName).End())
						{
							new Td(writer).Close(asset.PathID.ToString());
							new Td(writer).Close(asset.ClassName);
							using (new Td(writer).End())
							{
								PathLinking.WriteLink(writer, Path.GetAsset(asset.PathID), asset.GetBestName());
							}
						}
					}
				}
			}
		}

		if (Collection.Dependencies.Count > 1)
		{
			new H2(writer).Close(Localization.AssetTabDependencies);
			using (new Table(writer).WithClass("table").End())
			{
				using (new Thead(writer).End())
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.FileId);
						new Th(writer).Close(Localization.Name);
					}
				}
				using (new Tbody(writer).End())
				{
					for (int i = 1; i < Collection.Dependencies.Count; i++)
					{
						AssetCollection? dependency = Collection.Dependencies[i];
						if (dependency is null)
						{
							continue;
						}

						using (new Tr(writer).End())
						{
							new Td(writer).Close(i.ToString());
							using (new Td(writer).End())
							{
								PathLinking.WriteLink(writer, dependency);
							}
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
			//Todo: add page selection filter
			//Todo: add page reseting code after filtering
			writer.Write("""
				document.addEventListener('DOMContentLoaded', function() {
					const classFilter = document.getElementById('classFilter');
					if (classFilter) {
						classFilter.addEventListener('change', function() {
							const selectedClass = this.value;
							const rows = document.querySelectorAll('#assetsTable tr');

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

					const pageFilter = document.getElementById('assetPerPage');
					if (pageFilter) {
						pageFilter.addEventListener('change', function() {
							const selectedCount = parseInt(this.value);
							const pageNo = parseInt(document.getElementById('pageNo').innerHTML);
							const rows = document.querySelectorAll('#assetsTable tr');
							
							const start = selectedCount * (pageNo - 1);
							for(var i = 0; i < rows.length; ++i){
								if(i >= start && i < selectedCount * pageNo){
									rows[i].style.display = '';
								}else {
									rows[i].style.display = 'none';
								}
							}
						});
					}

					const prevButton = document.getElementById('prevBtn');
					const nextButton = document.getElementById('nextBtn');
					if(prevButton){
						prevButton.addEventListener('click', function() {
							var pageNo = parseInt(document.getElementById('pageNo').innerHTML);
							--pageNo;
							document.getElementById('pageNo').innerHTML = "" + pageNo;

							if(pageNo == 1){
								this.disabled = true;
							}

							if(nextButton){
								nextButton.disabled = false;
							}

							if (pageFilter) {
								var event = new Event('change');
								pageFilter.dispatchEvent(event);
							}
						});
					}

					if(nextButton){
						nextButton.addEventListener('click', function() {
							var pageNo = parseInt(document.getElementById('pageNo').innerHTML);
							++pageNo;
							document.getElementById('pageNo').innerHTML = "" + pageNo;
				
							if(prevButton){
								prevButton.disabled = false;
							}

							if (pageFilter) {
								const rows = document.querySelectorAll('#assetsTable tr');
								const selectedCount = parseInt(pageFilter.value);
								if(rows.length <= selectedCount * pageNo){
									this.disabled = true;
								}

								var event = new Event('change');
								pageFilter.dispatchEvent(event);
							}
						});
					}
				});
				""");
		}
	}

	private void CreatePageOption(TextWriter writer, int collectionCount)
	{
		//Todo: add page UI, add localization:
		/// page
		/// prev
		/// next
		/// Assets Per Page
		/// Todo: Un Naive the method---add in 
		/// Filters
		/// Page updates on Asset Per page changes
		new Label(writer).WithFor("pageCollection").WithClass("me-2").Close("Page");
		new Button(writer)
				.WithId("prevBtn")
				.WithClass("ms-2")
				.WithType("button")
				.WithCustomAttribute("v-else")
				.WithCustomAttribute("disabled")
				.Close("prev");

		new Label(writer).WithId("pageNo").WithClass("ms-2").Close("1");// dropdown also

		Button b = new Button(writer)
				.WithId("nextBtn")
				.WithType("button")
				.WithClass("ms-2")
				.WithCustomAttribute("v-else");

		if (Collection.Count <= assetsPerPage[0])
		{
			b.WithCustomAttribute("disabled");
		}

		b.Close("next");

		new Label(writer).WithClass("ms-2").Close("Assets Per Page");

		using (new Select(writer)
			.WithId("assetPerPage")
			.WithClass("ms-2")
			.End())
		{
			foreach (int count in assetsPerPage)
			{
				new Option(writer)
					.WithValue(string.Empty)
					.Close(Localization.All);

				if (collectionCount >= count)
				{

					new Option(writer)
					.WithValue("" + count)
					.Close("" + count);
				}
			}
		}
	}
}
