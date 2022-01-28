using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystem.SubEmitter
{
	public sealed class SubModule : ParticleSystemModule, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasSubEmitters(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasSecond(UnityVersion version) => version.IsGreaterEqual(4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasSubEmitters(reader.Version))
			{
				SubEmitters = reader.ReadAssetArray<SubEmitterData>();
			}
			else
			{
				List<SubEmitterData> subEmitters = new List<SubEmitterData>();
				PPtr<ParticleSystem> subEmitterBirth = reader.ReadAsset<PPtr<ParticleSystem>>();
				if (!subEmitterBirth.IsNull)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, subEmitterBirth));
				}
				if (HasSecond(reader.Version))
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
				if (HasSecond(reader.Version))
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
				if (HasSecond(reader.Version))
				{
					PPtr<ParticleSystem> subEmitterCollision1 = reader.ReadAsset<PPtr<ParticleSystem>>();
					if (!subEmitterCollision1.IsNull)
					{
						subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Collision, subEmitterCollision1));
					}
				}

				if (subEmitters.Count == 0)
				{
					subEmitters.Add(new SubEmitterData(ParticleSystemSubEmitterType.Birth, new()));
				}
				SubEmitters = subEmitters.ToArray();
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(SubEmitters, SubEmittersName))
			{
				yield return asset;
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SubEmittersName, SubEmitters.ExportYAML(container));
			return node;
		}

		public SubEmitterData[] SubEmitters { get; set; }

		public const string SubEmittersName = "subEmitters";
	}
}
