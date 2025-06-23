using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class YamlStreamedAssetExportCollection : AssetExportCollection<IUnityObjectBase>
{
	public YamlStreamedAssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return Asset switch
		{
			IMesh mesh => ExportMesh(container, filePath, dirPath, mesh, fileSystem),
			IImageTexture texture => ExportTexture(container, filePath, dirPath, texture, fileSystem),
			_ => false,
		};
	}

	private bool ExportMesh(IExportContainer container, string filePath, string dirPath, IMesh mesh, FileSystem fileSystem)
	{
		if (!mesh.Has_StreamData())
		{
			return base.ExportInner(container, filePath, dirPath, fileSystem);
		}

		bool result;
		mesh.StreamData.GetValues(out Utf8String path, out ulong offset, out uint size);
		if (mesh.VertexData.Data.Length != 0)
		{
			mesh.StreamData.ClearValues();
			result = base.ExportInner(container, filePath, dirPath, fileSystem);
		}
		else
		{
			mesh.VertexData.Data = mesh.StreamData.GetContent(mesh.Collection);
			mesh.StreamData.ClearValues();
			result = base.ExportInner(container, filePath, dirPath, fileSystem);
			mesh.VertexData.Data = [];
		}
		mesh.StreamData.SetValues(path, offset, size);

		return result;
	}

	private bool ExportTexture(IExportContainer container, string filePath, string dirPath, IImageTexture texture, FileSystem fileSystem)
	{
		if (!texture.Has_StreamData_C189())
		{
			return base.ExportInner(container, filePath, dirPath, fileSystem);
		}

		bool result;
		texture.StreamData_C189.GetValues(out Utf8String path, out ulong offset, out uint size);
		if (texture.ImageData_C189.Length != 0)
		{
			texture.StreamData_C189.ClearValues();
			result = base.ExportInner(container, filePath, dirPath, fileSystem);
		}
		else
		{
			byte[]? data = texture.StreamData_C189.GetContent(texture.Collection);
			if (data.IsNullOrEmpty())
			{
				return false;
			}
			texture.ImageData_C189 = data;
			texture.StreamData_C189.ClearValues();
			result = base.ExportInner(container, filePath, dirPath, fileSystem);
			texture.ImageData_C189 = [];
		}
		texture.StreamData_C189.SetValues(path, offset, size);

		return result;
	}
}
