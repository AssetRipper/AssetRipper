using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Animators;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Animator : Behaviour
	{
		public Animator(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadUpdateMode(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool IsReadAnimatePhisics(Version version)
		{
			return version.IsLess(4, 5);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadHasTransformHierarchy(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLinearVelocityBlending(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool IsReadWarningMessage(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 4.5.3 and greater
		/// </summary>
		public static bool IsReadAllowConstantOptimization(Version version)
		{
			return version.IsGreaterEqual(4, 5, 3);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadKeepAnimatorControllerStateOnDisable(Version version)
		{
			return version.IsGreaterEqual(2018);
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignMiddle(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlignEnd(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(4, 5))
			{
				return 3;
			}
			if(version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Avatar.Read(reader);
			Controller.Read(reader);
			CullingMode = (AnimatorCullingMode)reader.ReadInt32();

			if(IsReadUpdateMode(reader.Version))
			{
				UpdateMode = (AnimatorUpdateMode)reader.ReadInt32();
			}

			ApplyRootMotion = reader.ReadBoolean();

			if(IsReadAnimatePhisics(reader.Version))
			{
				AnimatePhisics = reader.ReadBoolean();
			}
			if (IsReadLinearVelocityBlending(reader.Version))
			{
				LinearVelocityBlending = reader.ReadBoolean();
			}
			if (IsAlignMiddle(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

#if UNIVERSAL
			if (IsReadWarningMessage(reader.Version, reader.Flags))
			{
				WarningMessage = reader.ReadString();
			}
#endif

			if(IsReadHasTransformHierarchy(reader.Version))
			{
				HasTransformHierarchy = reader.ReadBoolean();
			}
			if (IsReadAllowConstantOptimization(reader.Version))
			{
				AllowConstantClipSamplingOptimization = reader.ReadBoolean();
			}
			if (IsReadKeepAnimatorControllerStateOnDisable(reader.Version))
			{
				KeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
			}
			if (IsAlignEnd(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Avatar.FetchDependency(file, isLog, ToLogString, AvatarName);
			yield return Controller.FetchDependency(file, isLog, ToLogString, ControllerName);
		}

		public IReadOnlyDictionary<uint, string> RetrieveTOS()
		{
			Avatar avatar = Avatar.FindAsset(File);
			if (avatar == null)
			{
				return BuildTOS();
			}
			else
			{
				return avatar.TOS;
			}
		}

		public IReadOnlyDictionary<uint, string> BuildTOS()
		{
			if (IsReadHasTransformHierarchy(File.Version))
			{
				if (HasTransformHierarchy)
				{
					GameObject go = GameObject.GetAsset(File);
					return go.BuildTOS();
				}
				else
				{
					return new Dictionary<uint, string>() { { 0, string.Empty } };
				}
			}
			else
			{
				GameObject go = GameObject.GetAsset(File);
				return go.BuildTOS();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(AvatarName, Avatar.ExportYAML(container));
			node.Add(ControllerName, Controller.ExportYAML(container));
			node.Add(CullingModeName, (int)CullingMode);
			node.Add(UpdateModeName, (int)UpdateMode);
			node.Add(ApplyRootMotionName, ApplyRootMotion);
			node.Add(LinearVelocityBlendingName, LinearVelocityBlending);
			if (IsReadWarningMessage(container.ExportVersion, container.ExportFlags))
			{
				node.Add(WarningMessageName, GetWarningMessage(container.Version, container.Flags));
			}
			node.Add(HasTransformHierarchyName, HasTransformHierarchy);
			node.Add(AllowConstantClipSamplingOptimizationName, AllowConstantClipSamplingOptimization);
			if (IsReadKeepAnimatorControllerStateOnDisable(container.ExportVersion))
			{
				node.Add(KeepAnimatorControllerStateOnDisableName, KeepAnimatorControllerStateOnDisable);
			}
			return node;
		}

		private string GetWarningMessage(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadWarningMessage(version, flags))
			{
				return WarningMessage;
			}
#endif
			return string.Empty;
		}

		public AnimatorCullingMode CullingMode { get; private set; }
		public AnimatorUpdateMode UpdateMode { get; private set; }
		public bool ApplyRootMotion { get; private set; }
		public bool AnimatePhisics { get; private set; }
		public bool LinearVelocityBlending { get; private set; }
#if UNIVERSAL
		public string WarningMessage { get; private set; }
#endif
		public bool HasTransformHierarchy { get; private set; }
		public bool AllowConstantClipSamplingOptimization { get; private set; }
		public bool KeepAnimatorControllerStateOnDisable { get; private set; }

		public const string AvatarName = "m_Avatar";
		public const string ControllerName = "m_Controller";
		public const string CullingModeName = "m_CullingMode";
		public const string UpdateModeName = "m_UpdateMode";
		public const string ApplyRootMotionName = "m_ApplyRootMotion";
		public const string LinearVelocityBlendingName = "m_LinearVelocityBlending";
		public const string WarningMessageName = "m_WarningMessage";
		public const string HasTransformHierarchyName = "m_HasTransformHierarchy";
		public const string AllowConstantClipSamplingOptimizationName = "m_AllowConstantClipSamplingOptimization";
		public const string KeepAnimatorControllerStateOnDisableName = "m_KeepAnimatorControllerStateOnDisable";

		public PPtr<Avatar> Avatar;
		public PPtr<RuntimeAnimatorController> Controller;
	}
}
