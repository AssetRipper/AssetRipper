using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Objects;
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

		public static bool IsReadEditorHideFlags(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		public static bool IsReadEditorClassIdentifier(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			long position = reader.BaseStream.Position;
			base.Read(reader);

#if UNIVERSAL
			if (IsReadEditorHideFlags(reader.Version, reader.Flags))
			{
				EditorHideFlags = (HideFlags)reader.ReadUInt32();
			}
#endif
			Script.Read(reader);
			Name = reader.ReadString();
#if UNIVERSAL
			if (IsReadEditorClassIdentifier(reader.Version, reader.Flags))
			{
				EditorClassIdentifier = reader.ReadString();
			}
#endif

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
			node.Add(EditorHideFlagsName, (uint)GetEditorHideFlags(container.Version, container.Flags));
			node.Add(ScriptName, Script.ExportYAML(container));
			node.Add(NameName, Name);
			node.Add(EditorClassIdentifierName, GetEditorClassIdentifier(container.Version, container.Flags));
			if (Structure != null)
			{
				YAMLMappingNode structureNode = (YAMLMappingNode)Structure.ExportYAML(container);
				node.Concatenate(structureNode);
			}
			return node;
		}

		private HideFlags GetEditorHideFlags(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorHideFlags(version, flags))
			{
				return EditorHideFlags;
			}
#endif
			return HideFlags.None;
		}
		private string GetEditorClassIdentifier(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorClassIdentifier(version, flags))
			{
				return EditorClassIdentifier;
			}
#endif
			return string.Empty;
		}

		public override string ExportName => Path.Combine(AssetsKeyWord, "ScriptableObject");
		public override string ExportExtension => AssetExtension;

#if UNIVERSAL
		public HideFlags EditorHideFlags { get; private set; }
#endif
		public string Name { get; private set; }
		public ScriptStructure Structure { get; private set; }
#if UNIVERSAL
		public string EditorClassIdentifier { get; private set; }
#endif

		public const string EditorHideFlagsName = "m_EditorHideFlags";
		public const string ScriptName = "m_Script";
		public const string NameName = "m_Name";
		public const string EditorClassIdentifierName = "m_EditorClassIdentifier";

		public PPtr<MonoScript> Script;
	}
}
