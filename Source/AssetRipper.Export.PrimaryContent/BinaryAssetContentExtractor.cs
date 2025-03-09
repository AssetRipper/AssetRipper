using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.PrimaryContent;

public sealed class BinaryAssetContentExtractor : IContentExtractor
{
	public static BinaryAssetContentExtractor Instance { get; } = new();
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		exportCollection = asset switch
		{
			ITextAsset textAsset => TextAssetExportCollection.Create(textAsset),
			IFont font => FontExportCollection.Create(font),
			IMovieTexture movieTexture => MovieTextureExportCollection.Create(movieTexture),
			IVideoClip videoClip => VideoClipExportCollection.Create(videoClip),
			_ => null,
		};
		return exportCollection is not null;
	}

	private abstract class BinaryAssetExportCollection<T> : SingleExportCollection<T> where T : IUnityObjectBase
	{
		public BinaryAssetExportCollection(T asset) : base(Instance, asset)
		{
		}

		protected override bool ExportInner(string filePath, string dirPath, FileSystem fileSystem)
		{
			ReadOnlySpan<byte> data = Data;
			if (data.Length > 0)
			{
				fileSystem.File.WriteAllBytes(filePath, data);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected abstract ReadOnlySpan<byte> Data { get; }
	}

	private sealed class TextAssetExportCollection : BinaryAssetExportCollection<ITextAsset>
	{
		public TextAssetExportCollection(ITextAsset asset) : base(asset)
		{
		}

		protected override ReadOnlySpan<byte> Data => Asset.Script_C49.Data;

		protected override string ExportExtension => "bytes";

		public static TextAssetExportCollection? Create(ITextAsset asset)
		{
			if (asset.Script_C49.Data.Length > 0)
			{
				return new TextAssetExportCollection(asset);
			}
			return null;
		}
	}

	private sealed class FontExportCollection : BinaryAssetExportCollection<IFont>
	{
		public FontExportCollection(IFont asset) : base(asset)
		{
		}

		protected override ReadOnlySpan<byte> Data => Asset.FontData;

		protected override string ExportExtension => Asset.GetFontExtension();

		public static FontExportCollection? Create(IFont asset)
		{
			if (asset.FontData.Length > 0)
			{
				return new FontExportCollection(asset);
			}
			return null;
		}
	}

	private sealed class MovieTextureExportCollection : BinaryAssetExportCollection<IMovieTexture>
	{
		public MovieTextureExportCollection(IMovieTexture asset) : base(asset)
		{
		}

		protected override ReadOnlySpan<byte> Data => Asset.MovieData ?? [];

		protected override string ExportExtension => "ogv";

		public static MovieTextureExportCollection? Create(IMovieTexture asset)
		{
			if (asset.Has_MovieData() && asset.MovieData.Length > 0)
			{
				return new MovieTextureExportCollection(asset);
			}
			return null;
		}
	}

	private sealed class VideoClipExportCollection : BinaryAssetExportCollection<IVideoClip>
	{
		public VideoClipExportCollection(IVideoClip asset) : base(asset)
		{
		}

		protected override ReadOnlySpan<byte> Data => Asset.GetContent();

		protected override string ExportExtension => Asset.GetExtensionFromPath();

		public static VideoClipExportCollection? Create(IVideoClip asset)
		{
			if (asset.CheckIntegrity())
			{
				return new VideoClipExportCollection(asset);
			}
			return null;
		}
	}
}
