using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.Modules.Models;

internal static class MeshDataExtensions
{
	public static GlbMeshType GetMeshType(this MeshData mesh)
	{
		GlbMeshType meshType = default;

		if (mesh.HasNormals)
		{
			if (mesh.HasTangents)
			{
				meshType |= GlbMeshType.PositionNormalTangent;
			}
			else
			{
				meshType |= GlbMeshType.PositionNormal;
			}
		}

		meshType |= mesh.UVCount switch
		{
			0 => default,
			1 => GlbMeshType.Texture1,
			2 => GlbMeshType.Texture2,
			_ => GlbMeshType.TextureN,
		};

		if (mesh.HasColors)
		{
			meshType |= GlbMeshType.Color1;
		}

		if (mesh.HasSkin)
		{
			meshType |= GlbMeshType.Joints4;
		}

		return meshType;
	}
}
