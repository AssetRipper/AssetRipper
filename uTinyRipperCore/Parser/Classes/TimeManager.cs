using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class TimeManager : GlobalGameManager
	{
		public TimeManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadMaximumAllowedTimestep(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadMaximumParticleTimestep(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			FixedTimestep = reader.ReadSingle();
			if(IsReadMaximumAllowedTimestep(reader.Version))
			{
				MaximumAllowedTimestep = reader.ReadSingle();
			}
			TimeScale = reader.ReadSingle();
			if(IsReadMaximumParticleTimestep(reader.Version))
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

		private float GetMaximumAllowedTimestep(Version version)
		{
			return IsReadMaximumAllowedTimestep(version) ? MaximumAllowedTimestep : 1.0f / 3.0f;
		}
		private float GetMaximumParticleTimestep(Version version)
		{
			return IsReadMaximumParticleTimestep(version) ? MaximumParticleTimestep : 0.03f;
		}

		public float FixedTimestep { get; private set; }
		public float MaximumAllowedTimestep { get; private set; }
		public float TimeScale { get; private set; }
		public float MaximumParticleTimestep { get; private set; }

		public const string FixedTimestepName = "Fixed Timestep";
		public const string MaximumAllowedTimestepName = "Maximum Allowed Timestep";
		public const string TimeScaleName = "m_TimeScale";
		public const string MaximumParticleTimestepName = "Maximum Particle Timestep";
	}
}
