using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedTagMap : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_tags = new Dictionary<string, string>();

			m_tags.Read(reader);
		}

		public IReadOnlyDictionary<string, string> Tags => m_tags;

		private Dictionary<string, string> m_tags;
	}
}
