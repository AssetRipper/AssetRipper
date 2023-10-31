using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Settings;

[BindProperties]
public class EditModel : PageModel
{
	private readonly ILogger<EditModel> _logger;

	private static LibraryConfiguration Configuration => Program.Ripper.Settings;

	public bool EnableStaticMeshSeparation
	{
		get => Configuration.EnableStaticMeshSeparation;
		set => Configuration.EnableStaticMeshSeparation = value;
	}

	public bool EnablePrefabOutlining
	{
		get => Configuration.EnablePrefabOutlining;
		set => Configuration.EnablePrefabOutlining = value;
	}

	public bool IgnoreStreamingAssets
	{
		get => Configuration.IgnoreStreamingAssets;
		set => Configuration.IgnoreStreamingAssets = value;
	}

	public bool IgnoreEngineAssets
	{
		get => Configuration.IgnoreEngineAssets;
		set => Configuration.IgnoreEngineAssets = value;
	}

	public string? DefaultVersion
	{
		get => Configuration.DefaultVersion.ToString();
		set => Configuration.DefaultVersion = TryParseUnityVersion(value);
	}

	public string? AudioExportFormat
	{
		get => Configuration.AudioExportFormat.ToString();
		set => Configuration.AudioExportFormat = TryParseEnum<AudioExportFormat>(value);
	}

	public string? BundledAssetsExportMode
	{
		get => Configuration.BundledAssetsExportMode.ToString();
		set => Configuration.BundledAssetsExportMode = TryParseEnum<BundledAssetsExportMode>(value);
	}

	public string? ImageExportFormat
	{
		get => Configuration.ImageExportFormat.ToString();
		set => Configuration.ImageExportFormat = TryParseEnum<ImageExportFormat>(value);
	}

	public string? MeshExportFormat
	{
		get => Configuration.MeshExportFormat.ToString();
		set => Configuration.MeshExportFormat = TryParseEnum<MeshExportFormat>(value);
	}

	public string? SpriteExportMode
	{
		get => Configuration.SpriteExportMode.ToString();
		set => Configuration.SpriteExportMode = TryParseEnum<SpriteExportMode>(value);
	}

	public string? TerrainExportMode
	{
		get => Configuration.TerrainExportMode.ToString();
		set => Configuration.TerrainExportMode = TryParseEnum<TerrainExportMode>(value);
	}

	public string? TextExportMode
	{
		get => Configuration.TextExportMode.ToString();
		set => Configuration.TextExportMode = TryParseEnum<TextExportMode>(value);
	}

	public string? ShaderExportMode
	{
		get => Configuration.ShaderExportMode.ToString();
		set => Configuration.ShaderExportMode = TryParseEnum<ShaderExportMode>(value);
	}

	public string? ScriptExportMode
	{
		get => Configuration.ScriptExportMode.ToString();
		set => Configuration.ScriptExportMode = TryParseEnum<ScriptExportMode>(value);
	}

	public string? ScriptContentLevel
	{
		get => Configuration.ScriptContentLevel.ToString();
		set => Configuration.ScriptContentLevel = TryParseEnum<ScriptContentLevel>(value);
	}

	public string? ScriptLanguageVersion
	{
		get => Configuration.ScriptLanguageVersion.ToString();
		set => Configuration.ScriptLanguageVersion = TryParseEnum<ScriptLanguageVersion>(value);
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

	private static T TryParseEnum<T>(string? s) where T : struct, Enum
	{
		if (Enum.TryParse(s, out T result))
		{
			return result;
		}
		return default;
	}
}
