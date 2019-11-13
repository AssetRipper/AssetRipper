using System;
using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	internal sealed class GameStructureProcessor : IDisposable
	{
		~GameStructureProcessor()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		public void AddScheme(string filePath, string fileName)
		{
			FileScheme scheme = GameCollection.LoadScheme(filePath, fileName);
			OnSchemeLoaded(scheme);
			m_schemes.Add(scheme);
		}

		public void AddDependencySchemes(Func<string, string> dependencyCallback)
		{
			for (int i = 0; i < m_schemes.Count; i++)
			{
				FileScheme scheme = m_schemes[i];
				foreach (FileIdentifier dependency in scheme.Dependencies)
				{
					if (m_knownFiles.Contains(dependency.FilePath))
					{
						continue;
					}

					string systemFilePath = dependencyCallback.Invoke(dependency.FilePath);
					if (systemFilePath == null)
					{
						m_knownFiles.Add(dependency.FilePath);
						Logger.Log(LogType.Warning, LogCategory.Import, $"Dependency '{dependency}' hasn't been found");
						continue;
					}

					AddScheme(systemFilePath, dependency.FilePath);
				}
			}
		}

		public void ProcessSchemes(GameCollection fileCollection)
		{
			GameProcessorContext context = new GameProcessorContext(fileCollection);
			foreach (FileScheme scheme in m_schemes)
			{
				fileCollection.AddFile(context, scheme);
			}
			context.ReadSerializedFiles();
		}

		private void Dispose(bool disposing)
		{
			foreach (FileScheme scheme in m_schemes)
			{
				scheme.Dispose();
			}
		}

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

		private readonly List<FileScheme> m_schemes = new List<FileScheme>();
		private readonly HashSet<string> m_knownFiles = new HashSet<string>();
	}
}
