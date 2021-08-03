using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.Schemes
{
	public abstract class FileSchemeList : FileScheme
	{
		protected FileSchemeList(string filePath, string fileName) : base(filePath, fileName) { }

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
