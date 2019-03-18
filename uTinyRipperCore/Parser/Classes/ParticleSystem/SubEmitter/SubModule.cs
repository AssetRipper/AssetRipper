using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class SubModule : ParticleSystemModule, IDependent
	{
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadSubEmitters(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadSecond(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadSubEmitters(reader.Version))
			{
				m_subEmitters = reader.ReadAssetArray<SubEmitterData>();
			}
			else
			{
				List<SubEmitterData> subEmitters = new List<SubEmitterData>();
				PPtr<ParticleSystem> subEmitterBirth = reader.ReadAsset<PPtr<ParticleSystem>>();
				if (!subEmitterBirth.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, subEmitterBirth));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterBirth1 = reader.ReadAsset<PPtr<ParticleSystem>>();
					if (!subEmitterBirth1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, subEmitterBirth1));
					}
				}

				PPtr<ParticleSystem> subEmitterDeath = reader.ReadAsset<PPtr<ParticleSystem>>();
				if (!subEmitterDeath.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Death, subEmitterDeath));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterDeath1 = reader.ReadAsset<PPtr<ParticleSystem>>();
					if (!subEmitterDeath1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Death, subEmitterDeath1));
					}
				}

				PPtr<ParticleSystem> subEmitterCollision = reader.ReadAsset<PPtr<ParticleSystem>>();
				if (!subEmitterCollision.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Collision, subEmitterCollision));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterCollision1 = reader.ReadAsset<PPtr<ParticleSystem>>();
					if (!subEmitterCollision1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Collision, subEmitterCollision1));
					}
				}

				if (subEmitters.Count == 0)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, default));
				}
				m_subEmitters = subEmitters.ToArray();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (SubEmitterData subEmitter in SubEmitters)
			{
				foreach (Object asset in subEmitter.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(SubEmittersName, SubEmitters.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<SubEmitterData> SubEmitters => m_subEmitters;

		public const string SubEmittersName = "subEmitters";

		private SubEmitterData[] m_subEmitters;
	}
}
