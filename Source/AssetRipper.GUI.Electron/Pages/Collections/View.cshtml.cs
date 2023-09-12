using AssetRipper.Assets.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Collections
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;

		public AssetCollection Collection { get; private set; } = default!;

		public CollectionPath CollectionPath { get; private set; }

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

			CollectionPath = CollectionPath.FromJson(path);
			if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetCollection(CollectionPath, out AssetCollection? collection))
			{
				Collection = collection;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}
	}
}
