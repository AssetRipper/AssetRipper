using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;
		public IUnityObjectBase Asset { get; private set; } = default!;

		public string ImageSource
		{
			get
			{
				DirectBitmap bitmap = Bitmap;
				if (bitmap != default)
				{
					MemoryStream stream = new();
					bitmap.SaveAsPng(stream);
					return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		private DirectBitmap Bitmap
		{
			get
			{
				switch (Asset)
				{
					case ITexture2D texture:
						{
							if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
							{
								return bitmap;
							}
						}
						goto default;
					case ITerrainData terrainData:
						return TerrainHeatmapExporter.GetBitmap(terrainData);
					default:
						return default;
				}
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
