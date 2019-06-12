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

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadEditorHideFlags(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 2019.1 to 2019.1.0b4 exclusive and Not Release
		/// </summary>
		public static bool IsReadGeneratorAsset(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019) && version.IsLess(2019, 1, 0, VersionType.Beta, 4);
		}
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public static bool IsReadEditorClassIdentifier(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		}

		public override void Read(AssetReader reader)
		{
			long position = reader.BaseStream.Position;
			base.Read(reader);

#if UNIVERSAL
			if (IsReadEditorHideFlags(reader.Flags))
			{
				EditorHideFlags = (HideFlags)reader.ReadUInt32();
			}
			if (IsReadGeneratorAsset(reader.Version, reader.Flags))
			{
				GeneratorAsset.Read(reader);
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
				SerializableType behaviourType = script.GetBehaviourType();
				if (behaviourType != null)
				{
					Structure = behaviourType.CreateBehaviourStructure();
					Structure.Read(reader);
					return;
				}
			}

			AssetEntry info = File.GetAssetEntry(PathID);
			reader.BaseStream.Position = position + info.Size;
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

#if UNIVERSAL
			yield return GeneratorAsset.FindAsset(file);
#endif
			yield return Script.FindAsset(file);

			if (Structure != null)
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

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EditorHideFlagsName, (uint)GetEditorHideFlags(container.Version, container.Flags));
			if (IsReadGeneratorAsset(container.ExportVersion, container.ExportFlags))
			{
				node.Add(GeneratorAssetName, GetGeneratorAsset(container.Version, container.Flags).ExportYAML(container));
			}
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
			if (IsReadEditorHideFlags(flags))
			{
				return EditorHideFlags;
			}
#endif
			return HideFlags.None;
		}
		private PPtr<Object> GetGeneratorAsset(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadGeneratorAsset(version, flags))
			{
				return GeneratorAsset;
			}
#endif
			return default;
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

		public override string ExportPath => Path.Combine(AssetsKeyword, "ScriptableObject");
		public override string ExportExtension => AssetExtension;

		/// <summary>
		/// Whether this MonoBeh belongs to scene/prefab hierarchy or not
		/// </summary>
		// TODO: find out why GameObject may has value like PPtr(0, 894) but such game object doesn't exists
		public bool IsSceneObject => !GameObject.IsNull;
		public bool IsScriptableObject => Name != string.Empty;

#if UNIVERSAL
		public HideFlags EditorHideFlags { get; private set; }
#endif
		public string Name { get; private set; }
		public SerializableStructure Structure { get; private set; }
#if UNIVERSAL
		public string EditorClassIdentifier { get; private set; }
#endif

		public const string EditorHideFlagsName = "m_EditorHideFlags";
		public const string GeneratorAssetName = "m_GeneratorAsset";
		public const string ScriptName = "m_Script";
		public const string NameName = "m_Name";
		public const string EditorClassIdentifierName = "m_EditorClassIdentifier";

#if UNIVERSAL
		public PPtr<Object> GeneratorAsset;
#endif
		public PPtr<MonoScript> Script;
	}
}
