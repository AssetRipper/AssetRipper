using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
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
			//Possible improvement:
			//
			//The code for all these is very similar.
			//An interface could be added to them during source generation in order to avoid this switch expression.

			return Asset switch
			{
				IMesh mesh => ExportMesh(container, filePath, dirPath, mesh),
				ITexture2D texture2D => ExportTexture2D(container, filePath, dirPath, texture2D),
				ITexture3D texture3D => ExportTexture3D(container, filePath, dirPath, texture3D),
				ITexture2DArray texture2DArray => ExportTexture2DArray(container, filePath, dirPath, texture2DArray),
				ICubemapArray cubemapArray => ExportCubemapArray(container, filePath, dirPath, cubemapArray),
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

		/// <summary>
		/// Also handles <see cref="ICubemap"/> by inheritance
		/// </summary>
		private bool ExportTexture2D(IExportContainer container, string filePath, string dirPath, ITexture2D texture)
		{
			bool result;
			if (texture.Has_StreamData_C28())
			{
				ulong offset = texture.StreamData_C28.GetOffset();
				Utf8String path = texture.StreamData_C28.Path;
				uint size = texture.StreamData_C28.Size;
				if (texture.ImageData_C28.Length == 0 && texture.StreamData_C28.IsSet())
				{
					texture.ImageData_C28 = texture.StreamData_C28.GetContent(texture.Collection);
					texture.StreamData_C28.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					texture.ImageData_C28 = Array.Empty<byte>();
				}
				else
				{
					texture.StreamData_C28.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				texture.StreamData_C28.SetOffset(offset);
				texture.StreamData_C28.Path = path;
				texture.StreamData_C28.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}

		private bool ExportTexture3D(IExportContainer container, string filePath, string dirPath, ITexture3D texture)
		{
			bool result;
			if (texture.Has_StreamData())
			{
				ulong offset = texture.StreamData.GetOffset();
				Utf8String path = texture.StreamData.Path;
				uint size = texture.StreamData.Size;
				if (texture.ImageData.Length == 0 && texture.StreamData.IsSet())
				{
					texture.ImageData = texture.StreamData.GetContent(texture.Collection);
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					texture.ImageData = Array.Empty<byte>();
				}
				else
				{
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				texture.StreamData.SetOffset(offset);
				texture.StreamData.Path = path;
				texture.StreamData.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}

		private bool ExportTexture2DArray(IExportContainer container, string filePath, string dirPath, ITexture2DArray texture)
		{
			bool result;
			if (texture.Has_StreamData())
			{
				ulong offset = texture.StreamData.GetOffset();
				Utf8String path = texture.StreamData.Path;
				uint size = texture.StreamData.Size;
				if (texture.ImageData.Length == 0 && texture.StreamData.IsSet())
				{
					texture.ImageData = texture.StreamData.GetContent(texture.Collection);
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					texture.ImageData = Array.Empty<byte>();
				}
				else
				{
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				texture.StreamData.SetOffset(offset);
				texture.StreamData.Path = path;
				texture.StreamData.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}

		private bool ExportCubemapArray(IExportContainer container, string filePath, string dirPath, ICubemapArray texture)
		{
			bool result;
			if (texture.Has_StreamData())
			{
				ulong offset = texture.StreamData.GetOffset();
				Utf8String path = texture.StreamData.Path;
				uint size = texture.StreamData.Size;
				if (texture.ImageData.Length == 0 && texture.StreamData.IsSet())
				{
					texture.ImageData = texture.StreamData.GetContent(texture.Collection);
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
					texture.ImageData = Array.Empty<byte>();
				}
				else
				{
					texture.StreamData.ClearValues();
					result = base.ExportInner(container, filePath, dirPath);
				}
				texture.StreamData.SetOffset(offset);
				texture.StreamData.Path = path;
				texture.StreamData.Size = size;
			}
			else
			{
				result = base.ExportInner(container, filePath, dirPath);
			}

			return result;
		}
	}
}
