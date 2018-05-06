using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		/// 4.6.0 and greater
		/// </summary>
		public static bool IsReadAllowConstantOptimization(Version version)
		{
			return version.IsGreaterEqual(4, 6);
		}
		/// <summary>
		/// 4.6.0 and greater
		/// </summary>
		public static bool IsAlignMiddle(Version version)
		{
			return version.IsGreaterEqual(4, 6);
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Avatar.Read(stream);
			Controller.Read(stream);
			CullingMode = stream.ReadInt32();

			if(IsReadUpdateMode(stream.Version))
			{
				UpdateMode = stream.ReadInt32();
			}

			ApplyRootMotion = stream.ReadBoolean();

			if(IsReadAnimatePhisics(stream.Version))
			{
				AnimatePhisics = stream.ReadBoolean();
			}
			if (IsReadLinearVelocityBlending(stream.Version))
			{
				LinearVelocityBlending = stream.ReadBoolean();
			}
			if (IsAlignMiddle(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			if(IsReadHasTransformHierarchy(stream.Version))
			{
				HasTransformHierarchy = stream.ReadBoolean();
			}

			if (IsReadAllowConstantOptimization(stream.Version))
			{
				AllowConstantClipSamplingOptimization = stream.ReadBoolean();
			}
			if (IsAlignEnd(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return Avatar.FetchDependency(file, isLog, ToLogString, "m_Avatar");
			yield return Controller.FetchDependency(file, isLog, ToLogString, "m_Controller");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Avatar", Avatar.ExportYAML(container));
			node.Add("m_Controller", Controller.ExportYAML(container));
			node.Add("m_CullingMode", CullingMode);
			node.Add("m_UpdateMode", UpdateMode);
			node.Add("m_ApplyRootMotion", ApplyRootMotion);
			node.Add("m_LinearVelocityBlending", LinearVelocityBlending);
			node.Add("m_HasTransformHierarchy", HasTransformHierarchy);
			node.Add("m_AllowConstantClipSamplingOptimization", AllowConstantClipSamplingOptimization);
			return node;
		}

		public int CullingMode { get; private set; }
		public int UpdateMode { get; private set; }
		public bool ApplyRootMotion { get; private set; }
		public bool AnimatePhisics { get; private set; }
		public bool LinearVelocityBlending { get; private set; }
		public bool HasTransformHierarchy { get; private set; }
		public bool AllowConstantClipSamplingOptimization { get; private set; }
		
		public PPtr<Avatar> Avatar;
		public PPtr<RuntimeAnimatorController> Controller;
	}
}
