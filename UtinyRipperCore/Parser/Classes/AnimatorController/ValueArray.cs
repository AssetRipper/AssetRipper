using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct ValueArray : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.3
		/// </summary>
		public static bool IsReadVectorValues(Version version)
		{
			return version.IsLess(4, 3);
		}
		/// <summary>
		/// Less than 5.4.0
		/// </summary>
		public static bool IsVector4(Version version)
		{
			return version.IsLess(5, 4);
		}

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsPrimeFirst(Version version)
		{
			return version.IsLess(5, 5);
		}

		public void Read(AssetStream stream)
		{
			if(IsPrimeFirst(stream.Version))
			{
				m_boolValues = stream.ReadBooleanArray();
				stream.AlignStream(AlignType.Align4);

				m_intValues = stream.ReadInt32Array();
				m_floatValues = stream.ReadSingleArray();
			}

			if (IsReadVectorValues(stream.Version))
			{
				m_vectorValues = stream.ReadArray<Vector4f>();
			}
			else
			{
				if(IsVector4(stream.Version))
				{
					m_position4Values = stream.ReadArray<Vector4f>();
				}
				else
				{
					m_position3Values = stream.ReadArray<Vector3f>();
				}
				m_quaternionValues = stream.ReadArray<Quaternionf>();
				if (IsVector4(stream.Version))
				{
					m_scale4Values = stream.ReadArray<Vector4f>();
				}
				else
				{
					m_scale3Values = stream.ReadArray<Vector3f>();
				}
			}

			if (!IsPrimeFirst(stream.Version))
			{
				m_floatValues = stream.ReadSingleArray();
				m_intValues = stream.ReadInt32Array();
				m_boolValues = stream.ReadBooleanArray();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<Vector4f> VectorValues => m_vectorValues;
		public IReadOnlyList<Vector3f> Position3Values => m_position3Values;
		public IReadOnlyList<Vector4f> Position4Values => m_position4Values;
		public IReadOnlyList<Quaternionf> QuaternionValues => m_quaternionValues;
		public IReadOnlyList<Vector3f> Scale3Values => m_scale3Values;
		public IReadOnlyList<Vector4f> Scale4Values => m_scale4Values;
		public IReadOnlyList<float> FloatValues => m_floatValues;
		public IReadOnlyList<int> IntValues => m_intValues;
		public IReadOnlyList<bool> BoolValues => m_boolValues;

		private Vector4f[] m_vectorValues;
		private Vector3f[] m_position3Values;
		private Vector4f[] m_position4Values;
		private Quaternionf[] m_quaternionValues;
		private Vector3f[] m_scale3Values;
		private Vector4f[] m_scale4Values;
		private float[] m_floatValues;
		private int[] m_intValues;
		private bool[] m_boolValues;
	}
}
