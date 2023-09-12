using AssetRipper.Assets.Bundles;
using AssetRipper.IO.Files.ResourceFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Resources;

public class ViewModel : PageModel
{
	private readonly ILogger<ViewModel> _logger;

	public Bundle? Bundle => Program.Ripper.GameStructure.FileCollection.TryGetBundle(ResourcePath.BundlePath);

	public string Name => Resource.NameFixed;

	public ResourceFile Resource { get; private set; } = default!;

	public ResourcePath ResourcePath { get; private set; }

	public byte[] Data => Resource.ToByteArray();

	public ViewModel(ILogger<ViewModel> logger)
	{
		_logger = logger;
	}

	public IActionResult OnGet(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			_logger.LogError("Path is null");
			return Redirect("/");
		}

		ResourcePath = ResourcePath.FromJson(path);
		if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetResource(ResourcePath, out ResourceFile? resource))
		{
			Resource = resource;
			return Page();
		}
		else
		{
			return NotFound();
		}
	}
}
