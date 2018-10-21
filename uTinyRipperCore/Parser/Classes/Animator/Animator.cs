using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Animators;
using uTinyRipper.Exporter.YAML;
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
		/// 4.5.3 and greater
		/// </summary>
		public static bool IsReadAllowConstantOptimization(Version version)
		{
			return version.IsGreaterEqual(4, 5, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsAlignMiddle(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsAlignEnd(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

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
			
			if(IsReadHasTransformHierarchy(reader.Version))
			{
				HasTransformHierarchy = reader.ReadBoolean();
			}
			if (IsReadAllowConstantOptimization(reader.Version))
			{
				AllowConstantClipSamplingOptimization = reader.ReadBoolean();
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
			
			yield return Avatar.FetchDependency(file, isLog, ToLogString, "m_Avatar");
			yield return Controller.FetchDependency(file, isLog, ToLogString, "m_Controller");
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
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Avatar", Avatar.ExportYAML(container));
			node.Add("m_Controller", Controller.ExportYAML(container));
			node.Add("m_CullingMode", (int)CullingMode);
			node.Add("m_UpdateMode", (int)UpdateMode);
			node.Add("m_ApplyRootMotion", ApplyRootMotion);
			node.Add("m_LinearVelocityBlending", LinearVelocityBlending);
			node.Add("m_HasTransformHierarchy", HasTransformHierarchy);
			node.Add("m_AllowConstantClipSamplingOptimization", AllowConstantClipSamplingOptimization);
			return node;
		}

		public AnimatorCullingMode CullingMode { get; private set; }
		public AnimatorUpdateMode UpdateMode { get; private set; }
		public bool ApplyRootMotion { get; private set; }
		public bool AnimatePhisics { get; private set; }
		public bool LinearVelocityBlending { get; private set; }
		public bool HasTransformHierarchy { get; private set; }
		public bool AllowConstantClipSamplingOptimization { get; private set; }
		
		public PPtr<Avatar> Avatar;
		public PPtr<RuntimeAnimatorController> Controller;
	}
}
