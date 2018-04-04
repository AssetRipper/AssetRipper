using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadDensity(stream.Version))
			{
				Density = stream.ReadSingle();
			}
			Material.Read(stream);
			IsTrigger = stream.ReadBoolean();
			if (IsReadUsedByEffector(stream.Version))
			{
				UsedByEffector = stream.ReadBoolean();
			}
			if (IsReadUsedByComposite(stream.Version))
			{
				UsedByComposite = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);

			if (IsReadOffset(stream.Version))
			{
				Offset.Read(stream);
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

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Density", Density);
			node.Add("m_Material", Material.ExportYAML(exporter));
			node.Add("m_IsTrigger", IsTrigger);
			node.Add("m_UsedByEffector", UsedByEffector);
			node.Add("m_UsedByComposite", UsedByComposite);
			node.Add("m_Offset", Offset.ExportYAML(exporter));
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
