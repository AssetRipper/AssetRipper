using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal static class AssetAPI
{
	public const string Extension = "Extension";
	public const string Path = "Path";

	#region Image
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
	public static Task GetAudioData(HttpContext context)
	{
		//Accept Path and Extension in the query.
		throw new NotImplementedException();
	}
	public static bool HasAudioData(IUnityObjectBase asset)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Model
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
	public static Task GetFontData(HttpContext context)
	{
		//Only accept Path in the query.
		throw new NotImplementedException();
	}
	public static bool HasFontData(IUnityObjectBase asset)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Json
	public static Task GetJson(HttpContext context)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Yaml
	public static Task GetYaml(HttpContext context)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Text
	public static Task GetText(HttpContext context)
	{
		//Only accept Path in the query. It sensibly determines the file extension.
		throw new NotImplementedException();
	}
	public static bool HasText(IUnityObjectBase asset)
	{
		throw new NotImplementedException();
	}
	#endregion

	#region Binary Data
	public static Task GetBinaryData(HttpContext context)
	{
		//Only for RawDataObject. This should not call any of the IUnityAssetBase Write methods.
		throw new NotImplementedException();
	}
	public static bool HasBinaryData(IUnityObjectBase asset)
	{
		return asset is RawDataObject { RawData.Length: > 0 };
	}
	#endregion

	private static bool TryGetAssetFromQuery(HttpContext context, [NotNullWhen(true)] out IUnityObjectBase? asset, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			asset = null;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		AssetPath path;
		try
		{
			path = AssetPath.FromJson(json);
		}
		catch (Exception ex)
		{
			asset = null;
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
