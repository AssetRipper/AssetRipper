using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class TimeManager : GlobalGameManager
	{
		public TimeManager(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasMaximumAllowedTimestep(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMaximumParticleTimestep(UnityVersion version) => version.IsGreaterEqual(5, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			FixedTimestep = reader.ReadSingle();
			if (HasMaximumAllowedTimestep(reader.Version))
			{
				MaximumAllowedTimestep = reader.ReadSingle();
			}
			TimeScale = reader.ReadSingle();
			if (HasMaximumParticleTimestep(reader.Version))
			{
				MaximumParticleTimestep = reader.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(FixedTimestepName, FixedTimestep);
			node.Add(MaximumAllowedTimestepName, GetMaximumAllowedTimestep(container.Version));
			node.Add(TimeScaleName, TimeScale);
			node.Add(MaximumParticleTimestepName, GetMaximumParticleTimestep(container.Version));
			return node;
		}

		private float GetMaximumAllowedTimestep(UnityVersion version)
		{
			return HasMaximumAllowedTimestep(version) ? MaximumAllowedTimestep : 1.0f / 3.0f;
		}
		private float GetMaximumParticleTimestep(UnityVersion version)
		{
			return HasMaximumParticleTimestep(version) ? MaximumParticleTimestep : 0.03f;
		}

		public float FixedTimestep { get; set; }
		public float MaximumAllowedTimestep { get; set; }
		public float TimeScale { get; set; }
		public float MaximumParticleTimestep { get; set; }

		public const string FixedTimestepName = "Fixed Timestep";
		public const string MaximumAllowedTimestepName = "Maximum Allowed Timestep";
		public const string TimeScaleName = "m_TimeScale";
		public const string MaximumParticleTimestepName = "Maximum Particle Timestep";
	}
}
