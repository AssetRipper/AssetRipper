using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Collider : Component
	{
		protected Collider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadEnabled(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsMaterialConditional(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsTriggerConditional(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (!IsMaterialConditional(reader.Version) || IsReadMaterial)
			{
				Material.Read(reader);
			}
			if (!IsTriggerConditional(reader.Version) || IsReadIsTrigger)
			{
				IsTrigger = reader.ReadBoolean();
			}
			if (IsReadEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}
		
		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Material.FetchDependency(file, isLog, ToLogString, MaterialName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsReadMaterial)
			{
				node.Add(MaterialName, Material.ExportYAML(container));
			}
			if (IsReadIsTrigger)
			{
				node.Add(IsTriggerName, IsTrigger);
			}
			node.Add(EnabledName, Enabled);
			return node;
		}

		protected void ReadComponent(AssetReader reader)
		{
			base.Read(reader);
		}
		
		public bool IsTrigger { get; private set; }
		public bool Enabled { get; protected set; }

		public const string MaterialName = "m_Material";
		public const string IsTriggerName = "m_IsTrigger";
		public const string EnabledName = "m_Enabled";

		public PPtr<PhysicMaterial> Material;

		protected abstract bool IsReadMaterial { get; }
		protected abstract bool IsReadIsTrigger { get; }
	}
}
