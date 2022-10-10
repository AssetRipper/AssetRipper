using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_23;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Material_;

namespace AssetRipper.Library.Exporters.Models
{
	public partial class GlbModelExporter
	{
		private readonly struct MaterialList
		{
			private readonly AccessListBase<IPPtr_Material_> materials;
			private readonly AssetCollection file;

			private MaterialList(AccessListBase<IPPtr_Material_> materials, AssetCollection file)
			{
				this.materials = materials;
				this.file = file;
			}

			public MaterialList(IMeshRenderer renderer) : this(renderer.Materials_C23, renderer.Collection) { }

			public MaterialList(ISkinnedMeshRenderer renderer) : this(renderer.Materials_C137, renderer.Collection) { }

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
