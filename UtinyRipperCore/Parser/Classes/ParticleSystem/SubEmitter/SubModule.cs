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

		private IReadOnlyList<SubEmitterData> GetExportSubEmitters(Version version)
		{
			if (IsReadSubEmitters(version))
			{
				return SubEmitters;
			}
			else
			{
				SubEmitterData[] subEmitters = new SubEmitterData[IsReadSecond(version) ? 6 : 3];
				subEmitters[0] = new SubEmitterData(SubEmitterBirth, ParticleSystemSubEmitterType.Birth);
				subEmitters[1] = new SubEmitterData(SubEmitterDeath, ParticleSystemSubEmitterType.Death);
				subEmitters[2] = new SubEmitterData(SubEmitterCollision, ParticleSystemSubEmitterType.Collision);
				if (IsReadSecond(version))
				{
					subEmitters[3] = new SubEmitterData(SubEmitterBirth1, ParticleSystemSubEmitterType.Birth);
					subEmitters[4] = new SubEmitterData(SubEmitterDeath1, ParticleSystemSubEmitterType.Death);
					subEmitters[5] = new SubEmitterData(SubEmitterCollision1, ParticleSystemSubEmitterType.Collision);
				}
				return subEmitters;
			}
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadSubEmitters(stream.Version))
			{
				m_subEmitters = stream.ReadArray<SubEmitterData>();
			}
			else
			{
				SubEmitterBirth.Read(stream);
				if (IsReadSecond(stream.Version))
				{
					SubEmitterBirth1.Read(stream);
				}
				SubEmitterDeath.Read(stream);
				if (IsReadSecond(stream.Version))
				{
					SubEmitterDeath1.Read(stream);
				}
				SubEmitterCollision.Read(stream);
				if (IsReadSecond(stream.Version))
				{
					SubEmitterCollision1.Read(stream);
				}
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			if (IsReadSubEmitters(file.Version))
			{
				foreach (SubEmitterData subEmitter in SubEmitters)
				{
					foreach(Object @object in subEmitter.FetchDependencies(file, isLog))
					{
						yield return @object;
					}
				}
			}
			else
			{
				yield return SubEmitterBirth.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterBirth");
				yield return SubEmitterDeath.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterDeath");
				yield return SubEmitterCollision.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterCollision");
				if (IsReadSecond(file.Version))
				{
					yield return SubEmitterBirth1.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterBirth1");
					yield return SubEmitterDeath1.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterDeath1");
					yield return SubEmitterCollision1.FetchDependency(file, isLog, () => nameof(SubModule), "m_SubEmitterCollision1");
				}
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("subEmitters", GetExportSubEmitters(container.Version).ExportYAML(container));
			return node;
		}

		public IReadOnlyList<SubEmitterData> SubEmitters => m_subEmitters;

		public PPtr<ParticleSystem> SubEmitterBirth;
		public PPtr<ParticleSystem> SubEmitterBirth1;
		public PPtr<ParticleSystem> SubEmitterDeath;
		public PPtr<ParticleSystem> SubEmitterDeath1;
		public PPtr<ParticleSystem> SubEmitterCollision;
		public PPtr<ParticleSystem> SubEmitterCollision1;

		private SubEmitterData[] m_subEmitters;
	}
}
