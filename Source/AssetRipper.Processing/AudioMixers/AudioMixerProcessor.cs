using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
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
using AssetRipper.SourceGenerated.Subclasses.Utf8String;

namespace AssetRipper.Processing.AudioMixers
{
	public sealed class AudioMixerProcessor : IAssetProcessor
	{
		//Many uses of IAudioMixerGroupController should be IAudioMixerGroup.

		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Reconstruct AudioMixer Assets");

			ProcessedAssetCollection processedCollection = gameBundle.AddNewProcessedCollection("Generated Audio Mixer Effects", projectVersion);

			Dictionary<UnityGUID, IAudioMixerGroup> groupGuidMap = new();
			foreach (IAudioMixerGroup group in gameBundle.FetchAssets().OfType<IAudioMixerGroup>())
			{
				group.MainAsset = group.AudioMixer_C273P;
				groupGuidMap.Add(group.GroupID_C273, group);
			}

			foreach (IAudioMixer mixer in gameBundle.FetchAssets().OfType<IAudioMixer>())
			{
				mixer.MainAsset = mixer;
				ProcessAssets(mixer, processedCollection, groupGuidMap);
			}
		}

		private static void ProcessAssets(
			IAudioMixer mixer,
			ProcessedAssetCollection virtualFile,
			IReadOnlyDictionary<UnityGUID, IAudioMixerGroup> groupGuidMap)
		{
			Dictionary<uint, UnityGUID> indexToGuid = new();
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
			Dictionary<uint, UnityGUID> indexToGuid,
			List<IAudioMixerGroupController> groups,
			IReadOnlyDictionary<UnityGUID, IAudioMixerGroup> groupGuidMap)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;

			groups.AddRange(mixer.MixerConstant_C240.GroupGUIDs.Select(guid => (IAudioMixerGroupController)groupGuidMap[guid]));
			for (int i = 0; i < groups.Count; i++)
			{
				IAudioMixerGroupController group = groups[i];
				IGroupConstant groupConstant = constants.Groups[i];

				group.Volume_C243.CopyValues(IndexNewGuid(indexToGuid, groupConstant.VolumeIndex));
				group.Pitch_C243.CopyValues(IndexNewGuid(indexToGuid, groupConstant.PitchIndex));

				// Different Unity versions vary in whether a "send" field is used in groups as well as in snapshots.
				// GroupConstant.Has_SendIndex() can be used to determine its existence.
				// If "send" does not exist in GroupConstant, it may still exist in AudioMixerGroupController, but is just ignored.
				if (groupConstant.Has_SendIndex() && group.Has_Send_C243())
				{
					group.Send_C243.CopyValues(IndexNewGuid(indexToGuid, groupConstant.SendIndex));
				}
				group.Mute_C243 = groupConstant.Mute;
				group.Solo_C243 = groupConstant.Solo;
				group.BypassEffects_C243 = groupConstant.BypassEffects;
			}
		}

		private static void ProcessAudioMixerEffects(
			IAudioMixer mixer,
			Dictionary<uint, UnityGUID> indexToGuid,
			IReadOnlyList<IAudioMixerGroupController> groups,
			ProcessedAssetCollection virtualFile)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;

			IAudioMixerEffectController[] effects = new IAudioMixerEffectController[constants.Effects.Count];

			HashSet<IAudioMixerGroupController> groupsWithAttenuation = new();

			for (int i = 0; i < effects.Length; i++)
			{
				IAudioMixerEffectController effect = virtualFile.CreateAsset((int)ClassIDType.AudioMixerEffectController, AudioMixerEffectControllerFactory.CreateAsset);
				effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				effects[i] = effect;
				effect.MainAsset = mixer;
			}

			List<Utf8String> pluginEffectNames = ParseNameBuffer(constants.PluginEffectNameBuffer);
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
					effect.EffectName_C244.CopyValues(pluginEffectNames[pluginEffectIndex++]);
				}
				else
				{
					string name = AudioEffectDefinitions.EffectTypeToName(effectConstant.Type) ?? "Unknown";
					effect.EffectName_C244.String = name;
					if (name == "Attenuation")
					{
						groupsWithAttenuation.Add(group);
					}
				}

				bool enableWetMix = effectConstant.WetMixLevelIndex != uint.MaxValue;
				if (enableWetMix || effect.EffectName_C244 == "Send")
				{
					effect.MixLevel_C244.CopyValues(IndexNewGuid(indexToGuid, effectConstant.WetMixLevelIndex));
				}

				for (int j = 0; j < effectConstant.ParameterIndices.Length; j++)
				{
					Parameter param = effect.Parameters_C244.AddNew();
					// Use a dummy name here. The actual name will be recovered by AssetRipperAudioMixerPostprocessor.
					param.ParameterName.String = $"Param_{j}";
					param.GUID.CopyValues(IndexNewGuid(indexToGuid, effectConstant.ParameterIndices[j]));
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
					IAudioMixerEffectController effect = virtualFile.CreateAsset((int)ClassIDType.AudioMixerEffectController, AudioMixerEffectControllerFactory.CreateAsset);
					effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					effect.EffectID_C244.CopyValues(UnityGUID.NewGuid());
					effect.EffectName_C244.String = "Attenuation";
					effect.MainAsset = mixer;

					group.Effects_C243P.Add(effect);
				}
			}
		}

		private static void ProcessAudioMixerSnapshots(
			IAudioMixer mixer,
			IReadOnlyDictionary<uint, UnityGUID> indexToGuid)
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
					for (uint j = 0; j < snapshotConstant.Values.Length; j++)
					{
						if (indexToGuid.TryGetValue(j, out UnityGUID valueGuid))
						{
							snapshotController.FloatValues_C245[(GUID)valueGuid] = snapshotConstant.Values[j];
						}
						else
						{
							Logger.Warning(LogCategory.Processing, $"Snapshot({snapshotController.Name_C130}) value #{j} has no binding parameter");
						}
					}

					for (int j = 0; j < snapshotConstant.TransitionIndices.Length; j++)
					{
						uint paramIndex = snapshotConstant.TransitionIndices[j];
						int transitionType = (int)snapshotConstant.TransitionTypes[j];
						if (indexToGuid.TryGetValue(paramIndex, out UnityGUID paramGuid))
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
			IReadOnlyDictionary<uint, UnityGUID> indexToGuid,
			IReadOnlyList<IAudioMixerGroupController> groups)
		{
			IAudioMixerConstant constants = mixer.MixerConstant_C240;
			// generate exposed parameters
			mixer.ExposedParameters_C241.Clear();
			for (int i = 0; i < constants.ExposedParameterIndices.Length; i++)
			{
				uint paramIndex = constants.ExposedParameterIndices[i];
				uint paramNameCrc = constants.ExposedParameterNames[i];
				if (indexToGuid.TryGetValue(paramIndex, out UnityGUID paramGuid))
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
			groupView.NameString = "View";
			foreach (IAudioMixerGroupController group in groups)
			{
				groupView.Guids.Add(group.GroupID_C243);
			}
			mixer.CurrentViewIndex_C241 = 0;
			mixer.TargetSnapshot_C241P = mixer.StartSnapshot_C241P;
		}

		private static UnityGUID IndexNewGuid(Dictionary<uint, UnityGUID> table, uint index)
		{
			UnityGUID guid = UnityGUID.NewGuid();
			if (!table.TryAdd(index, guid))
			{
				Logger.Warning(LogCategory.Processing, $"Constant index #{index} conflicts with another one.");
			}
			return guid;
		}

		private static List<Utf8String> ParseNameBuffer(byte[] buffer)
		{
			List<Utf8String> names = new();
			int offset = 0;
			while (buffer[offset] != 0)
			{
				int start = offset;
				while (buffer[++offset] != 0) { }

				byte[] utf8Data = SubArray(buffer, start, offset - start);
				names.Add(new Utf8String { Data = utf8Data });

				offset++;
			}

			return names;
		}

		private static byte[] SubArray(byte[] data, int index, int length)
		{
			byte[] subArray = new byte[length];
			Array.Copy(data, index, subArray, 0, length);
			return subArray;
		}
	}
}
