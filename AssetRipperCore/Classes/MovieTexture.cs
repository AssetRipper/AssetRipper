using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class MovieTexture : BaseVideoTexture, IMovieTexture
	{
		public byte[] MovieData
		{
			get => m_MovieData;
			set => m_MovieData = value;
		}

		public MovieTexture(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasData(UnityVersion version) => version.IsLess(5);
		/// <summary>
		/// 5.0.0 to 2019.3 exclusive
		/// </summary>
		public static bool IsInherited(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLess(2019, 3);

		public override void Read(AssetReader reader)
		{
			if (HasData(reader.Version) || IsInherited(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadTexture(reader);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (HasData(context.Version) || IsInherited(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
				{
					yield return asset;
				}
			}
			else
			{
				foreach (PPtr<IUnityObjectBase> asset in FetchDependenciesTexture(context))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			if (HasData(container.Version) || IsInherited(container.Version))
			{
				return base.ExportYAMLRoot(container);
			}
			else
			{
				return ExportYAMLRootTexture(container);
			}
		}
	}
}
