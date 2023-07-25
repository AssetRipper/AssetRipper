using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Utils;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_244;
using AssetRipper.SourceGenerated.Classes.ClassID_245;
using AssetRipper.SourceGenerated.Classes.ClassID_272;
using AssetRipper.SourceGenerated.Classes.ClassID_273;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AudioMixerConstant;
using AssetRipper.SourceGenerated.Subclasses.AudioMixerGroupView;
using AssetRipper.SourceGenerated.Subclasses.EffectConstant;
using AssetRipper.SourceGenerated.Subclasses.ExposedAudioParameter;
using AssetRipper.SourceGenerated.Subclasses.GroupConstant;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.Parameter;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AudioMixerSnapshot;
using AssetRipper.SourceGenerated.Subclasses.SnapshotConstant;

namespace AssetRipper.Processing.AudioMixers
{
	public sealed class AudioMixerProcessor : IAssetProcessor
	{
		private static Utf8String ViewName { get; } = new("View");

		//Many uses of IAudioMixerGroupController should be IAudioMixerGroup.

		public void Process(GameData gameData)
		{
			Logger.Info(LogCategory.Processing, "Reconstruct AudioMixer Assets");

			ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Generated Audio Mixer Effects");

			Dictionary<IAudioMixer, Dictionary<UnityGuid, IAudioMixerGroup >> groupGuidMixerMap = new();
			foreach (IAudioMixerGroup group in gameData.GameBundle.FetchAssets().OfType<IAudioMixerGroup>())
			{
				IAudioMixer? mixer = group.AudioMixer_C273P;
				if (mixer is not null)
				{
					group.MainAsset = mixer;
					groupGuidMixerMap.GetOrAdd(mixer).Add(group.GroupID_C273, group);
				}
			}

			foreach (IAudioMixer mixer in gameData.GameBundle.FetchAssets().OfType<IAudioMixer>())
			{
				mixer.MainAsset = mixer;
				ProcessAssets(mixer, processedCollection, groupGuidMixerMap.GetOrAdd(mixer));
			}
		}

		private static void ProcessAssets(
			IAudioMixer mixer,
			ProcessedAssetCollection virtualFile,
			IReadOnlyDictionary<UnityGuid, IAudioMixerGroup> groupGuidMap)
		{
			GuidIndexTable indexToGuid = new();
			List<IAudioMixerGroupController> groups = new();

			ProcessAudioMixerGroups(mixer, indexToGuid, groups, groupGuidMap);
			ProcessAudioMixerEffects(mixer, indexToGuid, groups, virtualFile);
			ProcessAudioMixerSnapshots(mixer, indexToGuid);
			if (mixer is IAudioMixerController mixerController)
			{
				ProcessAudioMixer(mixerController, indexToGuid, groups);
			}
		}

		private static void ProcessAudioMixerGroups(
			IAudioMixer mixer,
			GuidIndexTable indexToGuid,
			List<IAudioMixerGroupController> groups,
			IReadOnlyDictionary<UnityGuid, IAudioMixerGroup> groupGuidMap)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;

			groups.AddRange(mixer.MixerConstant_C240.GroupGUIDs.Select(guid => (IAudioMixerGroupController)groupGuidMap[guid]));
			for (int i = 0; i < groups.Count; i++)
			{
				IAudioMixerGroupController group = groups[i];
				IGroupConstant groupConstant = constants.Groups[i];

				group.Volume_C243.CopyValues(indexToGuid.IndexNewGuid(groupConstant.VolumeIndex));
				group.Pitch_C243.CopyValues(indexToGuid.IndexNewGuid(groupConstant.PitchIndex));

				// Different Unity versions vary in whether a "send" field is used in groups as well as in snapshots.
				// GroupConstant.Has_SendIndex() can be used to determine its existence.
				// If "send" does not exist in GroupConstant, it may still exist in AudioMixerGroupController, but is just ignored.
				if (groupConstant.Has_SendIndex() && group.Has_Send_C243())
				{
					group.Send_C243.CopyValues(indexToGuid.IndexNewGuid(groupConstant.SendIndex));
				}
				group.Mute_C243 = groupConstant.Mute;
				group.Solo_C243 = groupConstant.Solo;
				group.BypassEffects_C243 = groupConstant.BypassEffects;
			}
		}

		private static void ProcessAudioMixerEffects(
			IAudioMixer mixer,
			GuidIndexTable indexToGuid,
			IReadOnlyList<IAudioMixerGroupController> groups,
			ProcessedAssetCollection virtualFile)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;

			IAudioMixerEffectController[] effects = new IAudioMixerEffectController[constants.Effects.Count];

			HashSet<IAudioMixerGroupController> groupsWithAttenuation = new();

			for (int i = 0; i < effects.Length; i++)
			{
				IAudioMixerEffectController effect = virtualFile.CreateAsset((int)ClassIDType.AudioMixerEffectController, AudioMixerEffectController.Create);
				effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				effects[i] = effect;
				effect.MainAsset = mixer;
			}

			Utf8String[] pluginEffectNames = ParseNameBuffer(constants.PluginEffectNameBuffer);
			int pluginEffectIndex = 0;
			for (int i = 0; i < constants.Effects.Count; i++)
			{
				EffectConstant effectConstant = constants.Effects[i];
				IAudioMixerEffectController effect = effects[i];

				IAudioMixerGroupController group = groups[(int)effectConstant.GroupConstantIndex];
				group.Effects_C243P.Add(effect);

				effect.EffectID_C244.CopyValues(constants.EffectGUIDs[i]);

				if (AudioEffectDefinitions.IsPluginEffect(effectConstant.Type))
				{
					effect.EffectName_C244 = pluginEffectNames[pluginEffectIndex++];
				}
				else
				{
					string name = AudioEffectDefinitions.EffectTypeToName(effectConstant.Type) ?? "Unknown";
					effect.EffectName_C244 = name;
					if (name == "Attenuation")
					{
						groupsWithAttenuation.Add(group);
					}
				}

				bool enableWetMix = effectConstant.WetMixLevelIndex != uint.MaxValue;
				if (enableWetMix || effect.EffectName_C244 == "Send")
				{
					effect.MixLevel_C244.CopyValues(indexToGuid.IndexNewGuid(effectConstant.WetMixLevelIndex));
				}

				for (int j = 0; j < effectConstant.ParameterIndices.Count; j++)
				{
					Parameter param = effect.Parameters_C244.AddNew();
					// Use a dummy name here. The actual name will be recovered by AssetRipperAudioMixerPostprocessor.
					param.ParameterName = $"Param_{j}";
					param.GUID.CopyValues(indexToGuid.IndexNewGuid(effectConstant.ParameterIndices[j]));
				}

				if (effectConstant.SendTargetEffectIndex != uint.MaxValue)
				{
					effect.SendTarget_C244P = effects[effectConstant.SendTargetEffectIndex];
				}
				effect.EnableWetMix_C244 = enableWetMix;
				effect.Bypass_C244 = effectConstant.Bypass;
			}

			// Append an Attenuation effect to a group if it has not yet got one,
			// as Unity doesn't store Attenuation effect if it is the last.
			foreach (IAudioMixerGroupController group in groups)
			{
				if (!groupsWithAttenuation.Contains(group))
				{
					IAudioMixerEffectController effect = virtualFile.CreateAsset((int)ClassIDType.AudioMixerEffectController, AudioMixerEffectController.Create);
					effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					effect.EffectID_C244.CopyValues(UnityGuid.NewGuid());
					effect.EffectName_C244 = "Attenuation";
					effect.MainAsset = mixer;

					group.Effects_C243P.Add(effect);
				}
			}
		}

		private static void ProcessAudioMixerSnapshots(
			IAudioMixer mixer,
			GuidIndexTable indexToGuid)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;
			PPtrAccessList<PPtr_AudioMixerSnapshot, IAudioMixerSnapshot> snapshots = mixer.Snapshots_C240P;
			for (int i = 0; i < snapshots.Count; i++)
			{
				IAudioMixerSnapshot? snapshot = snapshots[i];
				if (snapshot is null)
				{
					continue;
				}

				snapshot.MainAsset = mixer;

				if (snapshot is IAudioMixerSnapshotController snapshotController)
				{
					SnapshotConstant snapshotConstant = constants.Snapshots[i];
					for (int j = 0; j < snapshotConstant.Values.Count; j++)
					{
						if (indexToGuid.TryGetValue((uint)j, out UnityGuid valueGuid))
						{
							GUID guid = (GUID)valueGuid;
							if (!snapshotController.FloatValues_C245.TryGetSinglePairForKey(guid, out AccessPairBase<GUID, float>? pair))
							{
								pair = snapshotController.FloatValues_C245.AddNew();
								pair.Key.CopyValues(guid);
							}
							pair.Value = snapshotConstant.Values[j];
						}
						else
						{
							Logger.Warning(LogCategory.Processing, $"Snapshot({snapshotController.Name_C130}) value #{j} has no binding parameter");
						}
					}

					for (int j = 0; j < snapshotConstant.TransitionIndices.Count; j++)
					{
						uint paramIndex = snapshotConstant.TransitionIndices[j];
						int transitionType = (int)snapshotConstant.TransitionTypes[j];
						if (indexToGuid.TryGetValue(paramIndex, out UnityGuid paramGuid))
						{
							snapshotController.TransitionOverrides_C245[(GUID)paramGuid] = transitionType;
						}
						else
						{
							Logger.Warning(LogCategory.Processing, $"Snapshot({snapshotController.Name_C130}) transition #{paramIndex} has no binding parameter");
						}
					}
				}
			}
		}

		private static void ProcessAudioMixer(
			IAudioMixerController mixer,
			GuidIndexTable indexToGuid,
			IReadOnlyList<IAudioMixerGroupController> groups)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;
			// generate exposed parameters
			mixer.ExposedParameters_C241.Clear();
			for (int i = 0; i < constants.ExposedParameterIndices.Count; i++)
			{
				uint paramIndex = constants.ExposedParameterIndices[i];
				uint paramNameCrc = constants.ExposedParameterNames[i];
				if (indexToGuid.TryGetValue(paramIndex, out UnityGuid paramGuid))
				{
					ExposedAudioParameter exposedParam = mixer.ExposedParameters_C241.AddNew();
					exposedParam.Guid.CopyValues(paramGuid);
					exposedParam.NameString = CrcUtils.ReverseDigestAscii(paramNameCrc);
				}
				else
				{
					Logger.Warning(LogCategory.Processing, $"Exposed parameter #{paramIndex} has no binding parameter");
				}
			}

			// complete mixer controller
			mixer.AudioMixerGroupViews_C241.Clear();
			IAudioMixerGroupView groupView = mixer.AudioMixerGroupViews_C241.AddNew();
			groupView.Name = ViewName;
			foreach (IAudioMixerGroupController group in groups)
			{
				groupView.Guids.AddNew().CopyValues(group.GroupID_C243);
			}
			mixer.CurrentViewIndex_C241 = 0;
			mixer.TargetSnapshot_C241P = mixer.StartSnapshot_C241P;
		}

		private static Utf8String[] ParseNameBuffer(ReadOnlySpan<byte> buffer)
		{
			Utf8String[] names = new Utf8String[CountStrings(buffer)];
			int index = 0;
			int offset = 0;
			while (buffer[offset] != 0)
			{
				int start = offset;
				while (buffer[++offset] != 0) { }

				names[index] = new Utf8String(buffer[start..offset]);

				index++;
				offset++;
			}

			return names;

			static int CountStrings(ReadOnlySpan<byte> buffer)
			{
				int count = 0;
				for (int i = 0; i < buffer.Length; i++)
				{
					if (buffer[i] == 0)
					{
						count++;
					}
				}
				if (buffer[^1] != 0)
				{
					count++;
				}
				return count;
			}
		}
	}
}
