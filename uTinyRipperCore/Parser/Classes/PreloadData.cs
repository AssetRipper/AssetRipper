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
		public static bool HasDependencies(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasExplicitDataLayout(Version version) => version.IsGreaterEqual(2018, 2);
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Assets = reader.ReadAssetArray<PPtr<Object>>();
			if (HasDependencies(reader.Version))
			{
				Dependencies = reader.ReadStringArray();
			}
			if (HasExplicitDataLayout(reader.Version))
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

		public PPtr<Object>[] Assets { get; set; }
		public string[] Dependencies { get; set; }
		public bool ExplicitDataLayout { get; set; }

		public const string AssetsName = "m_Assets";
	}
}
