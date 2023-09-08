using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Export.UnityProjects.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.TextureDecoder.Rgb.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;
		public IUnityObjectBase Asset { get; private set; } = default!;

		public string TextureDataPath
		{
			get
			{
				if (Asset is ITexture2D texture
					&& TextureConverter.TryConvertToBitmap(texture, out DirectBitmap<ColorBGRA32, byte> bitmap))
				{
					MemoryStream stream = new();
					bitmap.SaveAsPng(stream);
					return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		public string Text
		{
			get
			{
				return Asset switch
				{
					ITextAsset textAsset => textAsset.Script_C49,
					_ => "",
				};
			}
		}

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
			else if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetAsset(AssetPath.FromJson(path), out IUnityObjectBase? asset))
			{
				Asset = asset;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}
	}
}
