using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes
{
	public sealed class PreloadData : NamedObject
	{
		public PreloadData(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasDependencies(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasExplicitDataLayout(UnityVersion version) => version.IsGreaterEqual(2018, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Assets = reader.ReadAssetArray<PPtr<Object.Object>>();
			if (HasDependencies(reader.Version))
			{
				Dependencies = reader.ReadStringArray();
			}
			if (HasExplicitDataLayout(reader.Version))
			{
				ExplicitDataLayout = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Assets, AssetsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<Object.Object>[] Assets { get; set; }
		public string[] Dependencies { get; set; }
		public bool ExplicitDataLayout { get; set; }

		public const string AssetsName = "m_Assets";
	}
}
