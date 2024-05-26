using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Decompilation.CSharp;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.PrimaryContent;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.GUI.Web.Paths;
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
using AssetRipper.Web.Extensions;
using AssetRipper.Yaml;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Runtime.InteropServices;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal static class AssetAPI
{
	public static class Urls
	{
		public const string Base = "/Assets";
		public const string View = Base + "/View";
		public const string Image = Base + "/Image";
		public const string Audio = Base + "/Audio";
		public const string Model = Base + "/Model";
		public const string Font = Base + "/Font";
		public const string Json = Base + "/Json";
		public const string Yaml = Base + "/Yaml";
		public const string Text = Base + "/Text";
		public const string Binary = Base + "/Binary";
	}
	private const string Extension = "Extension";
	private const string Path = "Path";

	#region View
	public static string GetViewUrl(AssetPath path) => $"{Urls.View}?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out AssetPath path, out Task? failureTask))
		{
			return new ViewPage() { Asset = asset, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}
	#endregion

	#region Image
	public static string GetImageUrl(AssetPath path, string? extension = null)
	{
		return $"{Urls.Image}?{GetPathQuery(path)}{GetExtensionQuerySuffix(extension)}";
	}

	public static Task GetImageData(HttpContext context)
	{
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		if (TryGetImageExtensionFromQuery(context, out string? extension, out ImageExportFormat format))
		{
			MemoryStream stream = new();
			GetImageBitmap(asset).Save(stream, format);
			return Results.Bytes(stream.ToArray(), $"image/{extension}").ExecuteAsync(context);
		}
		else
		{
			return Results.Bytes(GetRawImageData(asset), "application/octet-stream").ExecuteAsync(context);
		}
	}

	public static bool HasImageData(IUnityObjectBase asset) => asset switch
	{
		ITexture2D texture => texture.CheckAssetIntegrity(),
		SpriteInformationObject spriteInformationObject => spriteInformationObject.Texture.CheckAssetIntegrity(),
		ISprite sprite => sprite.TryGetTexture()?.CheckAssetIntegrity() ?? false,
		ITerrainData terrainData => terrainData.Heightmap.Heights.Count > 0,
		_ => false,
	};

	private static DirectBitmap GetImageBitmap(IUnityObjectBase asset)
	{
		return asset switch
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

	private static byte[] GetRawImageData(IUnityObjectBase asset)
	{
		return asset switch
		{
			ITexture2D texture => texture.GetImageData(),
			SpriteInformationObject spriteInformationObject => spriteInformationObject.Texture.GetImageData(),
			ISprite sprite => sprite.TryGetTexture()?.GetImageData() ?? [],
			ITerrainData terrainData => MemoryMarshal.AsBytes(terrainData.Heightmap.Heights.GetSpan()).ToArray(),
			_ => [],
		};
	}

	private static bool TryGetImageExtensionFromQuery(HttpContext context, [NotNullWhen(true)] out string? extension, out ImageExportFormat format)
	{
		if (context.Request.Query.TryGetValue(Extension, out extension))
		{
			return ImageExportFormatExtensions.TryGetFromExtension(extension, out format);
		}
		else
		{
			format = default;
			return false;
		}
	}
	#endregion

	#region Audio
	public static string GetAudioUrl(AssetPath path, string? extension = null)
	{
		return $"{Urls.Audio}?{GetPathQuery(path)}";
	}

	public static Task GetAudioData(HttpContext context)
	{
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		if (asset is not IAudioClip clip)
		{
			return context.Response.NotFound("Asset was not an audio clip.");
		}
		else if (AudioClipDecoder.TryDecode(clip, out byte[]? decodedAudioData, out string? extension, out _))
		{
			return Results.Bytes(decodedAudioData, $"audio/{extension}").ExecuteAsync(context);
		}
		else
		{
			return context.Response.NotFound("Audio data could not be decoded.");
		}
	}

	public static bool HasAudioData(IUnityObjectBase asset)
	{
		return asset is IAudioClip;
	}
	#endregion

	#region Model
	public static string GetModelUrl(AssetPath path)
	{
		return $"{Urls.Model}?{GetPathQuery(path)}";
	}

	public static Task GetModelData(HttpContext context)
	{
		//Only accept Path in the query.
		throw new NotImplementedException();
	}
	public static bool HasModelData(IUnityObjectBase asset)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Font
	public static string GetFontUrl(AssetPath path)
	{
		return $"{Urls.Font}?{GetPathQuery(path)}";
	}

	public static Task GetFontData(HttpContext context)
	{
		//Only accept Path in the query.
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		if (asset is not IFont font)
		{
			return context.Response.NotFound("Asset was not a font.");
		}
		else if (TryGetFontData(font, out byte[] data, out string? extension, out string? mimeType))
		{
			return Results.Bytes(data, mimeType, $"{font.GetBestName()}.{extension}").ExecuteAsync(context);
		}
		else
		{
			return context.Response.NotFound("Font data could not be decoded.");
		}
	}

	public static bool HasFontData(IUnityObjectBase asset)
	{
		return asset is IFont font && TryGetFontData(font, out _, out _, out _);
	}

	private static bool TryGetFontData(IFont font, out byte[] data, [NotNullWhen(true)] out string? extension, [NotNullWhen(true)] out string? mimeType)
	{
		data = font.FontData;

		if (data is { Length: >= 4 })
		{
			(extension, mimeType) = (data[0], data[1], data[2], data[3]) switch
			{
				(0x4F, 0x54, 0x54, 0x4F) => ("otf", "font/otf"),
				(0x00, 0x01, 0x00, 0x00) => ("ttf", "font/ttf"),
				(0x74, 0x74, 0x63, 0x66) => ("ttc", "font/collection"),
				_ => ("dat", "application/octet-stream"),
			};

			return true;
		}
		else
		{
			extension = null;
			mimeType = null;

			return false;
		}
	}
	#endregion

	#region Json
	public static string GetJsonUrl(AssetPath path)
	{
		return $"{Urls.Json}?{GetPathQuery(path)}";
	}
	public static Task GetJson(HttpContext context)
	{
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		string text = new DefaultJsonWalker().SerializeStandard(asset);
		return Results.Text(text, "application/json").ExecuteAsync(context);
	}
	#endregion

	#region Yaml
	public static string GetYamlUrl(AssetPath path)
	{
		return $"{Urls.Yaml}?{GetPathQuery(path)}";
	}
	public static Task GetYaml(HttpContext context)
	{
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		string text;
		using (StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" })
		{
			YamlWriter writer = new();
			writer.WriteHead(stringWriter);
			YamlDocument document = new YamlWalker().ExportYamlDocument(asset, ExportIdHandler.GetMainExportID(asset));
			writer.WriteDocument(document);
			writer.WriteTail(stringWriter);
			text = stringWriter.ToString();
		}
		return Results.Text(text, "application/yaml").ExecuteAsync(context);
	}
	#endregion

	#region Text
	public static string GetTextUrl(AssetPath path)
	{
		return $"{Urls.Text}?{GetPathQuery(path)}";
	}

	public static Task GetText(HttpContext context)
	{
		//Only accept Path in the query. It sensibly determines the file extension.
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		return Results.Text(TryGetText(asset), "text/plain").ExecuteAsync(context);
	}

	public static bool HasText(IUnityObjectBase asset)
	{
		return asset is IShader or IMonoScript or ITextAsset { Script_C49.IsEmpty: false };
	}

	public static string TryGetText(IUnityObjectBase asset)
	{
		return asset switch
		{
			IShader shader => DumpShaderDataAsText(shader),
			IMonoScript monoScript => DecompileMonoScript(monoScript),
			ITextAsset textAsset => textAsset.Script_C49,
			_ => "",
		};
	}

	public static string GetTextFileName(IUnityObjectBase asset)
	{
		return asset switch
		{
			IShader => $"{asset.GetBestName()}.shader",
			IMonoScript monoScript => $"{monoScript.ClassName_R}.cs",
			ITextAsset textAsset => $"{asset.GetBestName()}.{GetTextAssetExtension(textAsset)}",
			_ => $"{asset.GetBestName()}.txt",
		};

		static string GetTextAssetExtension(ITextAsset textAsset)
		{
			return string.IsNullOrEmpty(textAsset.OriginalExtension) ? "txt" : textAsset.OriginalExtension;
		}
	}

	private static string DumpShaderDataAsText(IShader shader)
	{
		InvariantStringWriter writer = new();
		DummyShaderTextExporter.ExportShader(shader, writer);
		return writer.ToString();
	}

	private static string DecompileMonoScript(IMonoScript monoScript)
	{
		IAssemblyManager assemblyManager = GameFileLoader.AssemblyManager;
		if (!monoScript.IsScriptPresents(assemblyManager))
		{
			return EmptyScript.GetContent(monoScript);
		}
		else
		{
			try
			{
				TypeDefinition type = monoScript.GetTypeDefinition(assemblyManager);
				return CSharpDecompiler.Decompile(type);
			}
			catch (Exception ex)
			{
				return $"{Localization.AnErrorOccuredDuringDecompilation}\n\n{ex}";
			}
		}
	}
	#endregion

	#region Binary Data
	public static string GetBinaryUrl(AssetPath path)
	{
		return $"{Urls.Binary}?{GetPathQuery(path)}";
	}

	public static Task GetBinaryData(HttpContext context)
	{
		//Only for RawDataObject. This should not call any of the IUnityAssetBase Write methods.
		context.Response.DisableCaching();
		if (!TryGetAssetFromQuery(context, out IUnityObjectBase? asset, out Task? failureTask))
		{
			return failureTask;
		}

		byte[] data = (asset as RawDataObject)?.RawData ?? [];
		return Results.Bytes(data, "application/octet-stream").ExecuteAsync(context);
	}

	public static bool HasBinaryData(IUnityObjectBase asset)
	{
		return asset is RawDataObject { RawData.Length: > 0 };
	}
	#endregion

	private static string GetPathQuery(AssetPath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static string? GetExtensionQuerySuffix(string? extension) => string.IsNullOrEmpty(extension) ? null : $"&{Extension}={extension}";

	private static bool TryGetAssetFromQuery(HttpContext context, [NotNullWhen(true)] out IUnityObjectBase? asset, [NotNullWhen(false)] out Task? failureTask)
	{
		return TryGetAssetFromQuery(context, out asset, out _, out failureTask);
	}

	private static bool TryGetAssetFromQuery(HttpContext context, [NotNullWhen(true)] out IUnityObjectBase? asset, out AssetPath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			asset = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = AssetPath.FromJson(json);
		}
		catch (Exception ex)
		{
			asset = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			asset = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetAsset(path, out asset))
		{
			failureTask = context.Response.NotFound($"Asset could not be resolved: {path}");
			return false;
		}
		else
		{
			failureTask = null;
			return true;
		}
	}
}
