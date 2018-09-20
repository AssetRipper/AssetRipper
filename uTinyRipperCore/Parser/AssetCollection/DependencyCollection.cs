using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	internal sealed class DependencyCollection
	{
		public DependencyCollection(FileCollection fileCollection, IEnumerable<FileEntry> entries, Action<string> dependencyCallback)
		{
			if (fileCollection == null)
			{
				throw new ArgumentNullException(nameof(fileCollection));
			}
			if (entries == null)
			{
				throw new ArgumentNullException(nameof(entries));
			}
			if (dependencyCallback == null)
			{
				throw new ArgumentNullException(nameof(dependencyCallback));
			}

			m_fileCollection = fileCollection;
			Dictionary<string, FileEntry> fileEntries = new Dictionary<string, FileEntry>();
			foreach(FileEntry entry in entries)
			{
				string name = FilenameUtils.FixFileIdentifier(entry.Name);
				fileEntries.Add(name, entry);
			}
			m_entries = fileEntries;
			m_dependencyCallback = dependencyCallback;
		}

		public void ReadFiles()
		{
			foreach (FileEntry entry in m_entries.Values)
			{
				if (entry.EntryType == FileEntryType.Resource)
				{
					entry.ReadResourcesFile(m_fileCollection);
				}
			}

			foreach (FileEntry entry in m_entries.Values)
			{
				if (entry.EntryType == FileEntryType.Serialized)
				{
					string name = FilenameUtils.FixFileIdentifier(entry.Name);
					if (m_loadedFiles.Add(name))
					{
						entry.ReadSerializedFile(m_fileCollection, OnRequestDependency);
					}
				}
			}

			foreach (FileEntry entry in m_entries.Values)
			{
				if (entry.EntryType == FileEntryType.Bundle)
				{
					entry.ReadBundleFile(m_fileCollection);
				}
				else if(entry.EntryType == FileEntryType.Web)
				{
					entry.ReadWebFile(m_fileCollection);
				}
			}
		}

		private void OnRequestDependency(string dependency)
		{
			if (m_loadedFiles.Contains(dependency))
			{
				return;
			}
			
			if(m_entries.TryGetValue(dependency, out FileEntry entry))
			{
				if (entry.EntryType == FileEntryType.Serialized)
				{
					entry.ReadSerializedFile(m_fileCollection, OnRequestDependency);
					m_loadedFiles.Add(dependency);
					return;
				}
			}

			m_dependencyCallback.Invoke(dependency);
		}

		private readonly HashSet<string> m_loadedFiles = new HashSet<string>();

		private readonly FileCollection m_fileCollection;
		private readonly IReadOnlyDictionary<string, FileEntry> m_entries;
		private readonly Action<string> m_dependencyCallback;
	}
}
