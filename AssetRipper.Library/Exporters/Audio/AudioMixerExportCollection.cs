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
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AudioMixerEffectController_;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using Cpp2IL.Core;
using System.Collections.Generic;
using System.Linq;
using Logger = AssetRipper.Core.Logging.Logger;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioMixerExportCollection : AssetsExportCollection
	{
		public AudioMixerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IAudioMixerController mixer) : base(assetExporter, mixer)
		{
			var constants = mixer.MixerConstant_C241;
			var indexToGUID = new Dictionary<uint, GUID>();

			// collect groups
			
			var groupGUIDMap = new Dictionary<GUID, IAudioMixerGroupController>();
			foreach (IAudioMixerGroupController group in mixer.SerializedFile.Collection.FetchAssetsOfType<IAudioMixerGroupController>())
			{
				if (group.AudioMixer_C243.IsAsset(mixer.SerializedFile, mixer))
				{
					AddAsset(group);
					groupGUIDMap.Add(group.GroupID_C243, group);
				}
			}
			
			var groups = mixer.MixerConstant_C241.GroupGUIDs.Select(guid => groupGUIDMap[guid]).ToArray();
			for (int i = 0; i < groups.Length; i++)
			{
				var group = groups[i];
				var groupConstant = constants.Groups[i];
				
				group.Volume_C243.CopyValues(IndexingNewGUID(groupConstant.VolumeIndex, indexToGUID));
				group.Pitch_C243.CopyValues(IndexingNewGUID(groupConstant.PitchIndex, indexToGUID));
				
				// Different Unity versions vary in whether a "send" field is used in groups as well as in snapshots.
				// GroupConstant.Has_SendIndex() can be used to determine its existence.
				// If "send" does not exist in GroupConstant, it may or may not exist in AudioMixerGroupController,
				// where in the latter case it exists but is just ignored.
				if (groupConstant.Has_SendIndex())
				{
					group.Send_C243.CopyValues(IndexingNewGUID(groupConstant.SendIndex, indexToGUID));
				}
				group.Mute_C243 = groupConstant.Mute;
				group.Solo_C243 = groupConstant.Solo;
				group.BypassEffects_C243 = groupConstant.BypassEffects;
			}
			
			// collect effects
			
			var effects = new (IAudioMixerEffectController, PPtr_AudioMixerEffectController_)[constants.Effects.Count];
			var groupsWithAttenuation = new HashSet<IAudioMixerGroupController>();
			for (int i = 0; i < effects.Length; i++)
			{
				var effect = virtualFile.CreateAsset<IAudioMixerEffectController>(ClassIDType.AudioMixerEffectController);
				effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				var effectPPtr = new PPtr_AudioMixerEffectController_();
				effectPPtr.CopyValues(effect.SerializedFile.CreatePPtr(effect));
				effects[i] = (effect, effectPPtr);
				AddAsset(effect);
			}
			
			var pluginEffectNames = ParseNameBuffer(constants.PluginEffectNameBuffer);
			int pluginEffectIndex = 0;
			for (int i = 0; i < constants.Effects.Count; i++)
			{
				var effectConstant = constants.Effects[i];
				var (effect, effectPPtr) = effects[i];

				var group = groups[effectConstant.GroupConstantIndex];
				group.Effects_C243.Add(effectPPtr);

				effect.EffectID_C244.CopyValues(constants.EffectGUIDs[i]);

				if (FMODDefinitions.IsPluginEffect(effectConstant.Type))
				{
					effect.EffectName_C244.CopyValues(pluginEffectNames[pluginEffectIndex++]);
				}
				else
				{
					var name = FMODDefinitions.EffectTypeToName(effectConstant.Type) ?? "Unknown";
					effect.EffectName_C244.String = name;
					if (name == "Attenuation")
					{
						groupsWithAttenuation.Add(group);
					}
				}

				bool enableWetMix = (int)effectConstant.WetMixLevelIndex != -1;
				if (enableWetMix || effect.EffectName_C244 == "Send")
				{
					effect.MixLevel_C244.CopyValues(IndexingNewGUID(effectConstant.WetMixLevelIndex, indexToGUID));
				}

				for (int j = 0; j < effectConstant.ParameterIndices.Length; j++)
				{
					var param = effect.Parameters_C244.AddNew();
					// Use a dummy name here. The actual name will be recovered by AssetRipperAudioMixerPostprocessor.
					param.ParameterName.String = $"Param_{j}";
					HasAnyEffectParameterNameToRecover = true;
					param.GUID.CopyValues(IndexingNewGUID(effectConstant.ParameterIndices[j], indexToGUID));
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
			foreach (var group in groups)
			{
				if (!groupsWithAttenuation.Contains(group))
				{
					var effect = virtualFile.CreateAsset<IAudioMixerEffectController>(ClassIDType.AudioMixerEffectController);
					effect.ObjectHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					var effectPPtr = new PPtr_AudioMixerEffectController_();
					effectPPtr.CopyValues(effect.SerializedFile.CreatePPtr(effect));
					group.Effects_C243.Add(effectPPtr);
					AddAsset(effect);
					
					effect.EffectID_C244.CopyValues((GUID)UnityGUID.NewGuid());
					effect.EffectName_C244.String = "Attenuation";
				}
			}

			// collect snapshots
			
			for (int i = 0; i < mixer.Snapshots_C241.Count; i++)
			{
				var snapshotPPtr = mixer.Snapshots_C241[i];
				var snapshot = snapshotPPtr.GetAsset(mixer.SerializedFile) as IAudioMixerSnapshotController;
				AddAsset(snapshot);
				
				var snapshotConstant = constants.Snapshots[i];
				for (int j = 0; j < snapshotConstant.Values.Length; j++)
				{
					if (indexToGUID.TryGetValue((uint)j, out var valueGUID))
					{
						snapshot.FloatValues_C245[valueGUID] = snapshotConstant.Values[j];
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
					if (indexToGUID.TryGetValue(paramIndex, out var paramGUID))
					{
						snapshot.TransitionOverrides_C245[paramGUID] = transitionType;
					}
					else
					{
						Logger.Warning(LogCategory.Export, $"Snapshot({snapshot.Name_C245}) transition #{paramIndex} has no binding parameter");
					}
				}
			}
			
			// generate exposed parameters

			for (int i = 0; i < constants.ExposedParameterIndices.Length; i++)
			{
				uint paramIndex = constants.ExposedParameterIndices[i];
				uint paramNameCrc = constants.ExposedParameterNames[i];
				if (indexToGUID.TryGetValue(paramIndex, out var paramGUID))
				{
					var exposedParam = mixer.ExposedParameters_C241.AddNew();
					exposedParam.Guid.CopyValues(paramGUID);
					exposedParam.NameString = $"{CrcUtils.ReverseDigestAscii(paramNameCrc)}";
				}
				else
				{
					Logger.Warning(LogCategory.Export, $"Exposed parameter #{paramIndex} has no binding parameter");
				}
			}
			
			// complete mixer controller
			
			var groupView = mixer.AudioMixerGroupViews_C241.AddNew();
			groupView.NameString = "View";
			foreach (var group in groups)
			{
				groupView.Guids.Add(group.GroupID_C243);
			}
			mixer.CurrentViewIndex_C241 = 0;
			mixer.TargetSnapshot_C241.CopyValues(mixer.StartSnapshot_C241);
		}

		private GUID IndexingNewGUID(uint index, Dictionary<uint, GUID> table)
		{
			var guid = (GUID)UnityGUID.NewGuid();
			if (!table.TryAdd(index, guid))
			{
				Logger.Warning(LogCategory.Export, $"Constant index #{index} conflicts with another one.");
			}
			return guid;
		}

		private List<Utf8String> ParseNameBuffer(byte[] buffer)
		{
			var names = new List<Utf8String>();
			int offset = 0;
			while (buffer[offset] != 0)
			{
				int start = offset;
				while (buffer[++offset] != 0) { }

				var utf8Data = buffer.SubArray(start, offset - start);
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
				// Put an editor script file in the exported project. See comments in the file for more detail.
				var postprocessorScriptPath = System.IO.Path.Combine(dirPath,
					"Assets/MonoScript/Editor/AssetRipperAudioMixerPostprocessor.cs");
				if (!System.IO.File.Exists(postprocessorScriptPath))
				{
					System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(postprocessorScriptPath));
					System.IO.File.WriteAllBytes(postprocessorScriptPath,
						Properties.Resources.AssetRipperAudioMixerPostprocessor);
				}
			}

			return base.ExportInner(container, filePath, dirPath);
		}
	}
}
