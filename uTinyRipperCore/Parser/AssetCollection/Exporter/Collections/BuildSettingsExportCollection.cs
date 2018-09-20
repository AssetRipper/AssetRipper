using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.EditorBuildSettingss;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public sealed class BuildSettingsExportCollection : ManagerExportCollection
	{
		public BuildSettingsExportCollection(IAssetExporter assetExporter, VirtualSerializedFile file, Object asset) :
			this(assetExporter, file, (BuildSettings)asset)
		{
		}

		public BuildSettingsExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, BuildSettings asset) :
			base(assetExporter, asset)
		{
			EditorBuildSettings = EditorBuildSettings.CreateVirtualInstance(virtualFile);
			EditorSettings = EditorSettings.CreateVirtualInstance(virtualFile);
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath = Path.Combine(dirPath, ProjectSettingsName);
			string fileName = $"{EditorBuildSettings.ClassID.ToString()}.asset";
			string filePath = Path.Combine(subPath, fileName);

			if (!DirectoryUtils.Exists(subPath))
			{
				DirectoryUtils.CreateDirectory(subPath);
			}

			BuildSettings asset = (BuildSettings)Asset;
			IEnumerable<Scene> scenes = asset.Scenes.Select(t => new Scene(t, container.SceneNameToGUID(t)));
			EditorBuildSettings.Initialize(scenes);
			AssetExporter.Export(container, EditorBuildSettings, filePath);

			fileName = $"{EditorSettings.ClassID.ToString()}.asset";
			filePath = Path.Combine(subPath, fileName);

			AssetExporter.Export(container, EditorSettings, filePath);
			
			fileName = $"ProjectVersion.txt";
			filePath = Path.Combine(subPath, fileName);

			using (FileStream file = FileUtils.Create(filePath))
			{
				using (StreamWriter writer = new StreamWriter(file))
				{
					writer.Write("m_EditorVersion: 2017.3.0f3");
				}
			}
			return true;
		}

		public override bool IsContains(Object asset)
		{
			if(asset == EditorBuildSettings || asset == EditorSettings)
			{
				return true;
			}
			return base.IsContains(asset);
		}

		public override long GetExportID(Object asset)
		{
			return 1;
		}

		public override IEnumerable<Object> Assets
		{
			get
			{
				yield return Asset;
				yield return EditorBuildSettings;
				yield return EditorSettings;
			}
		}

		public EditorBuildSettings EditorBuildSettings { get; }
		public EditorSettings EditorSettings { get; }
	}
}
