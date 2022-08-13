using AssetRipper.Core;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_244;
using AssetRipper.SourceGenerated.Classes.ClassID_245;
using AssetRipper.SourceGenerated.Subclasses.AudioMixerConstant;
using AssetRipper.SourceGenerated.Subclasses.AudioMixerGroupView;
using AssetRipper.SourceGenerated.Subclasses.EffectConstant;
using AssetRipper.SourceGenerated.Subclasses.ExposedAudioParameter;
using AssetRipper.SourceGenerated.Subclasses.GroupConstant;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.Parameter;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AudioMixerEffectController_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AudioMixerSnapshot_;
using AssetRipper.SourceGenerated.Subclasses.SnapshotConstant;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using Cpp2IL.Core;
using System.Collections.Generic;
using System.Linq;
using Logger = AssetRipper.Core.Logging.Logger;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioMixerExportCollection : AssetsExportCollection
	{
		public AudioMixerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile,
			IAudioMixerController mixer) : base(assetExporter, mixer)
		{
			AssetsProcessingContext context = new()
			{
				mixer = mixer,
				constants = mixer.MixerConstant_C241,
				virtualFile = virtualFile,
				indexToGuid = new(),
			};
			ProcessAssets(ref context);
		}

		private struct AssetsProcessingContext
		{
			public IAudioMixerController mixer;
			public IAudioMixerConstant constants;
			public VirtualSerializedFile virtualFile;
			public Dictionary<uint, GUID> indexToGuid;
			public IAudioMixerGroupController[] groups;
		}

		private void ProcessAssets(ref AssetsProcessingContext context)
		{
			ProcessAudioMixerGroups(ref context);
			ProcessAudioMixerEffects(ref context);
			ProcessAudioMixerSnapshots(ref context);
			ProcessAudioMixer(ref context);
		}

		private void ProcessAudioMixerGroups(ref AssetsProcessingContext context)
		{
			Dictionary<GUID, IAudioMixerGroupController> groupGuidMap = new();
			
			foreach (IAudioMixerGroupController group in context.mixer.SerializedFile.Collection.FetchAssetsOfType<IAudioMixerGroupController>())
			{
				if (group.AudioMixer_C243.IsAsset(context.mixer.SerializedFile, context.mixer))
				{
					AddAsset(group);
					groupGuidMap.Add(group.GroupID_C243, group);
				}
			}
			
			context.groups = context.mixer.MixerConstant_C241.GroupGUIDs.Select(guid => groupGuidMap[guid]).ToArray();
			for (int i = 0; i < context.groups.Length; i++)
			{
				IAudioMixerGroupController group = context.groups[i];
				IGroupConstant groupConstant = context.constants.Groups[i];
				
				group.Volume_C243.CopyValues(IndexingNewGuid(groupConstant.VolumeIndex, context.indexToGuid));
				group.Pitch_C243.CopyValues(IndexingNewGuid(groupConstant.PitchIndex, context.indexToGuid));
				
				// Different Unity versions vary in whether a "send" field is used in groups as well as in snapshots.
				// GroupConstant.Has_SendIndex() can be used to determine its existence.
				// If "send" does not exist in GroupConstant, it may still exist in AudioMixerGroupController, but is just ignored.
				if (groupConstant.Has_SendIndex() && group.Has_Send_C243())
				{
					group.Send_C243.CopyValues(IndexingNewGuid(groupConstant.SendIndex, context.indexToGuid));
				}
				group.Mute_C243 = groupConstant.Mute;
				group.Solo_C243 = groupConstant.Solo;
				group.BypassEffects_C243 = groupConstant.BypassEffects;
			}
		}

		private void ProcessAudioMixerEffects(ref AssetsProcessingContext context)
		{
			IAudioMixerConstant constants = context.mixer.MixerConstant_C241;

			(IAudioMixerEffectController, PPtr_AudioMixerEffectController_)[] effects =
				new (IAudioMixerEffectController, PPtr_AudioMixerEffectController_)[constants.Effects.Count];
			
			HashSet<IAudioMixerGroupController> groupsWithAttenuation = new();
			
			for (int i = 0; i < effects.Length; i++)
			{
				IAudioMixerEffectController effect = context.virtualFile.CreateAsset<IAudioMixerEffectController>(ClassIDType.AudioMixerEffectController);
				effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				PPtr_AudioMixerEffectController_ effectPPtr = new();
				effectPPtr.CopyValues(effect.SerializedFile.CreatePPtr(effect));
				effects[i] = (effect, effectPPtr);
				AddAsset(effect);
			}
			
			List<Utf8String> pluginEffectNames = ParseNameBuffer(constants.PluginEffectNameBuffer);
			int pluginEffectIndex = 0;
			for (int i = 0; i < constants.Effects.Count; i++)
			{
				EffectConstant effectConstant = constants.Effects[i];
				(IAudioMixerEffectController effect, PPtr_AudioMixerEffectController_ effectPPtr) = effects[i];

				IAudioMixerGroupController group = context.groups[effectConstant.GroupConstantIndex];
				group.Effects_C243.Add(effectPPtr);

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

				bool enableWetMix = (int)effectConstant.WetMixLevelIndex != -1;
				if (enableWetMix || effect.EffectName_C244 == "Send")
				{
					effect.MixLevel_C244.CopyValues(IndexingNewGuid(effectConstant.WetMixLevelIndex, context.indexToGuid));
				}

				for (int j = 0; j < effectConstant.ParameterIndices.Length; j++)
				{
					Parameter param = effect.Parameters_C244.AddNew();
					// Use a dummy name here. The actual name will be recovered by AssetRipperAudioMixerPostprocessor.
					param.ParameterName.String = $"Param_{j}";
					HasAnyEffectParameterNameToRecover = true;
					param.GUID.CopyValues(IndexingNewGuid(effectConstant.ParameterIndices[j], context.indexToGuid));
				}
				
				if ((int)effectConstant.SendTargetEffectIndex != -1)
				{
					effect.SendTarget_C244.CopyValues(effects[effectConstant.SendTargetEffectIndex].Item2);
				}
				effect.EnableWetMix_C244 = enableWetMix;
				effect.Bypass_C244 = effectConstant.Bypass;
			}
			
			// append an Attenuation effect to a group if it has not yet got one,
			// as Unity doesn't store Attenuation effect if it is the last.
			foreach (IAudioMixerGroupController group in context.groups)
			{
				if (!groupsWithAttenuation.Contains(group))
				{
					IAudioMixerEffectController effect = context.virtualFile.CreateAsset<IAudioMixerEffectController>(ClassIDType.AudioMixerEffectController);
					effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					PPtr_AudioMixerEffectController_ effectPPtr = new();
					effectPPtr.CopyValues(effect.SerializedFile.CreatePPtr(effect));
					group.Effects_C243.Add(effectPPtr);
					AddAsset(effect);
					
					effect.EffectID_C244.CopyValues((GUID)UnityGUID.NewGuid());
					effect.EffectName_C244.String = "Attenuation";
				}
			}
		}

		private void ProcessAudioMixerSnapshots(ref AssetsProcessingContext context)
		{
			for (int i = 0; i < context.mixer.Snapshots_C241.Count; i++)
			{
				PPtr_AudioMixerSnapshot_ snapshotPPtr = context.mixer.Snapshots_C241[i];
				IAudioMixerSnapshotController snapshot = (IAudioMixerSnapshotController)snapshotPPtr.GetAsset(context.mixer.SerializedFile);
				AddAsset(snapshot);
				
				SnapshotConstant snapshotConstant = context.constants.Snapshots[i];
				for (int j = 0; j < snapshotConstant.Values.Length; j++)
				{
					if (context.indexToGuid.TryGetValue((uint)j, out GUID? valueGuid))
					{
						snapshot.FloatValues_C245[valueGuid] = snapshotConstant.Values[j];
					}
					else
					{
						Logger.Warning(LogCategory.Export, $"Snapshot({snapshot.Name_C245}) value #{j} has no binding parameter");
					}
				}

				for (int j = 0; j < snapshotConstant.TransitionIndices.Length; j++)
				{
					uint paramIndex = snapshotConstant.TransitionIndices[j];
					int transitionType = (int)snapshotConstant.TransitionTypes[j];
					if (context.indexToGuid.TryGetValue(paramIndex, out GUID? paramGuid))
					{
						snapshot.TransitionOverrides_C245[paramGuid] = transitionType;
					}
					else
					{
						Logger.Warning(LogCategory.Export, $"Snapshot({snapshot.Name_C245}) transition #{paramIndex} has no binding parameter");
					}
				}
			}
		}

		private void ProcessAudioMixer(ref AssetsProcessingContext context)
		{
			// generate exposed parameters
			for (int i = 0; i < context.constants.ExposedParameterIndices.Length; i++)
			{
				uint paramIndex = context.constants.ExposedParameterIndices[i];
				uint paramNameCrc = context.constants.ExposedParameterNames[i];
				if (context.indexToGuid.TryGetValue(paramIndex, out GUID? paramGuid))
				{
					ExposedAudioParameter exposedParam = context.mixer.ExposedParameters_C241.AddNew();
					exposedParam.Guid.CopyValues(paramGuid);
					exposedParam.NameString = $"{CrcUtils.ReverseDigestAscii(paramNameCrc)}";
				}
				else
				{
					Logger.Warning(LogCategory.Export, $"Exposed parameter #{paramIndex} has no binding parameter");
				}
			}
			
			// complete mixer controller
			IAudioMixerGroupView groupView = context.mixer.AudioMixerGroupViews_C241.AddNew();
			groupView.NameString = "View";
			foreach (IAudioMixerGroupController group in context.groups)
			{
				groupView.Guids.Add(group.GroupID_C243);
			}
			context.mixer.CurrentViewIndex_C241 = 0;
			context.mixer.TargetSnapshot_C241.CopyValues(context.mixer.StartSnapshot_C241);
		}
		
		private GUID IndexingNewGuid(uint index, Dictionary<uint, GUID> table)
		{
			GUID guid = (GUID)UnityGUID.NewGuid();
			if (!table.TryAdd(index, guid))
			{
				Logger.Warning(LogCategory.Export, $"Constant index #{index} conflicts with another one.");
			}
			return guid;
		}

		private List<Utf8String> ParseNameBuffer(byte[] buffer)
		{
			List<Utf8String> names = new();
			int offset = 0;
			while (buffer[offset] != 0)
			{
				int start = offset;
				while (buffer[++offset] != 0) { }

				byte[] utf8Data = buffer.SubArray(start, offset - start);
				names.Add(new Utf8String { Data = utf8Data });

				offset++;
			}

			return names;
		}

		private uint m_nextExportID;
		
		protected override long GenerateExportID(IUnityObjectBase asset)
		{
			long exportID = ExportIdHandler.GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}
		
		protected override string GetExportExtension(IUnityObjectBase asset) => "mixer";
		
		private bool HasAnyEffectParameterNameToRecover { get; set; }
		
		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			if (HasAnyEffectParameterNameToRecover)
			{
				UnityPatchUtils.ApplyPatchFromManifestResource(typeof(AudioMixerExporter).Assembly, UnityPatchName, dirPath);
			}
			return base.ExportInner(container, filePath, dirPath);
		}
		
		private const string UnityPatchName = "AssetRipper.Library.Exporters.Audio.UnityPatch.AudioMixerPostprocessor.txt";
	}
}
