using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Library.Exporters.TypeTrees
{
	public class TypeTreeExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			string outputDirectory = Path.Combine(ripper.Settings.AuxiliaryFilesPath, "TypeTrees");

			Logger.Info(LogCategory.Export, "Exporting type trees...");
			foreach (Core.Parser.Files.SerializedFiles.SerializedFile? serializedFile in ripper.GameStructure.FileCollection.GameSerializedFiles)
			{
				if (serializedFile.Metadata.EnableTypeTree)
				{
					Logger.Info(LogCategory.Export, serializedFile.Name);
				}

				List<IMonoScript> monoScripts = serializedFile.FetchAssets().Where(asset => asset is IMonoScript).Select(asset => (IMonoScript)asset).ToList();
				StringBuilder sb = new StringBuilder();
				foreach (Core.Parser.Files.SerializedFiles.Parser.SerializedType? type in serializedFile.Metadata.Types)
				{
					//Logger.Info(LogCategory.Export, $"\t\tID: {type.TypeID.ToString()} Node Count: {type.OldType?.Nodes?.Count ?? 0}");
					string? typeTreeText = type.OldType?.Dump;
					if (!string.IsNullOrEmpty(typeTreeText))
					{
						IMonoScript? monoScript = monoScripts.FirstOrDefault(asset => asset.GetPropertiesHash() == type.OldTypeHash);
						string typeName = monoScript is null ? type.TypeID.ToString() : monoScript.GetFullName();
						sb.AppendLine($"// classID{{{(int)type.TypeID}}}: {typeName}");
						sb.AppendLine(typeTreeText);
					}
				}
				string text = sb.ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					Directory.CreateDirectory(outputDirectory);
					string filePath = Path.Combine(outputDirectory, serializedFile.Name + ".txt");
					TaskManager.AddTask(File.WriteAllTextAsync(filePath, text));
				}
			}
		}
	}
}
