using AssetRipper.Assets.Bundles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Bundles
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;

		public Bundle Bundle { get; private set; } = default!;

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
			else if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetBundle(BundlePath.FromJson(path), out Bundle? bundle))
			{
				Bundle = bundle;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}
	}
}
