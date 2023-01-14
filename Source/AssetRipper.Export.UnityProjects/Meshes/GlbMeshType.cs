using SharpGLTF.Geometry.VertexTypes;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	[Flags]
	public enum GlbMeshType
	{
		/// <summary>
		/// <see cref="VertexEmpty"/>
		/// </summary>
		Empty = 0,
		/// <summary>
		/// <see cref="VertexPosition"/>
		/// </summary>
		Position = 0,
		Normal = 1,
		Tangent = 2,
		/// <summary>
		/// <see cref="VertexPositionNormal"/>
		/// </summary>
		PositionNormal = Position | Normal,
		/// <summary>
		/// <see cref="VertexPositionNormalTangent"/>
		/// </summary>
		PositionNormalTangent = Position | Normal | Tangent,
		/// <summary>
		/// <see cref="VertexColor1"/>
		/// </summary>
		Color1 = 4,
		/// <summary>
		/// <see cref="VertexColor2"/>
		/// </summary>
		Color2 = 8,
		/// <summary>
		/// <see cref="VertexTexture1"/>
		/// </summary>
		Texture1 = 16,
		/// <summary>
		/// <see cref="VertexTexture2"/>
		/// </summary>
		Texture2 = 32,
		/// <summary>
		/// <see cref="VertexColor1Texture1"/>
		/// </summary>
		Color1Texture1 = Color1 | Texture1,
		/// <summary>
		/// <see cref="VertexColor1Texture2"/>
		/// </summary>
		Color1Texture2 = Color1 | Texture2,
		/// <summary>
		/// <see cref="VertexColor2Texture1"/>
		/// </summary>
		Color2Texture1 = Color2 | Texture1,
		/// <summary>
		/// <see cref="VertexColor2Texture2"/>
		/// </summary>
		Color2Texture2 = Color2 | Texture2,
		/// <summary>
		/// <see cref="VertexJoints4"/>
		/// </summary>
		Joints4 = 64,
		/// <summary>
		/// <see cref="VertexJoints8"/>
		/// </summary>
		Joints8 = 128,
	}
}
