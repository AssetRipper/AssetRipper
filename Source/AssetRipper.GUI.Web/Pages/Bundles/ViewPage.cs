using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

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
				for (int i = 0; i < Bundle.Bundles.Count; i++)
				{
					using (new Li(writer).End())
					{
						PathLinking.WriteLink(writer, Path.GetChild(i), Bundle.Bundles[i].Name);
					}
				}
			}
		}

		if (Bundle.Collections.Count > 0)
		{
			new H2(writer).Close(Localization.Collections);
			using (new Ul(writer).End())
			{
				for (int i = 0; i < Bundle.Collections.Count; i++)
				{
					AssetCollection collection = Bundle.Collections[i];
					if (collection.Count > 0 || collection is SerializedAssetCollection)
					{
						using (new Li(writer).End())
						{
							PathLinking.WriteLink(writer, Path.GetCollection(i), collection.Name);
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
				for (int i = 0; i < Bundle.Resources.Count; i++)
				{
					using (new Li(writer).End())
					{
						PathLinking.WriteLink(writer, Path.GetResource(i), Bundle.Resources[i].Name);
					}
				}
			}
		}
	}

	public static Task HandlePostRequest(HttpContext context)
	{
		string? json = context.Request.Form[PathLinking.FormKey];
		if (string.IsNullOrEmpty(json))
		{
			return context.Response.NotFound();
		}

		BundlePath path;
		try
		{
			path = BundlePath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetBundle(path, out Bundle? bundle))
		{
			return context.Response.NotFound($"Bundle could not be resolved: {path}");
		}
		else
		{
			return new ViewPage() { Bundle = bundle, Path = path }.WriteToResponse(context.Response);
		}
	}
}
