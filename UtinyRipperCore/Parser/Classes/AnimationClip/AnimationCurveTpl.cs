using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationCurveTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public AnimationCurveTpl(int preInfinity, int postInfinity, int rotationOrder)
		{
			Curve = new List<KeyframeTpl<T>>();
			PreInfinity = preInfinity;
			PostInfinity = postInfinity;
			RotationOrder = rotationOrder;
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
		
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Curve", (Curve == null) ? YAMLSequenceNode.Empty : Curve.ExportYAML(exporter));
			node.Add("m_PreInfinity", PreInfinity);
			node.Add("m_PostInfinity", PostInfinity);
			node.Add("m_RotationOrder", IsReadRotationOrder(exporter.Version) ? RotationOrder : 0);

			return node;
		}

		public List<KeyframeTpl<T>> Curve { get; private set; }
		public int PreInfinity { get; private set; }
		public int PostInfinity { get; private set; }
		public int RotationOrder { get; private set; }
	}
}
