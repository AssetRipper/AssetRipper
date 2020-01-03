using System.Collections.Generic;
using uTinyRipper.Classes.Animators;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class Animator : Behaviour
	{
		public Animator(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(4, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasUpdateMode(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool HasAnimatePhisics(Version version) => version.IsLess(4, 5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasHasTransformHierarchy(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLinearVelocityBlending(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool HasWarningMessage(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 4.5.3 and greater
		/// </summary>
		public static bool HasAllowConstantOptimization(Version version) => version.IsGreaterEqual(4, 5, 3);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasKeepAnimatorControllerStateOnDisable(Version version) => version.IsGreaterEqual(2018);

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignMiddle(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlignEnd(Version version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Avatar.Read(reader);
			Controller.Read(reader);
			CullingMode = (AnimatorCullingMode)reader.ReadInt32();

			if (HasUpdateMode(reader.Version))
			{
				UpdateMode = (AnimatorUpdateMode)reader.ReadInt32();
			}

			ApplyRootMotion = reader.ReadBoolean();

			if (HasAnimatePhisics(reader.Version))
			{
				AnimatePhisics = reader.ReadBoolean();
			}
			if (HasLinearVelocityBlending(reader.Version))
			{
				LinearVelocityBlending = reader.ReadBoolean();
			}
			if (IsAlignMiddle(reader.Version))
			{
				reader.AlignStream();
			}

#if UNIVERSAL
			if (HasWarningMessage(reader.Version, reader.Flags))
			{
				WarningMessage = reader.ReadString();
			}
#endif

			if (HasHasTransformHierarchy(reader.Version))
			{
				HasTransformHierarchy = reader.ReadBoolean();
			}
			if (HasAllowConstantOptimization(reader.Version))
			{
				AllowConstantClipSamplingOptimization = reader.ReadBoolean();
			}
			if (HasKeepAnimatorControllerStateOnDisable(reader.Version))
			{
				KeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
			}
			if (IsAlignEnd(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(Avatar, AvatarName);
			yield return context.FetchDependency(Controller, ControllerName);
		}

		public IReadOnlyDictionary<uint, string> BuildTOS()
		{
			if (HasHasTransformHierarchy(File.Version))
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
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AvatarName, Avatar.ExportYAML(container));
			node.Add(ControllerName, Controller.ExportYAML(container));
			node.Add(CullingModeName, (int)CullingMode);
			node.Add(UpdateModeName, (int)UpdateMode);
			node.Add(ApplyRootMotionName, ApplyRootMotion);
			node.Add(LinearVelocityBlendingName, LinearVelocityBlending);
			if (HasWarningMessage(container.ExportVersion, container.ExportFlags))
			{
				node.Add(WarningMessageName, GetWarningMessage(container.Version, container.Flags));
			}
			node.Add(HasTransformHierarchyName, HasTransformHierarchy);
			node.Add(AllowConstantClipSamplingOptimizationName, AllowConstantClipSamplingOptimization);
			if (HasKeepAnimatorControllerStateOnDisable(container.ExportVersion))
			{
				node.Add(KeepAnimatorControllerStateOnDisableName, KeepAnimatorControllerStateOnDisable);
			}
			return node;
		}

		private string GetWarningMessage(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasWarningMessage(version, flags))
			{
				return WarningMessage;
			}
#endif
			return string.Empty;
		}

		public AnimatorCullingMode CullingMode { get; set; }
		public AnimatorUpdateMode UpdateMode { get; set; }
		public bool ApplyRootMotion { get; set; }
		public bool AnimatePhisics { get; set; }
		public bool LinearVelocityBlending { get; set; }
#if UNIVERSAL
		public string WarningMessage { get; set; }
#endif
		public bool HasTransformHierarchy { get; set; }
		public bool AllowConstantClipSamplingOptimization { get; set; }
		public bool KeepAnimatorControllerStateOnDisable { get; set; }

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
