using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_6;
using System.IO;

namespace AssetRipper.Core.Project.Collections
{
	public class ManagerExportCollection : AssetExportCollection
	{
		public ManagerExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : this(assetExporter, (GlobalGameManager)asset) { }

		public ManagerExportCollection(IAssetExporter assetExporter, GlobalGameManager asset) : base(assetExporter, asset) { }

		public override bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			string subPath = Path.Combine(projectDirectory, ProjectSettingsName);
			string name = GetCorrectName(Asset.AssetClassName);
			string fileName = $"{name}.asset";
			string filePath = Path.Combine(subPath, fileName);

			Directory.CreateDirectory(subPath);

			ExportInner(container, filePath, projectDirectory);
			return true;
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (asset == Asset)
			{
				return 1;
			}
			throw new ArgumentException(null, nameof(asset));
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		private static string GetCorrectName(string typeName)
		{
			return typeName switch
			{
				PlayerSettingsName => ProjectSettingsName,
				NavMeshProjectSettingsName => NavMeshAreasName,
				PhysicsManagerName => DynamicsManagerName,
				_ => typeName,
			};
		}

		//Type names
		protected const string PlayerSettingsName = "PlayerSettings";
		protected const string NavMeshProjectSettingsName = "NavMeshProjectSettings";
		protected const string PhysicsManagerName = "PhysicsManager";

		//Altered names
		protected const string ProjectSettingsName = "ProjectSettings";
		protected const string NavMeshAreasName = "NavMeshAreas";
		protected const string DynamicsManagerName = "DynamicsManager";
	}
}
