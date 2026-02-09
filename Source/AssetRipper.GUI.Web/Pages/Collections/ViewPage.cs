using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Collections;

public sealed class ViewPage : DefaultPage
{
	public required AssetCollection Collection { get; init; }
	public required CollectionPath Path { get; init; }

	public override string GetTitle() => Collection.Name;

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
				});
				""");
		}
	}
}
