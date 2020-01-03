using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public abstract class FileSchemeList : FileScheme
	{
		protected FileSchemeList(string filePath, string fileName) :
			base(filePath, fileName)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			foreach (FileScheme scheme in Schemes)
			{
				scheme.Dispose();
			}
		}

		protected void AddScheme(FileScheme scheme)
		{
			m_schemes.Add(scheme);
		}

		public sealed override IEnumerable<FileIdentifier> Dependencies
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
