using AssetRipper.Core.Classes.ShaderBlob;
using AssetRipper.Core.Layout;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShaderExtensions
	{
		public static ShaderSubProgramBlob[] ReadBlobs(this IShader shader)
		{
			if (shader.Has_CompressedBlob_C48())
			{
				LayoutInfo layout = new LayoutInfo(shader.AssetUnityVersion, Parser.Files.BuildTarget.NoTarget, shader.TransferInstructionFlags);
				if (shader.Has_CompressedLengths_C48_UInt32_Array())
				{
					return UnpackSubProgramBlobs(
						layout,
						shader.Offsets_C48_UInt32_Array,
						shader.CompressedLengths_C48_UInt32_Array,
						shader.DecompressedLengths_C48_UInt32_Array,
						shader.CompressedBlob_C48);
				}
				else if (shader.Has_CompressedLengths_C48_UInt32_Array_Array())
				{
					return UnpackSubProgramBlobs(
						layout, 
						shader.Offsets_C48_UInt32_Array_Array, 
						shader.CompressedLengths_C48_UInt32_Array_Array, 
						shader.DecompressedLengths_C48_UInt32_Array_Array, 
						shader.CompressedBlob_C48);
				}
				else
				{
					return UnpackSubProgramBlobs(
						layout, 
						0, 
						(uint)shader.CompressedBlob_C48.Length, 
						shader.DecompressedSize_C48, 
						shader.CompressedBlob_C48);
				}
			}
			return Array.Empty<ShaderSubProgramBlob>();
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(LayoutInfo layout, uint offset, uint compressedLength, uint decompressedLength, byte[] compressedBlob)
		{
			if (compressedBlob.Length == 0)
			{
				return Array.Empty<ShaderSubProgramBlob>();
			}
			else
			{
				ShaderSubProgramBlob[] blobs = new ShaderSubProgramBlob[1] { new() };
				uint[] offsets = new uint[] { offset };
				uint[] compressedLengths = new uint[] { compressedLength };
				uint[] decompressedLengths = new uint[] { decompressedLength };
				blobs[0].Read(layout, compressedBlob, offsets, compressedLengths, decompressedLengths);
				return blobs;
			}
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(LayoutInfo layout, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths, byte[] compressedBlob)
		{
			ShaderSubProgramBlob[] blobs = new ShaderSubProgramBlob[offsets.Length];
			for (int i = 0; i < blobs.Length; i++)
			{
				blobs[i] = new();
				uint[] blobOffsets = new uint[] { offsets[i] };
				uint[] blobCompressedLengths = new uint[] { compressedLengths[i] };
				uint[] blobDecompressedLengths = new uint[] { decompressedLengths[i] };
				blobs[i].Read(layout, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
			return blobs;
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(LayoutInfo layout, uint[][] offsets, uint[][] compressedLengths, uint[][] decompressedLengths, byte[] compressedBlob)
		{
			ShaderSubProgramBlob[] blobs = new ShaderSubProgramBlob[offsets.Length];
			for (int i = 0; i < blobs.Length; i++)
			{
				blobs[i] = new();
				uint[] blobOffsets = offsets[i];
				uint[] blobCompressedLengths = compressedLengths[i];
				uint[] blobDecompressedLengths = decompressedLengths[i];
				blobs[i].Read(layout, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
			return blobs;
		}
	}
}
