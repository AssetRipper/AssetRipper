using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class MonoBehaviour : Behaviour
	{
		public MonoBehaviour(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			long position = reader.BaseStream.Position;
			base.Read(reader);

			Script.Read(reader);
			Name = reader.ReadString();
			
			MonoScript script = Script.FindAsset(File);
			if (script != null)
			{
				Structure = script.CreateStructure();
				if(Structure != null)
				{
					Structure.Read(reader);
					return;
				}
			}

			AssetEntry info = File.GetAssetEntry(PathID);
			reader.BaseStream.Position = position + info.DataSize;
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Script.FindAsset(file);
			
			if(Structure != null)
			{
				foreach (Object asset in Structure.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public override string ToString()
		{
			return $"{Name}({nameof(MonoBehaviour)})";
		}

		/// <summary>
		/// Whether this MonoBeh belongs to scene/prefab hierarchy or not
		/// </summary>
		public bool IsSceneObject()
		{
			// TODO: find out why GameObject may has value like PPtr(0, 894) but such game object doesn't exists
			return GameObject.FindAsset(File) != null;
		}

		public bool IsScriptableObject()
		{
			return Name != string.Empty;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EditorHideFlagsName, false);
			node.Add(ScriptName, Script.ExportYAML(container));
			node.Add(NameName, Name);
			node.Add(EditorClassIdentifierName, string.Empty);
			if (Structure != null)
			{
				YAMLMappingNode structureNode = (YAMLMappingNode)Structure.ExportYAML(container);
				node.Concatenate(structureNode);
			}
			return node;
		}

		public override string ExportName => Path.Combine(AssetsKeyWord, "ScriptableObject");
		public override string ExportExtension => AssetExtension;

		public string Name { get; private set; }
		public ScriptStructure Structure { get; private set; }

		public const string EditorHideFlagsName = "m_EditorHideFlags";
		public const string ScriptName = "m_Script";
		public const string NameName = "m_Name";
		public const string EditorClassIdentifierName = "m_EditorClassIdentifier";

		public PPtr<MonoScript> Script;
	}
}
