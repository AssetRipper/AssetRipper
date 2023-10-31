using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Decompilation.CSharp;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;

		public IUnityObjectBase Asset { get; private set; } = default!;

		public string AudioSource
		{
			get
			{
				if (Asset is IAudioClip clip && AudioClipDecoder.TryGetDecodedAudioClipData(clip, out byte[]? decodedAudioData, out string? extension))
				{
					return $"data:audio/{extension};base64,{Convert.ToBase64String(decodedAudioData, Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		public DirectBitmap ImageBitmap
		{
			get
			{
				return Asset switch
				{
					ITexture2D texture => TextureToBitmap(texture),
					SpriteInformationObject spriteInformationObject => TextureToBitmap(spriteInformationObject.Texture),
					ISprite sprite => SpriteToBitmap(sprite),
					ITerrainData terrainData => TerrainHeatmapExporter.GetBitmap(terrainData),
					_ => default,
				};

				static DirectBitmap TextureToBitmap(ITexture2D texture)
				{
					return TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap) ? bitmap : default;
				}

				static DirectBitmap SpriteToBitmap(ISprite sprite)
				{
					return sprite.TryGetTexture() is { } spriteTexture ? TextureToBitmap(spriteTexture) : default;
				}
			}
		}

		public byte[] Font => Asset is IFont font ? font.FontData : Array.Empty<byte>();

		public string FontFileName
		{
			get
			{
				if (Asset is IFont font && Font is { Length: >= 4 })
				{
					return $"{font.GetBestName()}.{(Font[0], Font[1], Font[2], Font[3]) switch
					{
						(0x4F, 0x54, 0x54, 0x4F) => "otf",
						(0x00, 0x01, 0x00, 0x00) => "ttf",
						(0x74, 0x74, 0x63, 0x66) => "ttc",
						_ => "",
					}}";
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
					IShader shader => DumpShaderDataAsText(shader),
					IMonoScript monoScript => DecompileMonoScript(monoScript),
					ITextAsset textAsset => textAsset.Script_C49,
					_ => "",
				};
			}
		}

		public string TextFileName
		{
			get
			{
				return Asset switch
				{
					IShader shader => $"{shader.GetBestName()}.shader",
					IMonoScript monoScript => $"{monoScript.ClassName_R}.cs",
					ITextAsset textAsset => $"{textAsset.GetBestName()}.txt",
					_ => $"{Asset.GetBestName()}.txt",
				};
			}
		}

		public string YamlFileName => $"{Asset.GetBestName()}.asset";

		public string DataFileName => $"{Asset.GetBestName()}.dat";

		public byte[] Data
		{
			get
			{
				if (Asset is RawDataObject rawData)
				{
					return rawData.RawData;
				}
				else
				{
					MemoryStream stream = new();
					AssetWriter writer = new(stream, Asset.Collection);
					try
					{
						Asset.Write(writer);
					}
					catch (NotSupportedException)
					{
						//This should never happen, but it could if an asset type is not fully implemented.
						return Array.Empty<byte>();
					}
					return stream.ToArray();
				}
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

		private static string DumpShaderDataAsText(IShader shader)
		{
			MemoryStream stream = new();
			DummyShaderTextExporter.ExportShader(shader, stream);

			return Encoding.UTF8.GetString(GetStreamData(stream));

			static ReadOnlySpan<byte> GetStreamData(MemoryStream stream)
			{
				if (stream.TryGetBuffer(out ArraySegment<byte> buffer))
				{
					return buffer;
				}
				else
				{
					return stream.ToArray();
				}
			}
		}

		private static string DecompileMonoScript(IMonoScript monoScript)
		{
			IAssemblyManager assemblyManager = Program.Ripper.GameStructure.AssemblyManager;
			if (!monoScript.IsScriptPresents(assemblyManager))
			{
				return EmptyScript.GetContent(monoScript);
			}
			else
			{
				TypeDefinition type = monoScript.GetTypeDefinition(assemblyManager);
				return CSharpDecompiler.Decompile(type);
			}
		}
	}
}
