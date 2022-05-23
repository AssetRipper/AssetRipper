using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_89;

namespace AssetRipper.Library.Exporters
{
	public sealed class YamlStreamedAssetExportCollection : AssetExportCollection
	{
		public YamlStreamedAssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			ProcessAsset(Asset);
			return AssetExporter.Export(container, Asset, filePath);
		}

		private void ProcessAsset(IUnityObjectBase asset)
		{
			//Possible improvement:
			//
			//The code for all these is very similar.
			//An interface could be added to them during source generation in order to avoid this switch statement.

			switch (asset)
			{
				case IMesh mesh:
					ProcessMesh(mesh);
					return;
				case ITexture2D texture2D:
					ProcessTexture2D(texture2D);
					return;
				case ITexture3D texture3D:
					ProcessTexture3D(texture3D);
					return;
				case ITexture2DArray texture2DArray:
					ProcessTexture2DArray(texture2DArray);
					return;
				case ICubemapArray cubemapArray:
					ProcessCubemapArray(cubemapArray);
					return;
			}
		}

		private void ProcessMesh(IMesh mesh)
		{
			if (mesh.Has_StreamData_C43())
			{
				if (mesh.VertexData_C43 is not null && mesh.VertexData_C43.Data.Length == 0 && mesh.StreamData_C43.IsSet())
				{
					mesh.VertexData_C43.Data = mesh.StreamData_C43.GetContent(File);
				}
				mesh.StreamData_C43.ClearValues();
			}
		}

		/// <summary>
		/// Also handles <see cref="ICubemap"/> by inheritance
		/// </summary>
		private void ProcessTexture2D(ITexture2D texture)
		{
			if (texture.Has_StreamData_C28())
			{
				if (texture.ImageData_C28.Length == 0 && texture.StreamData_C28.IsSet())
				{
					texture.ImageData_C28 = texture.StreamData_C28.GetContent(File);
				}
				texture.StreamData_C28.ClearValues();
			}
		}

		private void ProcessTexture3D(ITexture3D texture)
		{
			if (texture.Has_StreamData_C117())
			{
				if (texture.ImageData_C117.Length == 0 && texture.StreamData_C117.IsSet())
				{
					texture.ImageData_C117 = texture.StreamData_C117.GetContent(File);
				}
				texture.StreamData_C117.ClearValues();
			}
		}

		private void ProcessTexture2DArray(ITexture2DArray texture)
		{
			if (texture.Has_StreamData_C187())
			{
				if (texture.ImageData_C187.Length == 0 && texture.StreamData_C187.IsSet())
				{
					texture.ImageData_C187 = texture.StreamData_C187.GetContent(File);
				}
				texture.StreamData_C187.ClearValues();
			}
		}

		private void ProcessCubemapArray(ICubemapArray texture)
		{
			if (texture.Has_StreamData_C188())
			{
				if (texture.ImageData_C188.Length == 0 && texture.StreamData_C188.IsSet())
				{
					texture.ImageData_C188 = texture.StreamData_C188.GetContent(File);
				}
				texture.StreamData_C188.ClearValues();
			}
		}
	}
}
