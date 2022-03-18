using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Flare
{
	public sealed class LensFlare : Behaviour
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasFadeSpeed(UnityVersion version) => version.IsGreaterEqual(4, 3, 0);

		public LensFlare(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Flare.Read(reader);
			Color.Read(reader);
			Brightness = reader.ReadSingle();
			if (HasFadeSpeed(reader.Version))
			{
				FadeSpeed = reader.ReadSingle();
			}
			IgnoreLayers.Read(reader);
			Directional = reader.ReadBoolean();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Flare, "m_Flare");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Flare", Flare.ExportYAML(container));
			node.Add("m_Color", Color.ExportYAML(container));
			node.Add("m_Brightness", Brightness);
			if (HasFadeSpeed(container.ExportVersion))
			{
				node.Add("m_FadeSpeed", FadeSpeed);
			}
			node.Add("m_IgnoreLayers", IgnoreLayers.ExportYAML(container));
			node.Add("m_Directional", Directional);
			return node;
		}

		public PPtr<Flare> Flare = new();
		public ColorRGBAf Color = new();
		public BitField IgnoreLayers = new();

		public float Brightness { get; set; }
		public float FadeSpeed { get; set; }
		public bool Directional { get; set; }
	}
}
