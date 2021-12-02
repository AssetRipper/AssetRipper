using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public sealed class MonoBehaviour : Behaviour, IMonoBehaviour
	{
		public MonoBehaviour(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			long position = reader.BaseStream.Position;
			base.Read(reader);

			Script.Read(reader);
			Name = reader.ReadString();

			ReadStructure(reader);
			ObjectInfo info = File.GetAssetEntry(PathID);
			reader.BaseStream.Position = position + info.ByteSize;
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			Script.Write(writer);
			writer.Write(Name);

			if (Structure != null)
			{
				Structure.Write(writer);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Script, ScriptName);

			if (Structure != null)
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Structure, Structure.Type.Name))
				{
					yield return asset;
				}
			}
		}

		public override string ToString()
		{
			return $"{Name}({nameof(MonoBehaviour)})";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EditorHideFlagsName, (uint)GetEditorHideFlags(container));
			if (HasGeneratorAsset(container.ExportVersion, container.ExportFlags))
			{
				node.Add(GeneratorAssetName, GetGeneratorAsset(container).ExportYAML(container));
			}
			node.Add(ScriptName, Script.ExportYAML(container));
			node.Add(NameName, Name);
			node.Add(EditorClassIdentifierName, GetEditorClassIdentifier(container));
			if (Structure != null)
			{
				YAMLMappingNode structureNode = (YAMLMappingNode)Structure.ExportYAML(container);
				node.Append(structureNode);
			}
			return node;
		}

		private HideFlags GetEditorHideFlags(IExportContainer container)
		{
			return HideFlags.None;
		}
		private PPtr<Object.Object> GetGeneratorAsset(IExportContainer container)
		{
			return default;
		}
		private string GetEditorClassIdentifier(IExportContainer container)
		{
			return string.Empty;
		}

		/// <summary>Reads the structure with an AssetReader</summary>
		private void ReadStructure(AssetReader reader)
		{
			if (!File.Collection.AssemblyManager.IsSet)
			{
				return;
			}

			MonoScript script = Script.FindAsset(File);
			if (script == null)
			{
				return;
			}

			SerializableType behaviourType = script.GetBehaviourType();
			if (behaviourType == null)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Unable to read {ValidName}, because valid definition for script {script.ValidName} wasn't found");
				return;
			}

			Structure = behaviourType.CreateSerializableStructure();
			try
			{
				Structure.Read(reader);
			}
			catch(System.Exception ex)
			{
				Structure = null;
				Logger.Log(LogType.Error, LogCategory.Import, $"Unable to read {ValidName}, because script layout {script.ValidName} mismatch binary content");
				Logger.Log(LogType.Debug, LogCategory.Import, $"Stack trace: {ex.ToString()}");
			}
			return;
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasEditorHideFlags(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 2019.1 to 2019.1.0b4 exclusive and Not Release
		/// </summary>
		public static bool HasGeneratorAsset(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2019) && version.IsLess(2019, 1, 0, UnityVersionType.Beta, 4) && !flags.IsRelease();
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public static bool HasEditorClassIdentifier(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(4, 2) && !flags.IsRelease();

		public override string ExportPath => Path.Combine(AssetsKeyword, "ScriptableObject");
		public override string ExportExtension => AssetExtension;

		public string ValidName => Name.Length == 0 ? nameof(MonoBehaviour) : Name;
		

		public string Name { get; set; }
		public SerializableStructure Structure { get; set; }

		public PPtr<MonoScript> Script;
		public PPtr<IMonoScript> ScriptPtr => Script.CastTo<IMonoScript>();

		public const string EditorHideFlagsName = "m_EditorHideFlags";
		public const string GeneratorAssetName = "m_GeneratorAsset";
		public const string ScriptName = "m_Script";
		public const string NameName = "m_Name";
		public const string EditorClassIdentifierName = "m_EditorClassIdentifier";
	}
}
