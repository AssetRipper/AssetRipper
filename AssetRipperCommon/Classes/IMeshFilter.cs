using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Misc;

namespace AssetRipper.Core.Classes
{
	public interface IMeshFilter : IComponent
	{
		PPtr<IMesh> Mesh { get; }
	}
}
