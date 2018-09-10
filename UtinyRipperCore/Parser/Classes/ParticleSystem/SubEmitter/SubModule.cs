using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
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
				m_subEmitters = reader.ReadArray<SubEmitterData>();
			}
			else
			{
				List<SubEmitterData> subEmitters = new List<SubEmitterData>();
				PPtr<ParticleSystem> subEmitterBirth = reader.Read<PPtr<ParticleSystem>>();
				if (!subEmitterBirth.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, subEmitterBirth));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterBirth1 = reader.Read<PPtr<ParticleSystem>>();
					if (!subEmitterBirth1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, subEmitterBirth1));
					}
				}

				PPtr<ParticleSystem> subEmitterDeath = reader.Read<PPtr<ParticleSystem>>();
				if (!subEmitterDeath.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Death, subEmitterDeath));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterDeath1 = reader.Read<PPtr<ParticleSystem>>();
					if (!subEmitterDeath1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Death, subEmitterDeath1));
					}
				}

				PPtr<ParticleSystem> subEmitterCollision = reader.Read<PPtr<ParticleSystem>>();
				if (!subEmitterCollision.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Collision, subEmitterCollision));
				}
				if (IsReadSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterCollision1 = reader.Read<PPtr<ParticleSystem>>();
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
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("subEmitters", SubEmitters.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<SubEmitterData> SubEmitters => m_subEmitters;

		private SubEmitterData[] m_subEmitters;
	}
}
