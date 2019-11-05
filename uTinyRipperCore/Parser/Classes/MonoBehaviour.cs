using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;
using uTinyRipper.Game.Assembly;

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
		public static bool HasEditorHideFlags(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 2019.1 to 2019.1.0b4 exclusive and Not Release
		/// </summary>
		public static bool HasGeneratorAsset(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019) && version.IsLess(2019, 1, 0, VersionType.Beta, 4);
		}
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public static bool HasEditorClassIdentifier(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		}

		private static bool IsAlign(Version version, TransferInstructionFlags flags)
		{
			// NOTE: unknown version
			if (version.IsGreaterEqual(3))
			{
				return true;
			}
			if (version.IsGreaterEqual(2, 1) && flags.IsRelease())
			{
				return true;
			}
			return false;
		}

		public new static void GenerateTypeTree(TypeTreeContext context)
		{
			Behaviour.GenerateTypeTree(context);

			if (HasEditorHideFlags(context.Flags))
			{
				context.AddUInt32(EditorHideFlagsName);
			}
			if (HasGeneratorAsset(context.Version, context.Flags))
			{
				context.AddPPtr(nameof(Object), GeneratorAssetName);
			}
			context.AddPPtr(nameof(MonoScript), ScriptName);
			context.AddString(NameName);
			if (HasEditorClassIdentifier(context.Version, context.Flags))
			{
				context.AddString(EditorClassIdentifierName);
			}
		}

		public override void Read(AssetReader reader)
		{
			long position = reader.BaseStream.Position;
			base.Read(reader);

#if UNIVERSAL
			if (HasEditorHideFlags(reader.Flags))
			{
				EditorHideFlags = (HideFlags)reader.ReadUInt32();
			}
			if (HasGeneratorAsset(reader.Version, reader.Flags))
			{
				GeneratorAsset.Read(reader);
			}
#endif

			Script.Read(reader);
			Name = reader.ReadString();

#if UNIVERSAL
			if (HasEditorClassIdentifier(reader.Version, reader.Flags))
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
					Structure = behaviourType.CreateSerializableStructure();
					Structure.Read(reader);
					return;
				}
			}

			AssetEntry info = File.GetAssetEntry(PathID);
			reader.BaseStream.Position = position + info.Size;
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

#if UNIVERSAL
			yield return context.FetchDependency(GeneratorAsset, GeneratorAssetName);
#endif
			yield return context.FetchDependency(Script, ScriptName);

			if (Structure != null)
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Structure, Structure.Type.Name))
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
			if (HasGeneratorAsset(container.ExportVersion, container.ExportFlags))
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
			if (HasEditorHideFlags(flags))
			{
				return EditorHideFlags;
			}
#endif
			return HideFlags.None;
		}
		private PPtr<Object> GetGeneratorAsset(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasGeneratorAsset(version, flags))
			{
				return GeneratorAsset;
			}
#endif
			return default;
		}
		private string GetEditorClassIdentifier(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorClassIdentifier(version, flags))
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
		public bool IsScriptableObject => Name.Length > 0;

#if UNIVERSAL
		public HideFlags EditorHideFlags { get; set; }
#endif
		public string Name { get; set; }
		public SerializableStructure Structure { get; set; }
#if UNIVERSAL
		public string EditorClassIdentifier { get; set; }
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
