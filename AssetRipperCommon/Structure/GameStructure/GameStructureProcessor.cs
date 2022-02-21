using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.ArchiveFiles;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Structure.GameStructure
{
	internal sealed class GameStructureProcessor : IDisposable
	{
		private readonly List<FileScheme> m_schemes = new List<FileScheme>();
		private readonly HashSet<string> m_knownFiles = new HashSet<string>();

		public bool IsValid => m_schemes.Any(t => t.SchemeType != FileEntryType.Resource);

		/// <summary>Adds a file and its type to the list of files</summary>
		public void AddScheme(string filePath, string fileName)
		{
			FileScheme scheme = SchemeReader.LoadScheme(filePath, fileName);
			OnSchemeLoaded(scheme);
			m_schemes.Add(scheme);
		}

		/// <summary>Recursively adds Serialized files to the m_knownFiles hashset</summary>
		private void OnSchemeLoaded(FileScheme scheme)
		{
			if (scheme.SchemeType == FileEntryType.Serialized)
			{
				m_knownFiles.Add(scheme.Name);
			}

			if (scheme is FileSchemeList list)
			{
				foreach (FileScheme nestedScheme in list.Schemes)
				{
					OnSchemeLoaded(nestedScheme);
				}
			}
		}

		/// <summary>Attempts to add any missing dependencies to the file list</summary>
		/// <param name="dependencyCallback">A method that takes a dependency name and tries to output a path</param>
		public void AddDependencySchemes(Func<string, string> dependencyCallback)
		{
			for (int i = 0; i < m_schemes.Count; i++)
			{
				FileScheme scheme = m_schemes[i];
				foreach (FileIdentifier dependency in scheme.Dependencies)
				{
					if (m_knownFiles.Contains(dependency.PathName))
					{
						continue;
					}

					string systemFilePath = dependencyCallback.Invoke(dependency.PathName);
					if (systemFilePath == null)
					{
						m_knownFiles.Add(dependency.PathName);
						Logger.Log(LogType.Warning, LogCategory.Import, $"Dependency '{dependency}' wasn't found");
						continue;
					}

					AddScheme(systemFilePath, dependency.PathName);
				}
			}
		}

		public void ProcessSchemes(GameCollection fileCollection)
		{
			//Initializes it with a reference to the file collection
			GameProcessorContext context = new GameProcessorContext(fileCollection);

			Logger.SendStatusChange("loading_step_pre_processing");

			foreach (FileScheme scheme in m_schemes)
			{
				//Not just a simple add
				//Grouped by scheme type
				//And new objects are added to lists
				fileCollection.AddFile(context, scheme);
			}
			context.ReadSerializedFiles();
		}

		public LayoutInfo GetLayoutInfo()
		{
			SerializedFileScheme prime = GetPrimaryFile();
			if (prime != null)
			{
				return GetLayoutInfo(prime);
			}

			SerializedFileScheme serialized = GetEngineFile();
			return GetLayoutInfo(serialized);
		}

		private static IEnumerable<FileScheme> EnumerateSchemes(IReadOnlyList<FileScheme> schemes)
		{
			foreach (FileScheme scheme in schemes)
			{
				yield return scheme;
				if (scheme is FileSchemeList fileList)
				{
					foreach (FileScheme nestedScheme in EnumerateSchemes(fileList.Schemes))
					{
						yield return nestedScheme;
					}
				}
				else if (scheme.SchemeType == FileEntryType.Archive)
				{
					ArchiveFileScheme archive = (ArchiveFileScheme)scheme;
					yield return archive.WebScheme;
					foreach (FileScheme nestedScheme in EnumerateSchemes(archive.WebScheme.Schemes))
					{
						yield return nestedScheme;
					}
				}
			}
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

		private LayoutInfo GetLayoutInfo(SerializedFileScheme serialized)
		{
			if (SerializedFileMetadata.HasPlatform(serialized.Header.Version))
			{
				SerializedFileMetadata metadata = serialized.Metadata;
				return new LayoutInfo(metadata.UnityVersion, metadata.TargetPlatform, serialized.Flags);
			}
			else
			{
				const Platform DefaultPlatform = Platform.StandaloneWinPlayer;
				const TransferInstructionFlags DefaultFlags = TransferInstructionFlags.SerializeGameRelease;
				BundleFileScheme bundle = GetBundleFile();
				if (bundle == null)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "Unable to determine layout for provided files. Tring default one");
					UnityVersion version = GetDefaultGenerationVersions(serialized.Header.Version);
					return new LayoutInfo(version, DefaultPlatform, DefaultFlags);

				}
				else
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "Unable to precisly determine layout for provided files. Tring default one");
					return new LayoutInfo(bundle.Header.UnityWebMinimumRevision, DefaultPlatform, DefaultFlags);
				}
			}
		}

		private SerializedFileScheme GetPrimaryFile()
		{
			foreach (FileScheme scheme in EnumerateSchemes(m_schemes))
			{
				if (scheme.SchemeType == FileEntryType.Serialized)
				{
					if (PlatformGameStructure.IsPrimaryEngineFile(scheme.NameOrigin))
					{
						return (SerializedFileScheme)scheme;
					}
				}
			}
			return null;
		}

		private SerializedFileScheme GetEngineFile()
		{
			foreach (FileScheme scheme in EnumerateSchemes(m_schemes))
			{
				if (scheme.SchemeType == FileEntryType.Serialized)
				{
					return (SerializedFileScheme)scheme;
				}
			}
			return null;
		}

		private BundleFileScheme GetBundleFile()
		{
			foreach (FileScheme scheme in EnumerateSchemes(m_schemes))
			{
				if (scheme.SchemeType == FileEntryType.Bundle)
				{
					return (BundleFileScheme)scheme;
				}
			}
			return null;
		}

		~GameStructureProcessor()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			foreach (FileScheme scheme in m_schemes)
			{
				scheme.Dispose();
			}
		}
	}
}
