using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ResourceManagers;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class ResourceManager : GlobalGameManager
	{
		public ResourceManager(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool IsReadDependentAssets(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3, 5) && flags.IsRelease();
		}

		public override void Read(AssetReader reader)
        {
            base.Read(reader);

            m_container = reader.ReadStringTKVPArray<PPtr<Object>>();
            if (IsReadDependentAssets(reader.Version, reader.Flags))
            {
                m_dependentAssets = reader.ReadAssetArray<ResourceManagerDependency>();
            }
        }

        public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach (KeyValuePair<string, PPtr<Object>> asset in Container)
			{
				yield return asset.Value.FetchDependency(file, isLog, () => nameof(ResourceManager), ContainerName);
			}
			if (IsReadDependentAssets(file.Version, file.Flags))
			{
				foreach (ResourceManagerDependency dependentAsset in DependentAssets)
				{
					foreach (Object asset in dependentAsset.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ContainerName, Container.ExportYAML(container));
			if (IsReadDependentAssets(container.Version, container.ExportFlags))
			{
				node.Add(DependentAssetsName, DependentAssets.ExportYAML(container));
			}
			return node;
		}

        public bool GetResourcePathFromAsset(Object asset, string filePath, ref string resourceSubFolder, ref string resourceFileName)
        {
            string assetsFolderLocation = filePath.Substring(0, filePath.LastIndexOf("Assets\\") + 7);
            string exportedFileExtension = filePath.Substring(filePath.LastIndexOf("."));

            foreach (KeyValuePair<string, PPtr<Object>> containerEntry in m_container)
            {
                try
                {
                    if (asset == containerEntry.Value.GetAsset(File)) // containerEntry.Key contains a basic path in lowercase (folder1/folder2/filename), containerEntry.Value is linked to the actual asset
                    {
                        string basicPath = "Resources/" + containerEntry.Key;
                        string pathWithNoFileName = basicPath.Substring(0, basicPath.LastIndexOf('/') + 1);
                        string resourceFileNameFromPath = basicPath.Substring(basicPath.LastIndexOf('/') + 1); // lacks an extension, we need to use it because sometimes it's different than the export name given to it

                        resourceFileName = resourceFileNameFromPath + exportedFileExtension;
                        resourceSubFolder = assetsFolderLocation + pathWithNoFileName;

                        return true; 
                    }
                } catch (System.Exception ex) // we might run into a "path ID x was not found" error (maybe unimplemented class?)
                {
                    continue;
                }
            }
            return false;
        }

		public IReadOnlyList<KeyValuePair<string, PPtr<Object>>> Container => m_container;
		public ILookup<string, PPtr<Object>> ContainerMap => Container.ToLookup(t => t.Key, t => t.Value);
		public IReadOnlyList<ResourceManagerDependency> DependentAssets => m_dependentAssets;

		public const string ContainerName = "m_Container";
		public const string DependentAssetsName = "m_DependentAssets";

		private KeyValuePair<string, PPtr<Object>>[] m_container;
		private ResourceManagerDependency[] m_dependentAssets;
	}
}
