using SharpGLTF.Geometry.VertexTypes;

namespace AssetRipper.Export.Modules.Models;

[Flags]
internal enum GlbMeshType
{
	/// <summary>
	/// <see cref="VertexEmpty"/>
	/// </summary>
	Empty = 0,
	/// <summary>
	/// <see cref="VertexPosition"/>
	/// </summary>
	Position = 0,
	Normal = 1 << 0,
	Tangent = 1 << 1,
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
	Color1 = 1 << 2,
	/// <summary>
	/// <see cref="VertexTexture1"/>
	/// </summary>
	Texture1 = 1 << 3,
	/// <summary>
	/// <see cref="VertexTexture2"/>
	/// </summary>
	Texture2 = 1 << 4,
	/// <summary>
	/// Not implemented yet. Defines a vertex with up to 8 UV channels.
	/// </summary>
	TextureN = 1 << 5,
	/// <summary>
	/// <see cref="VertexColor1Texture1"/>
	/// </summary>
	Color1Texture1 = Color1 | Texture1,
	/// <summary>
	/// <see cref="VertexColor1Texture2"/>
	/// </summary>
	Color1Texture2 = Color1 | Texture2,
	/// <summary>
	/// Not implemented yet.
	/// </summary>
	Color1TextureN = Color1 | TextureN,
	/// <summary>
	/// <see cref="VertexJoints4"/>
	/// </summary>
	Joints4 = 1 << 6,
}
