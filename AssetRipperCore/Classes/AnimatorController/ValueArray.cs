using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;


namespace AssetRipper.Core.Classes.AnimatorController
{
	public sealed class ValueArray : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.3
		/// </summary>
		public static bool HasVectorValues(UnityVersion version) => version.IsLess(4, 3);
		/// <summary>
		/// Less than 5.4.0
		/// </summary>
		public static bool IsVector4f(UnityVersion version) => version.IsLess(5, 4);

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsPrimeFirst(UnityVersion version) => version.IsLess(5, 5);

		public void Read(AssetReader reader)
		{
			if (IsPrimeFirst(reader.Version))
			{
				BoolValues = reader.ReadBooleanArray();
				reader.AlignStream();

				IntValues = reader.ReadInt32Array();
				FloatValues = reader.ReadSingleArray();
			}

			if (HasVectorValues(reader.Version))
			{
				VectorValues = reader.ReadAssetArray<Vector4f>();
			}
			else
			{
				if (IsVector4f(reader.Version))
				{
					Position4Values = reader.ReadAssetArray<Vector4f>();
				}
				else
				{
					Position3Values = reader.ReadAssetArray<Vector3f>();
				}
				QuaternionValues = reader.ReadAssetArray<Quaternionf>();
				if (IsVector4f(reader.Version))
				{
					Scale4Values = reader.ReadAssetArray<Vector4f>();
				}
				else
				{
					Scale3Values = reader.ReadAssetArray<Vector3f>();
				}
			}

			if (!IsPrimeFirst(reader.Version))
			{
				FloatValues = reader.ReadSingleArray();
				IntValues = reader.ReadInt32Array();
				BoolValues = reader.ReadBooleanArray();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public Vector4f[] VectorValues { get; set; }
		public Vector3f[] Position3Values { get; set; }
		public Vector4f[] Position4Values { get; set; }
		public Quaternionf[] QuaternionValues { get; set; }
		public Vector3f[] Scale3Values { get; set; }
		public Vector4f[] Scale4Values { get; set; }
		public float[] FloatValues { get; set; }
		public int[] IntValues { get; set; }
		public bool[] BoolValues { get; set; }
	}
}
