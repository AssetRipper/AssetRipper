using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Collider2D : Behaviour
	{
		protected Collider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadDensity(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadUsedByEffector(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadUsedByComposite(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadOffset(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadDensity(reader.Version))
			{
				Density = reader.ReadSingle();
			}
			Material.Read(reader);
			IsTrigger = reader.ReadBoolean();
			if (IsReadUsedByEffector(reader.Version))
			{
				UsedByEffector = reader.ReadBoolean();
			}
			if (IsReadUsedByComposite(reader.Version))
			{
				UsedByComposite = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadOffset(reader.Version))
			{
				Offset.Read(reader);
			}
		}
		
		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Material.FetchDependency(file, isLog, ToLogString, "m_Material");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Density", Density);
			node.Add("m_Material", Material.ExportYAML(container));
			node.Add("m_IsTrigger", IsTrigger);
			node.Add("m_UsedByEffector", UsedByEffector);
			node.Add("m_UsedByComposite", UsedByComposite);
			node.Add("m_Offset", Offset.ExportYAML(container));
			return node;
		}

		public float Density { get; private set; }
		public bool IsTrigger { get; private set; }
		public bool UsedByEffector { get; private set; }
		public bool UsedByComposite { get; private set; }

		public PPtr<PhysicsMaterial2D> Material;
		public Vector2f Offset;
	}
}
