using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public abstract class FileSchemeList : FileScheme
	{
		public FileSchemeList(SmartStream stream, long offset, long size, string filePath, string fileName):
			base(stream, offset, size, filePath, fileName)
		{
		}

		public sealed override bool ContainsFile(string fileName)
		{
			foreach (FileScheme fileScheme in Schemes)
			{
				if (fileScheme.Name == fileName)
				{
					return true;
				}
				if (fileScheme.ContainsFile(fileName))
				{
					return true;
				}
			}
			return false;
		}

		protected void AddScheme(FileScheme scheme)
		{
			m_schemes.Add(scheme);
		}

		public override sealed IEnumerable<FileIdentifier> Dependencies
		{
			get
			{
				foreach (FileScheme scheme in m_schemes)
				{
					foreach (FileIdentifier dependency in scheme.Dependencies)
					{
						yield return dependency;
					}
				}
			}
		}

		public IReadOnlyList<FileScheme> Schemes => m_schemes;

		private readonly List<FileScheme> m_schemes = new List<FileScheme>();
	}
}
