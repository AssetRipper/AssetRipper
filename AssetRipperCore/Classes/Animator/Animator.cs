using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Animator
{
	public sealed class Animator : Behaviour
	{
		public Animator(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasUpdateMode(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool HasAnimatePhisics(UnityVersion version) => version.IsLess(4, 5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasHasTransformHierarchy(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLinearVelocityBlending(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool HasWarningMessage(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 4.5.3 and greater
		/// </summary>
		public static bool HasAllowConstantOptimization(UnityVersion version) => version.IsGreaterEqual(4, 5, 3);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasKeepAnimatorControllerStateOnDisable(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasStabilizeFeet(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignMiddle(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlignEnd(UnityVersion version) => version.IsGreaterEqual(5);

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

			if (HasStabilizeFeet(reader.Version))
			{
				StabilizeFeet = reader.ReadBoolean();
			}
			if (IsAlignMiddle(reader.Version))
			{
				reader.AlignStream();
			}

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Avatar, AvatarName);
			yield return context.FetchDependency(Controller, ControllerName);
		}

		public IReadOnlyDictionary<uint, string> BuildTOS()
		{
			if (HasHasTransformHierarchy(SerializedFile.Version))
			{
				if (HasTransformHierarchy)
				{
					GameObject.GameObject go = GameObject.GetAsset(SerializedFile);
					return go.BuildTOS();
				}
				else
				{
					return new Dictionary<uint, string>() { { 0, string.Empty } };
				}
			}
			else
			{
				GameObject.GameObject go = GameObject.GetAsset(SerializedFile);
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
			if (HasStabilizeFeet(container.ExportVersion))
			{
				node.Add(StabilizeFeetName, StabilizeFeet);
			}
			if (HasWarningMessage(container.ExportVersion, container.ExportFlags))
			{
				node.Add(WarningMessageName, "");
			}
			node.Add(HasTransformHierarchyName, HasTransformHierarchy);
			node.Add(AllowConstantClipSamplingOptimizationName, AllowConstantClipSamplingOptimization);
			if (HasKeepAnimatorControllerStateOnDisable(container.ExportVersion))
			{
				node.Add(KeepAnimatorControllerStateOnDisableName, KeepAnimatorControllerStateOnDisable);
			}
			return node;
		}

		public AnimatorCullingMode CullingMode { get; set; }
		public AnimatorUpdateMode UpdateMode { get; set; }
		public bool ApplyRootMotion { get; set; }
		public bool AnimatePhisics { get; set; }
		public bool LinearVelocityBlending { get; set; }
		public bool StabilizeFeet { get; set; }
		public bool HasTransformHierarchy { get; set; }
		public bool AllowConstantClipSamplingOptimization { get; set; }
		public bool KeepAnimatorControllerStateOnDisable { get; set; }

		public const string AvatarName = "m_Avatar";
		public const string ControllerName = "m_Controller";
		public const string CullingModeName = "m_CullingMode";
		public const string UpdateModeName = "m_UpdateMode";
		public const string ApplyRootMotionName = "m_ApplyRootMotion";
		public const string LinearVelocityBlendingName = "m_LinearVelocityBlending";
		public const string StabilizeFeetName = "m_StabilizeFeet";
		public const string WarningMessageName = "m_WarningMessage";
		public const string HasTransformHierarchyName = "m_HasTransformHierarchy";
		public const string AllowConstantClipSamplingOptimizationName = "m_AllowConstantClipSamplingOptimization";
		public const string KeepAnimatorControllerStateOnDisableName = "m_KeepAnimatorControllerStateOnDisable";

		public PPtr<Avatar.Avatar> Avatar = new();
		public PPtr<RuntimeAnimatorController> Controller = new();
	}
}
