using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class YamlStreamedAssetExportCollection : AssetExportCollection<IUnityObjectBase>
	{
		public YamlStreamedAssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportInner(IExportContainer container, string filePath, string dirPath)
		{
			return Asset switch
			{
				IMesh mesh => ExportMesh(container, filePath, dirPath, mesh),
				IImageTexture texture => ExportTexture(container, filePath, dirPath, texture),
				_ => false,
			};
		}

		private bool ExportMesh(IExportContainer container, string filePath, string dirPath, IMesh mesh)
		{
			bool result;
			if (mesh.Has_StreamData())
			{
				ulong offset = mesh.StreamData.GetOffset();
				Utf8String path = mesh.StreamData.Path;
				uint size = mesh.StreamData.Size;
				if (mesh.VertexData is not null && mesh.VertexData.Data.Length == 0 && mesh.StreamData.IsSet())
				{
					mesh.VertexData.Data = mesh.StreamData.GetContent(mesh.Collection);
					mesh.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					mesh.VertexData.Data = Array.Empty<byte>();
				}
				else
				{
					mesh.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				mesh.StreamData.SetOffset(offset);
				mesh.StreamData.Path = path;
				mesh.StreamData.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}

		private bool ExportTexture(IExportContainer container, string filePath, string dirPath, IImageTexture texture)
		{
			bool result;
			if (texture.Has_StreamData_C189())
			{
				ulong offset = texture.StreamData_C189.GetOffset();
				Utf8String path = texture.StreamData_C189.Path;
				uint size = texture.StreamData_C189.Size;
				if (texture.ImageData_C189.Length == 0 && texture.StreamData_C189.IsSet())
				{
					texture.ImageData_C189 = texture.StreamData_C189.GetContent(texture.Collection);
					texture.StreamData_C189.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					texture.ImageData_C189 = Array.Empty<byte>();
				}
				else
				{
					texture.StreamData_C189.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				texture.StreamData_C189.SetOffset(offset);
				texture.StreamData_C189.Path = path;
				texture.StreamData_C189.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}
	}
}
