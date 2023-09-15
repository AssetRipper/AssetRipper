using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Settings;

public class EditModel : PageModel
{
	private readonly ILogger<EditModel> _logger;

	private static LibraryConfiguration Configuration => Program.Ripper.Settings;

	[BindProperty]
	public string? DefaultVersion
	{
		get => Configuration.DefaultVersion.ToString();
		set => Configuration.DefaultVersion = TryParseUnityVersion(value);
	}

	public EditModel(ILogger<EditModel> logger)
	{
		_logger = logger;
	}

	public void OnPostUpdateSettings()
	{
	}

	private static UnityVersion TryParseUnityVersion(string? version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return default;
		}
		try
		{
			return UnityVersion.Parse(version);
		}
		catch
		{
			return default;
		}
	}
}
