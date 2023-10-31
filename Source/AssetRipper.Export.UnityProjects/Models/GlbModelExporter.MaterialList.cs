using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_23;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Material;

namespace AssetRipper.Export.UnityProjects.Models
{
	public partial class GlbModelExporter
	{
		private readonly struct MaterialList
		{
			private readonly AccessListBase<IPPtr_Material> materials;
			private readonly AssetCollection file;

			private MaterialList(AccessListBase<IPPtr_Material> materials, AssetCollection file)
			{
				this.materials = materials;
				this.file = file;
			}

			public MaterialList(IMeshRenderer renderer) : this(renderer.Materials, renderer.Collection) { }

			public MaterialList(ISkinnedMeshRenderer renderer) : this(renderer.Materials, renderer.Collection) { }

			public int Count => materials.Count;

			public IMaterial? this[int index]
			{
				get
				{
					if (index >= materials.Count)
					{
						return null;
					}
					return materials[index].TryGetAsset(file);
				}
			}
		}
	}
}
