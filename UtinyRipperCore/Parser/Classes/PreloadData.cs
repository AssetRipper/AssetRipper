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
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadExplicitDataLayout(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		
		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_assets = stream.ReadArray<PPtr<Object>>();
			if(IsReadDependencies(stream.Version))
			{
				m_dependencies = stream.ReadStringArray();
			}
			if(IsReadExplicitDataLayout(stream.Version))
			{
				ExplicitDataLayout = stream.ReadBoolean();
			}
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

		public IReadOnlyList<PPtr<Object>> Assets => m_assets;
		public IReadOnlyList<string> Dependencies => m_dependencies;
		public bool ExplicitDataLayout { get; private set; }

		private PPtr<Object>[] m_assets;
		private string[] m_dependencies;
	}
}
