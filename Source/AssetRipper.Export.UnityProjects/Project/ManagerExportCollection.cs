using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class ManagerExportCollection : AssetExportCollection<IUnityObjectBase>
	{
		public ManagerExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset) { }

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath = Path.Combine(projectDirectory, ProjectSettingsName);
			string name = GetCorrectName(Asset.ClassName);
			string fileName = $"{name}.asset";
			string filePath = Path.Combine(subPath, fileName);

			Directory.CreateDirectory(subPath);

			ExportInner(container, filePath, projectDirectory);
			return true;
		}

		public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
		{
			if (asset == Asset)
			{
				return 1;
			}
			throw new ArgumentException(null, nameof(asset));
		}

		public override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		private static string GetCorrectName(string typeName)
		{
			return typeName switch
			{
				PlayerSettingsName or "129" => ProjectSettingsName,
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
