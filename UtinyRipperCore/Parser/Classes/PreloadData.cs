using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class PreloadData : NamedObject
	{
		public PreloadData(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDependencies(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_assets = stream.ReadArray<PPtr<Object>>();
			m_dependencies = stream.ReadStringArray();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<Object> passet in Assets)
			{
				yield return passet.FetchDependency(file, isLog, ToLogString, "m_Assets");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		private IReadOnlyList<PPtr<Object>> Assets => m_assets;
		private IReadOnlyList<string> Dependencies => m_dependencies;

		private PPtr<Object>[] m_assets;
		private string[] m_dependencies;
	}
}
