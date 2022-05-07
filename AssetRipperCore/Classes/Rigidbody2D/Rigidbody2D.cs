﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Rigidbody2D
{
	public sealed class Rigidbody2D : Component
	{
		public Rigidbody2D(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 5))
			{
				return 4;
			}
			// there is no 3rd version
			if (version.IsGreaterEqual(5, 1))
			{
				return 2;
			}
			return 4;
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasBodyType(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasUseAutoMass(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMaterial(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasInterpolate(UnityVersion version) => version.IsGreaterEqual(5, 5);

		/// <summary>
		/// Less than 5.1.0
		/// </summary>
		private static bool HasFixedAngle(UnityVersion version) => version.IsLess(5, 1);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool HasIsKinematic(UnityVersion version) => version.IsLess(5, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasBodyType(reader.Version))
			{
				BodyType = (RigidbodyType2D)reader.ReadInt32();
				Simulated = reader.ReadBoolean();
				UseFullKinematicContacts = reader.ReadBoolean();
			}
			if (HasUseAutoMass(reader.Version))
			{
				UseAutoMass = reader.ReadBoolean();
				reader.AlignStream();
			}

			Mass = reader.ReadSingle();
			LinearDrag = reader.ReadSingle();
			AngularDrag = reader.ReadSingle();
			GravityScale = reader.ReadSingle();
			if (HasMaterial(reader.Version))
			{
				Material.Read(reader);
			}

			if (HasFixedAngle(reader.Version))
			{
				bool fixedAngle = reader.ReadBoolean();
				Constraints = fixedAngle ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
			}
			if (HasIsKinematic(reader.Version))
			{
				bool isKinematic = reader.ReadBoolean();
				BodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static;
				Interpolate = (RigidbodyInterpolation2D)reader.ReadByte();
				SleepingMode = (RigidbodySleepMode2D)reader.ReadByte();
				CollisionDetection = (CollisionDetectionMode2D)reader.ReadByte();
				reader.AlignStream();
			}

			if (HasInterpolate(reader.Version))
			{
				Interpolate = (RigidbodyInterpolation2D)reader.ReadInt32();
				SleepingMode = (RigidbodySleepMode2D)reader.ReadInt32();
				CollisionDetection = (CollisionDetectionMode2D)reader.ReadInt32();
			}
			if (!HasFixedAngle(reader.Version))
			{
				Constraints = (RigidbodyConstraints2D)reader.ReadInt32();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Material, MaterialName);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BodyTypeName, (int)BodyType);
			node.Add(SimulatedName, Simulated);
			node.Add(UseFullKinematicContactsName, UseFullKinematicContacts);
			node.Add(UseAutoMassName, UseAutoMass);
			node.Add(MassName, Mass);
			node.Add(LinearDragName, LinearDrag);
			node.Add(AngularDragName, AngularDrag);
			node.Add(GravityScaleName, GravityScale);
			node.Add(MaterialName, Material.ExportYaml(container));
			node.Add(InterpolateName, (int)Interpolate);
			node.Add(SleepingModeName, (int)SleepingMode);
			node.Add(CollisionDetectionName, (int)CollisionDetection);
			node.Add(ConstraintsName, (int)Constraints);
			return node;
		}

		public RigidbodyType2D BodyType { get; set; }
		public bool Simulated { get; set; }
		public bool UseFullKinematicContacts { get; set; }
		public bool UseAutoMass { get; set; }
		public float Mass { get; set; }
		public float LinearDrag { get; set; }
		public float AngularDrag { get; set; }
		public float GravityScale { get; set; }
		public RigidbodyInterpolation2D Interpolate { get; set; }
		public RigidbodySleepMode2D SleepingMode { get; set; }
		public CollisionDetectionMode2D CollisionDetection { get; set; }
		public RigidbodyConstraints2D Constraints { get; set; }

		public const string BodyTypeName = "m_BodyType";
		public const string SimulatedName = "m_Simulated";
		public const string UseFullKinematicContactsName = "m_UseFullKinematicContacts";
		public const string UseAutoMassName = "m_UseAutoMass";
		public const string MassName = "m_Mass";
		public const string LinearDragName = "m_LinearDrag";
		public const string AngularDragName = "m_AngularDrag";
		public const string GravityScaleName = "m_GravityScale";
		public const string MaterialName = "m_Material";
		public const string InterpolateName = "m_Interpolate";
		public const string SleepingModeName = "m_SleepingMode";
		public const string CollisionDetectionName = "m_CollisionDetection";
		public const string ConstraintsName = "m_Constraints";

		public PPtr<PhysicsMaterial2D> Material = new();
	}
}
