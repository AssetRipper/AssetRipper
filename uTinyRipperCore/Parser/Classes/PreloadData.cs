using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_assets = reader.ReadAssetArray<PPtr<Object>>();
			if(IsReadDependencies(reader.Version))
			{
				m_dependencies = reader.ReadStringArray();
			}
			if(IsReadExplicitDataLayout(reader.Version))
			{
				ExplicitDataLayout = reader.ReadBoolean();
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
