using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.Modules.Shaders.Extensions
{
	public static class ShaderExtensions
	{
		public static ShaderSubProgramBlob[] ReadBlobs(this IShader shader)
		{
			if (shader.Has_CompressedBlob_C48())
			{
				if (shader.Has_CompressedLengths_C48_AssetList_UInt32())
				{
					return UnpackSubProgramBlobs(
						shader.Collection,
						shader.Offsets_C48_AssetList_UInt32,
						shader.CompressedLengths_C48_AssetList_UInt32,
						shader.DecompressedLengths_C48_AssetList_UInt32,
						shader.CompressedBlob_C48);
				}
				else if (shader.Has_CompressedLengths_C48_AssetList_AssetList_UInt32())
				{
					return UnpackSubProgramBlobs(
						shader.Collection,
						shader.Offsets_C48_AssetList_AssetList_UInt32,
						shader.CompressedLengths_C48_AssetList_AssetList_UInt32,
						shader.DecompressedLengths_C48_AssetList_AssetList_UInt32,
						shader.CompressedBlob_C48);
				}
			}
			else if (shader.Has_SubProgramBlob_C48())//todo: rename to CompressedBlob
			{
				return UnpackSubProgramBlobs(
					shader.Collection,
					0,
					(uint)shader.SubProgramBlob_C48.Length,
					shader.DecompressedSize_C48,
					shader.SubProgramBlob_C48);
			}
			return Array.Empty<ShaderSubProgramBlob>();
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(AssetCollection shaderCollection, uint offset, uint compressedLength, uint decompressedLength, byte[] compressedBlob)
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
				blobs[0].Read(shaderCollection, compressedBlob, offsets, compressedLengths, decompressedLengths);
				return blobs;
			}
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(AssetCollection shaderCollection, AssetList<uint> offsets, AssetList<uint> compressedLengths, AssetList<uint> decompressedLengths, byte[] compressedBlob)
		{
			ShaderSubProgramBlob[] blobs = new ShaderSubProgramBlob[offsets.Count];
			for (int i = 0; i < blobs.Length; i++)
			{
				blobs[i] = new();
				uint[] blobOffsets = new uint[] { offsets[i] };
				uint[] blobCompressedLengths = new uint[] { compressedLengths[i] };
				uint[] blobDecompressedLengths = new uint[] { decompressedLengths[i] };
				blobs[i].Read(shaderCollection, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
			return blobs;
		}

		private static ShaderSubProgramBlob[] UnpackSubProgramBlobs(AssetCollection shaderCollection, AssetList<AssetList<uint>> offsets, AssetList<AssetList<uint>> compressedLengths, AssetList<AssetList<uint>> decompressedLengths, byte[] compressedBlob)
		{
			ShaderSubProgramBlob[] blobs = new ShaderSubProgramBlob[offsets.Count];
			for (int i = 0; i < blobs.Length; i++)
			{
				blobs[i] = new();
				AssetList<uint> blobOffsets = offsets[i];
				AssetList<uint> blobCompressedLengths = compressedLengths[i];
				AssetList<uint> blobDecompressedLengths = decompressedLengths[i];
				blobs[i].Read(shaderCollection, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
			return blobs;
		}
	}
}
