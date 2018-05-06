using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationCurveTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public AnimationCurveTpl(bool _)
		{
			Curve = new List<KeyframeTpl<T>>();
			PreInfinity = 2;
			PostInfinity = 2;
			RotationOrder = 4;
		}

		public AnimationCurveTpl(T defaultValue, T defaultWeight):
			this(true)
		{
			KeyframeTpl<T> firstKey = new KeyframeTpl<T>(0.0f, defaultValue, defaultWeight);
			KeyframeTpl<T> secondKey = new KeyframeTpl<T>(1.0f, defaultValue, defaultWeight);
			Curve.Add(firstKey);
			Curve.Add(secondKey);
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadRotationOrder(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2, 1))
			{
				return 2;
			}
			return 1;
		}

		private int GetExportRotationOrder(Version version)
		{
			return IsReadRotationOrder(version) ? RotationOrder : 4;
		}

		public void Read(AssetStream stream)
		{
			Curve = new List<KeyframeTpl<T>>();
			Curve.Read(stream);
			stream.AlignStream(AlignType.Align4);

			PreInfinity = stream.ReadInt32();
			PostInfinity = stream.ReadInt32();
			if (IsReadRotationOrder(stream.Version))
			{
				RotationOrder = stream.ReadInt32();
			}
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Curve", (Curve == null) ? YAMLSequenceNode.Empty : Curve.ExportYAML(container));
			node.Add("m_PreInfinity", PreInfinity);
			node.Add("m_PostInfinity", PostInfinity);
			node.Add("m_RotationOrder", GetExportRotationOrder(container.Version));

			return node;
		}

		public List<KeyframeTpl<T>> Curve { get; private set; }
		public int PreInfinity { get; private set; }
		public int PostInfinity { get; private set; }
		public int RotationOrder { get; private set; }
	}
}
