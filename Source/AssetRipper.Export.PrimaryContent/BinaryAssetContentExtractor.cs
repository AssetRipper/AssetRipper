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
			ITextAsset textAsset => new TextAssetExportCollection(textAsset),
			IFont font => new FontExportCollection(font),
			IMovieTexture movieTexture when movieTexture.Has_MovieData() => new MovieTextureExportCollection(movieTexture),
			IVideoClip videoClip => new VideoClipExportCollection(videoClip),
			_ => null,
		};
		return exportCollection is not null;
	}

	private abstract class BinaryAssetExportCollection<T> : SingleExportCollection<T> where T : IUnityObjectBase
	{
		public BinaryAssetExportCollection(T asset) : base(Instance, asset)
		{
		}

		protected override bool ExportInner(string filePath, string dirPath)
		{
			byte[] data = Data;
			if (data.Length > 0)
			{
				File.WriteAllBytes(filePath, data);
				return true;
			}
			else
			{
				return false;
			}
		}

		// This should be ReadOnlySpan<byte>
		// Switch after we bump to .NET 9
		protected abstract byte[] Data { get; }
	}

	private sealed class TextAssetExportCollection : BinaryAssetExportCollection<ITextAsset>
	{
		public TextAssetExportCollection(ITextAsset asset) : base(asset)
		{
		}

		protected override byte[] Data => Asset.Script_C49.Data.ToArray();

		protected override string ExportExtension => "bytes";
	}

	private sealed class FontExportCollection : BinaryAssetExportCollection<IFont>
	{
		public FontExportCollection(IFont asset) : base(asset)
		{
		}

		protected override byte[] Data => Asset.FontData;

		protected override string ExportExtension => Asset.GetFontExtension();
	}

	private sealed class MovieTextureExportCollection : BinaryAssetExportCollection<IMovieTexture>
	{
		public MovieTextureExportCollection(IMovieTexture asset) : base(asset)
		{
		}

		protected override byte[] Data => Asset.MovieData ?? [];

		protected override string ExportExtension => "ogv";
	}

	private sealed class VideoClipExportCollection : BinaryAssetExportCollection<IVideoClip>
	{
		public VideoClipExportCollection(IVideoClip asset) : base(asset)
		{
		}

		protected override byte[] Data => Asset.GetContent();

		protected override string ExportExtension => Asset.GetExtensionFromPath();
	}
}
