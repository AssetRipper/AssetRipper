using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IBlendShapeVertex : IAsset
	{
		uint Index { get; set; }
		IVector3f Normal { get; }
		IVector3f Tangent { get; }
		IVector3f Vertex { get; }
	}
}
