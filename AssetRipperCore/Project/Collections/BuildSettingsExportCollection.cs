using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.EditorBuildSettings;
using AssetRipper.Core.Classes.EditorSettings;
using AssetRipper.Core.Classes.NavMeshProjectSettings;
using AssetRipper.Core.Classes.Physics2DSettings;
using AssetRipper.Core.Classes.QualitySettings;
using AssetRipper.Core.Classes.UnityConnectSettings;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class BuildSettingsExportCollection : ManagerExportCollection
	{
		public BuildSettingsExportCollection(IAssetExporter assetExporter, VirtualSerializedFile file, IUnityObjectBase asset) : this(assetExporter, file, (IBuildSettings)asset) { }

		public BuildSettingsExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IBuildSettings asset) : base(assetExporter, asset)
		{
			EditorBuildSettings = EditorBuildSettings.CreateVirtualInstance(virtualFile);
			EditorSettings = EditorSettings.CreateVirtualInstance(virtualFile);
			if (!NavMeshProjectSettings.HasNavMeshProjectSettings(asset.File.Version))
			{
				NavMeshProjectSettings = NavMeshProjectSettings.CreateVirtualInstance(virtualFile);
			}
			if (!NetworkManager.HasNetworkManager(asset.File.Version))
			{
				NetworkManager = NetworkManager.CreateVirtualInstance(virtualFile);
			}
			if (!Physics2DSettings.HasPhysics2DSettings(asset.File.Version))
			{
				Physics2DSettings = Physics2DSettings.CreateVirtualInstance(virtualFile);
			}
			if (!UnityConnectSettings.HasUnityConnectSettings(asset.File.Version))
			{
				UnityConnectSettings = UnityConnectSettings.CreateVirtualInstance(virtualFile);
			}
			if (!QualitySettings.HasQualitySettings(asset.File.Version))
			{
				QualitySettings = QualitySettings.CreateVirtualInstance(virtualFile);
			}
		}

		public override bool Export(IProjectAssetContainer container, string dirPath)
		{
			string subPath = Path.Combine(dirPath, ProjectSettingsName);
			string fileName = $"{EditorBuildSettings.ClassID}.asset";
			string filePath = Path.Combine(subPath, fileName);

			Directory.CreateDirectory(subPath);

			IBuildSettings asset = (IBuildSettings)Asset;
			IEnumerable<Scene> scenes = asset.Scenes.Select(t => new Scene(t, container.SceneNameToGUID(t)));
			EditorBuildSettings.Initialize(scenes);
			AssetExporter.Export(container, EditorBuildSettings, filePath);

			fileName = $"{EditorSettings.ClassID}.asset";
			filePath = Path.Combine(subPath, fileName);

			AssetExporter.Export(container, EditorSettings, filePath);

			if (NavMeshProjectSettings != null)
			{
				fileName = $"{NavMeshProjectSettings.ExportPath}.asset";
				filePath = Path.Combine(subPath, fileName);

				AssetExporter.Export(container, NavMeshProjectSettings, filePath);
			}
			if (NetworkManager != null)
			{
				fileName = $"{NetworkManager.ExportPath}.asset";
				filePath = Path.Combine(subPath, fileName);

				AssetExporter.Export(container, NetworkManager, filePath);
			}
			if (Physics2DSettings != null)
			{
				fileName = $"{Physics2DSettings.ExportPath}.asset";
				filePath = Path.Combine(subPath, fileName);

				AssetExporter.Export(container, Physics2DSettings, filePath);
			}
			if (UnityConnectSettings != null)
			{
				fileName = $"{UnityConnectSettings.ExportPath}.asset";
				filePath = Path.Combine(subPath, fileName);

				AssetExporter.Export(container, UnityConnectSettings, filePath);
			}
			if (QualitySettings != null)
			{
				fileName = $"{QualitySettings.ExportPath}.asset";
				filePath = Path.Combine(subPath, fileName);

				AssetExporter.Export(container, QualitySettings, filePath);
			}

			SaveProjectVersion(subPath);
			return true;
		}

		private static void SaveProjectVersion(string projectSettingsDirectory)
		{
			SaveProjectVersion(projectSettingsDirectory, new UnityVersion(2017, 3, 0, UnityVersionType.Final, 3));
		}
		private static void SaveProjectVersion(string projectSettingsDirectory, UnityVersion version)
		{
			using Stream fileStream = System.IO.File.Create(Path.Combine(projectSettingsDirectory, "ProjectVersion.txt"));
			using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
			writer.Write($"m_EditorVersion: {version}");
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			switch (asset.ClassID)
			{
				case ClassIDType.EditorBuildSettings:
					return asset == EditorBuildSettings;
				case ClassIDType.EditorSettings:
					return asset == EditorSettings;
				case ClassIDType.NavMeshProjectSettings:
					return asset == NavMeshProjectSettings;
				case ClassIDType.NetworkManager:
					return asset == Physics2DSettings;
				case ClassIDType.Physics2DSettings:
					return asset == EditorBuildSettings;
				case ClassIDType.UnityConnectSettings:
					return asset == UnityConnectSettings;
				case ClassIDType.QualitySettings:
					return asset == QualitySettings;

				default:
					return base.IsContains(asset);
			}
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return 1;
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				yield return Asset;
				yield return EditorBuildSettings;
				yield return EditorSettings;
				if (NavMeshProjectSettings != null)
				{
					yield return NavMeshProjectSettings;
				}
				if (NetworkManager != null)
				{
					yield return NetworkManager;
				}
				if (Physics2DSettings != null)
				{
					yield return Physics2DSettings;
				}
				if (UnityConnectSettings != null)
				{
					yield return UnityConnectSettings;
				}
				if (QualitySettings != null)
				{
					yield return QualitySettings;
				}
			}
		}

		public EditorBuildSettings EditorBuildSettings { get; }
		public EditorSettings EditorSettings { get; }
		public NavMeshProjectSettings NavMeshProjectSettings { get; }
		public NetworkManager NetworkManager { get; }
		public Physics2DSettings Physics2DSettings { get; }
		public UnityConnectSettings UnityConnectSettings { get; }
		public QualitySettings QualitySettings { get; }
	}
}
