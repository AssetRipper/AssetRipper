using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using AssetRipper.IO.Files.SerializedFiles;
using System.Linq;


namespace AssetRipper.Core.Structure.GameStructure
{
	public static class GameBundleExtensions
	{
		public static LayoutInfo GetLayoutInfo(this GameBundle gameBundle)
		{
			AssetCollection? prime = gameBundle.GetPrimaryFile();
			if (prime != null)
			{
				return gameBundle.GetLayoutInfo(prime);
			}

			AssetCollection? serialized = gameBundle.GetEngineFile()
				?? throw new NullReferenceException($"{nameof(GameBundle)} has no {nameof(AssetCollection)}.");
			return gameBundle.GetLayoutInfo(serialized);
		}

		private static UnityVersion GetDefaultGenerationVersions(FormatVersion generation)
		{
			if (generation < FormatVersion.Unknown_5)
			{
				return new UnityVersion(1, 2, 2);
			}

			return generation switch
			{
				FormatVersion.Unknown_5 => new UnityVersion(1, 6),
				FormatVersion.Unknown_6 => new UnityVersion(2, 5),
				FormatVersion.Unknown_7 => new UnityVersion(3, 0, 0, UnityVersionType.Beta, 1),
				_ => throw new NotSupportedException(),
			};
		}

		private static LayoutInfo GetLayoutInfo(this GameBundle gameBundle, AssetCollection collection)
		{
			//if (SerializedFileMetadata.HasPlatform(serialized.Header.Version))
			{
				return new LayoutInfo(collection.Version, collection.Platform, collection.Flags);
			}
			/*else
			{
				const BuildTarget DefaultPlatform = BuildTarget.StandaloneWinPlayer;
				const TransferInstructionFlags DefaultFlags = TransferInstructionFlags.SerializeGameRelease;
				FileContainer? bundle = gameBundle.GetBundleFile();
				string? versionString = (bundle as FileStreamBundleFile)?.Header.UnityWebMinimumRevision
					?? (bundle as WebBundleFile)?.Header.UnityWebMinimumRevision
					?? (bundle as RawBundleFile)?.Header.UnityWebMinimumRevision;
				if (versionString is not null)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "Unable to precisly determine layout for provided files. Trying default one");
					return new LayoutInfo(UnityVersion.Parse(versionString), DefaultPlatform, DefaultFlags);
				}
				else
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "Unable to determine layout for provided files. Trying default one");
					UnityVersion version = GetDefaultGenerationVersions(serialized.Header.Version);
					return new LayoutInfo(version, DefaultPlatform, DefaultFlags);
				}
			}*/
		}

		private static AssetCollection? GetPrimaryFile(this GameBundle gameBundle)
		{
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				if (PlatformGameStructure.IsPrimaryEngineFile(collection.Name))
				{
					return collection;
				}
			}
			return null;
		}

		private static AssetCollection? GetEngineFile(this GameBundle gameBundle)
		{
			return gameBundle.FetchAssetCollections().FirstOrDefault();
		}

		private static Bundle? GetBundleFile(this GameBundle gameBundle)
		{
			return gameBundle.Bundles.Count > 0 ? gameBundle.Bundles[0] : null;
		}
	}
}
