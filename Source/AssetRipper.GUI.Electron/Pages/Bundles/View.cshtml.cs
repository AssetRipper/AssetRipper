using AssetRipper.Assets.Bundles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Bundles
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;

		public Bundle Bundle { get; private set; } = default!;

		public BundlePath BundlePath { get; private set; }

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

			BundlePath = BundlePath.FromJson(path);
			if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetBundle(BundlePath, out Bundle? bundle))
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
