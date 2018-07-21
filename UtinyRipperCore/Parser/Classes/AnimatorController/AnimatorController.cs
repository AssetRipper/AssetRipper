using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimatorControllers;
using UtinyRipper.Classes.AnimatorControllers.Editor;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;
using IExportContainer = UtinyRipper.AssetExporters.IExportContainer;

namespace UtinyRipper.Classes
{
	public sealed class AnimatorController : RuntimeAnimatorController
	{
		public AnimatorController(AssetInfo assetsInfo):
			base(assetsInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadStateMachineBehaviourVectorDescription(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b2 to 5.1.x and 5.4.0 and greater
		/// </summary>
		public static bool IsReadMultiThreadedStateMachine(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1) && version.IsLess(5, 2) || version.IsGreaterEqual(5, 4);
		}

		/*/// <summary>
		/// Less than 5.1.0
		/// </summary>
		private static bool IsReadAlign(Version version)
		{
			return version.IsLess(5, 1);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlignMultiThreadedStateMachine(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}*/

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 5;
			}
			
#warning unknown
			if (version.IsGreater(5, 0, 0, VersionType.Beta))
			{
				return 5;
			}
			if (version.IsEqual(5, 0, 0, VersionType.Beta))
			{
				return 4;
			}
#warning unknown
			if (version.IsGreaterEqual(5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_TOS.Clear();

			ControllerSize = stream.ReadUInt32();
			Controller.Read(stream);
			TOS.Read(stream);
			m_animationClips = stream.ReadArray<PPtr<AnimationClip>>();

			if (IsReadStateMachineBehaviourVectorDescription(stream.Version))
			{
				StateMachineBehaviourVectorDescription.Read(stream);
				m_stateMachineBehaviours = stream.ReadArray<PPtr<MonoBehaviour>>();
			}

			if (IsReadMultiThreadedStateMachine(stream.Version))
			{
				MultiThreadedStateMachine = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (PPtr<AnimationClip> clip in AnimationClips)
			{
				yield return clip.FetchDependency(file, isLog, ToLogString, "AnimationClips");
			}
			if (IsReadStateMachineBehaviourVectorDescription(file.Version))
			{
				foreach (PPtr<MonoBehaviour> behaviour in StateMachineBehaviours)
				{
					yield return behaviour.FetchDependency(file, isLog, ToLogString, "StateMachineBehaviours");
				}
			}
		}

#warning TODO: exporter for animator controller
		public YAMLDocument FetchSubDocuments()
		{
#warning TODO: build submachines, animstates, etc from data

			throw new System.NotImplementedException();
		}

		public override bool IsContainsAnimationClip(AnimationClip clip)
		{
			foreach (PPtr<AnimationClip> clipPtr in AnimationClips)
			{
				if (clipPtr.IsObject(File, clip))
				{
					return true;
				}
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
#warning TODO: build controller from data
			AnimatorControllerParameter[] @params = null;
			AnimatorControllerLayers[] layers = null;

			IReadOnlyList<ValueConstant> values = Controller.Values.Instance.ValueArray;
			ValueArray defaultValues = Controller.DefaultValues.Instance;
			@params = new AnimatorControllerParameter[values.Count];
			for(int i = 0; i < values.Count; i++)
			{
				ValueConstant value = values[i];
				string name = TOS[value.ID];
#warning TODO:
				AnimatorControllerParameterType type = ValueConstant.IsReadType(container.Version) ? value.Type : (AnimatorControllerParameterType)value.TypeID;
				switch (type)
				{
					case AnimatorControllerParameterType.Trigger:
						@params[i] = new AnimatorControllerParameter(name, type, this);
						break;

					case AnimatorControllerParameterType.Bool:
						@params[i] = new AnimatorControllerParameter(name, type, this, defaultValues.BoolValues[value.Index]);
						break;

					case AnimatorControllerParameterType.Int:
						@params[i] = new AnimatorControllerParameter(name, type, this, defaultValues.IntValues[value.Index]);
						break;

					case AnimatorControllerParameterType.Float:
						@params[i] = new AnimatorControllerParameter(name, type, this, defaultValues.FloatValues[value.Index]);
						break;

					default:
						throw new NotSupportedException($"Parameter type '{type}' isn't supported");
				}
			}
			
			layers = new AnimatorControllerLayers[Controller.LayerArray.Count];
			for(int i = 0; i < Controller.LayerArray.Count; i++)
			{
				LayerConstant layerConstant = Controller.LayerArray[i].Instance;

			}

			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_AnimatorParameters", @params.ExportYAML(container));
			node.Add("m_AnimatorLayers", layers.ExportYAML(container));
			return node;
		}

		public uint ControllerSize { get; private set; }
		public IDictionary<uint, string> TOS => m_TOS;
		public IReadOnlyList<PPtr<AnimationClip>> AnimationClips => m_animationClips;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		public bool MultiThreadedStateMachine { get; private set; }

		public ControllerConstant Controller;
		public StateMachineBehaviourVectorDescription StateMachineBehaviourVectorDescription;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();

		private PPtr<AnimationClip>[] m_animationClips;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}