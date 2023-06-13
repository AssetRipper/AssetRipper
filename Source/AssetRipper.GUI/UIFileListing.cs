using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Structure;
using AssetRipper.IO.Files.ResourceFiles;
using System.Collections.ObjectModel;

namespace AssetRipper.GUI
{
	public static class UIFileListing
	{
		public static List<NewUiFileListItem> GetItemsFromStructure(GameStructure structure)
		{
			List<NewUiFileListItem> ret = new();
			foreach (AssetCollection collection in structure.FileCollection.FetchAssetCollections())
			{
				//Create a top-level tree view entry for each file
				NewUiFileListItem? topLevelEntry = new(collection.Name, collection);

				//Create a dictionary to hold the sub-categories.
				Dictionary<string, NewUiFileListItem> categories = new();

				foreach (IUnityObjectBase asset in collection)
				{
					//Get the name of the category this asset should go in.
					string categoryName = asset.ClassName;

					//Get or create the category.
					NewUiFileListItem category;
					if (!categories.TryGetValue(categoryName, out category!))
					{
						category = new NewUiFileListItem(categoryName);
						categories[categoryName] = category;
					}

					//Create the asset tree view item
					NewUiFileListItem assetListItem = new(asset);

					//Add it to the category.
					category.SubItems.Add(assetListItem);
				}

				//Add categories to the top-level file.
				categories.Values.ToList().ForEach(item =>
				{
					item.UpdateOnceAllAssetsAdded();
					topLevelEntry.SubItems.Add(item);
				});

				//Add the top-level entry to our result
				ret.Add(topLevelEntry);
			}

			//Create a top-level tree view entry for any loose resource files
			NewUiFileListItem? looseFiles = new("Loose Resource Files");
			foreach (ResourceFile resourceFile in structure.FileCollection.FetchResourceFiles())
			{
				looseFiles.SubItems.Add(new(new DummyAssetForLooseResourceFile(resourceFile)));
			}

			ret.Add(looseFiles);

			return ret;
		}
	}

	public class NewUiFileListItem : BaseViewModel
	{
		private string _displayAs;
		private IUnityObjectBase? _associatedObject;
		private AssetCollection? _associatedFile;

		//Read from UI
		public string DisplayAs
		{
			get => _displayAs;
			set
			{
				_displayAs = value;
				OnPropertyChanged();
			}
		}

		public IUnityObjectBase? AsObjectAsset => _associatedObject;

		//Read from UI
		public ObservableCollection<NewUiFileListItem> SubItems { get; } = new();

		/// <summary>
		/// Creates a top-level tree view item from a <see cref="AssetCollection"/> and the given display name.
		/// </summary>
		public NewUiFileListItem(string name, AssetCollection resourceFile)
		{
			_displayAs = name;
			_associatedFile = resourceFile;
		}

		/// <summary>
		/// Creates a sub-level tree view item from an individual asset. Inherits the name of the asset if it is a NamedObject,
		/// otherwise takes the name of the asset's class.
		/// </summary>
		public NewUiFileListItem(IUnityObjectBase asset)
		{
			_associatedObject = asset;
			_displayAs = asset.GetBestName();
		}

		/// <summary>
		/// Creates an intermediate-level tree view item from a category name. Takes the name of the category as its name.
		/// </summary>
		public NewUiFileListItem(string categoryName)
		{
			_displayAs = categoryName;
		}

		public void UpdateOnceAllAssetsAdded()
		{
			DisplayAs += $" ({SubItems.Count})";

			//Sort alphabetically
			List<NewUiFileListItem> itemsCopy = SubItems.ToList();
			itemsCopy.Sort((a, b) => string.Compare(a.DisplayAs, b.DisplayAs, StringComparison.Ordinal));
			SubItems.Clear();
			itemsCopy.ForEach(SubItems.Add);
		}
	}
}
