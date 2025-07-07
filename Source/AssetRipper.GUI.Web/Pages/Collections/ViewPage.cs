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
				using (new Tbody(writer).End())
				{
					foreach (IUnityObjectBase asset in Collection)
					{
						using (new Tr(writer).End())
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
}
