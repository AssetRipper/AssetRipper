using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

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

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(Assets, AssetsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<PPtr<Object>> Assets => m_assets;
		public IReadOnlyList<string> Dependencies => m_dependencies;
		public bool ExplicitDataLayout { get; private set; }

		public const string AssetsName = "m_Assets";

		private PPtr<Object>[] m_assets;
		private string[] m_dependencies;
	}
}
