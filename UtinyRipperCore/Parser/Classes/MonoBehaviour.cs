using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class MonoBehaviour : Behaviour
	{
		public MonoBehaviour(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			long position = stream.BaseStream.Position;
			base.Read(stream);

			Script.Read(stream);
			Name = stream.ReadStringAligned();
			
			MonoScript script = Script.FindObject(File);
			if(script != null)
			{
				Structure = script.CreateStructure();
				if(Structure != null)
				{
					Structure.Read(stream);
					return;
				}
			}

			ObjectInfo info = File.GetObjectInfo(PathID);
			stream.BaseStream.Position = position + info.DataSize;
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Script.GetObject(file);
			
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

		public bool IsScriptableObject()
		{
			if(!GameObject.IsNull)
			{
				return false;
			}

			IScriptStructure structure = Structure;
			while(structure != null)
			{
				if(ScriptType.IsScriptableObject(structure.Namespace, structure.Name))
				{
					return true;
				}
				structure = structure.Base;
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_EditorHideFlags", false);
			node.Add("m_Script", Script.ExportYAML(container));
			node.Add("m_Name", Name);
			node.Add("m_EditorClassIdentifier", string.Empty);
			if (Structure != null)
			{
				YAMLMappingNode structureNode = (YAMLMappingNode)Structure.ExportYAML(container);
				node.Concatenate(structureNode);
			}
			return node;
		}

		public override bool IsValid
		{
			get
			{
				if(GameObject.IsNull)
				{
					return IsScriptableObject();
				}
				return true;
			}
		}

		public override string ExportName => Path.Combine(AssetsName, "ScriptableObject");
		public override string ExportExtension => AssetExtension;

		public string Name { get; private set; }
		public ScriptStructure Structure { get; private set; }

		public PPtr<MonoScript> Script;
	}
}
