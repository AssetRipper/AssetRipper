using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public struct ConstantBuffer : IAssetReadable
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasStructParams(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		/// <summary>
		/// 2020.3.0f2 to 2020.3.x<br/>
		/// 2021.1.4 and greater
		/// </summary>
		public static bool HasIsPartialCB(UnityVersion version)
		{
			if (version.Major == 2020 && version.IsGreaterEqual(2020, 3, 0, UnityVersionType.Final, 2))
				return true;
			else if (version.IsGreaterEqual(2021, 1, 4))
				return true;
			else
				return false;
		}

		public ConstantBuffer(string name, MatrixParameter[] matrices, VectorParameter[] vectors, StructParameter[] structs, int usedSize)
		{
			Name = name;
			NameIndex = -1;
			MatrixParams = matrices;
			VectorParams = vectors;
			StructParams = structs;
			Size = usedSize;
			IsPartialCB = false;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			MatrixParams = reader.ReadAssetArray<MatrixParameter>();
			VectorParams = reader.ReadAssetArray<VectorParameter>();
			if (HasStructParams(reader.Version))
			{
				StructParams = reader.ReadAssetArray<StructParameter>();
			}
			Size = reader.ReadInt32();
			if (HasIsPartialCB(reader.Version))
			{
				IsPartialCB = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public VectorParameter[] VectorParams { get; set; }
		public StructParameter[] StructParams { get; set; }
		public int Size { get; set; }
		public bool IsPartialCB { get; set; }
	}
}
