using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Collider : Component
	{
		protected Collider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

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
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return Material.FetchDependency(file, isLog, ToLogString, "m_Material");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsReadMaterial)
			{
				node.Add("m_Material", Material.ExportYAML(container));
			}
			if (IsReadIsTrigger)
			{
				node.Add("m_IsTrigger", IsTrigger);
			}
			node.Add("m_Enabled", Enabled);
			return node;
		}
		
		public bool IsTrigger { get; private set; }
		public bool Enabled { get; private set; }

		public PPtr<PhysicMaterial> Material;

		protected abstract bool IsReadMaterial { get; }
		protected abstract bool IsReadIsTrigger { get; }
	}
}
