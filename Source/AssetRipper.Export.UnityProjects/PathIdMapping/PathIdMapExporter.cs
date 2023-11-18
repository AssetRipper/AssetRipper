using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_27;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.PathIdMapping;

public sealed class PathIdMapExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, LibraryConfiguration settings)
	{
		SerializedGameInfo gameInfo = new();
		foreach (SerializedAssetCollection collection in gameData.GameBundle.FetchAssetCollections().OfType<SerializedAssetCollection>())
		{
			SerializedFileInfo fileInfo = new()
			{
				Name = collection.Name,
			};
			gameInfo.Files.Add(fileInfo);
			foreach (IUnityObjectBase asset in collection)
			{
				if (asset is IMesh or ITexture or IAudioClip or ITextAsset)//Commonly useful asset types
				{
					fileInfo.Assets.Add(new()
					{
						Name = (asset as INamed)?.Name,
						Type = asset.ClassName,
						PathID = asset.PathID,
					});
				}
			}
		}

		string outputDirectory = settings.AuxiliaryFilesPath;
		Directory.CreateDirectory(outputDirectory);
		using FileStream stream = File.Create(Path.Combine(outputDirectory, "path_id_map.json"));
		JsonSerializer.Serialize(stream, gameInfo, SerializedGameInfoSerializerContext.Default.SerializedGameInfo);
		stream.Flush();
	}
}
