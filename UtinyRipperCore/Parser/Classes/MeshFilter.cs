using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class MeshFilter : Component
	{
		public MeshFilter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Mesh.Read(stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			Mesh mesh = Mesh.FindObject(file);
			if (mesh == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"{ToLogString()} m_Mesh {Mesh.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return mesh;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Mesh", Mesh.ExportYAML(exporter));
			return node;
		}

		public PPtr<Mesh> Mesh;
	}
}
