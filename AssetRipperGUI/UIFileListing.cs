using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AudioManager;
using AssetRipper.Core.Classes.EditorBuildSettings;
using AssetRipper.Core.Classes.EditorSettings;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.Core.Classes.InputManager;
using AssetRipper.Core.Classes.LightmapSettings;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.NavMeshProjectSettings;
using AssetRipper.Core.Classes.NavMeshSettings;
using AssetRipper.Core.Classes.Physics2DSettings;
using AssetRipper.Core.Classes.PhysicsManager;
using AssetRipper.Core.Classes.QualitySettings;
using AssetRipper.Core.Classes.RenderSettings;
using AssetRipper.Core.Classes.ResourceManager;
using AssetRipper.Core.Classes.Rigidbody;
using AssetRipper.Core.Classes.TagManager;
using AssetRipper.Core.Classes.UI.Canvas;
using AssetRipper.Core.Classes.UnityConnectSettings;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Structure.GameStructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SerializedFile = AssetRipper.Core.Parser.Files.SerializedFiles.SerializedFile;

namespace AssetRipper.GUI
{
	public static class UIFileListing
	{
		private static readonly Type[] SettingsClasses =
		{
			typeof(AudioManager),
			typeof(BuildSettings),
			typeof(EditorBuildSettings),
			typeof(EditorSettings),
			typeof(GraphicsSettings),
			typeof(InputManager),
			typeof(LightingSettings),
			typeof(LightmapSettings),
			typeof(MonoManager),
			typeof(NavMeshProjectSettings),
			typeof(NavMeshSettings),
			typeof(Physics2DSettings),
			typeof(PhysicsManager),
			typeof(QualitySettings),
			typeof(RenderSettings),
			typeof(ResourceManager),
			typeof(TagManager),
			typeof(TimeManager),
			typeof(UnityConnectSettings),
		};

		public static List<NewUiFileListItem> GetItemsFromStructure(GameStructure structure)
		{
			List<NewUiFileListItem> ret = new();
			foreach (var (name, resourceFile) in structure.FileCollection.GameFiles)
			{
				//Create a top-level tree view entry for each file
				NewUiFileListItem? topLevelEntry = new(name!, resourceFile);

				//Create a dictionary to hold the sub-categories.
				Dictionary<string, NewUiFileListItem> categories = new();

				foreach (IUnityObjectBase asset in resourceFile.FetchAssets())
				{
					//Get the name of the category this asset should go in.
					string categoryName = GetCategoryName(asset.GetType());

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
			foreach (ResourceFile resourceFile in structure.FileCollection.GameResourceFiles)
			{
				if (resourceFile != null)
					looseFiles.SubItems.Add(new(new DummyAssetForLooseResourceFile(resourceFile)));
			}

			ret.Add(looseFiles);

			return ret;
		}

		private static string GetCategoryName(Type assetType)
		{
			if (SettingsClasses.Contains(assetType))
				return "Settings";

			return assetType.Name switch
			{
				nameof(Mesh) => "Meshes",
				nameof(Canvas) => "Canvases",
				nameof(Rigidbody) => "Rigidbodies",
				_ => $"{assetType.Name}s",//E.g. Materials, AudioClips, etc.
			};
		}
	}

	public class NewUiFileListItem : BaseViewModel
	{
		private string _displayAs;
		private IUnityObjectBase? _associatedObject;
		private SerializedFile? _associatedFile;

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
		/// Creates a top-level tree view item from a SerializedFile and the given display name.
		/// </summary>

		public NewUiFileListItem(string name, SerializedFile resourceFile)
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

			if (_associatedObject is INamedObject no)
			{
				_displayAs = no.GetValidName();
			}

			if (_associatedObject is IHasName hasName)
			{
				_displayAs = hasName.Name;
			}

			if (_associatedObject is UnknownObject)
			{
				_displayAs = asset.ClassID.ToString();
			}

			if (string.IsNullOrEmpty(_displayAs))
			{
				_displayAs = _associatedObject.GetType().Name;
			}

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