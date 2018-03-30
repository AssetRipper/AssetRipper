using System;
using System.Collections.Generic;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFileData : IDisposable
	{
		public BundleFileData(BundleMetadata metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException(nameof(metadata));
			}

			m_metadatas = new[] { metadata };
		}

		public BundleFileData(BundleMetadata[] metadatas)
		{
			if (metadatas == null || metadatas.Length == 0)
			{
				throw new ArgumentNullException(nameof(metadatas));
			}

			m_metadatas = metadatas;
		}

		public void Dispose()
		{
			if (m_isDisposable)
			{
				foreach (BundleMetadata chunk in m_metadatas)
				{
					chunk.Dispose();
				}
				m_isDisposable = false;
			}
		}

		public IReadOnlyList<BundleMetadata> Metadatas => m_metadatas;

		private readonly BundleMetadata[] m_metadatas;

		private bool m_isDisposable = true;
	}
}
