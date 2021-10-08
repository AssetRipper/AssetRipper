using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystemRenderer
{
	public sealed class ParticleSystemRenderer : Renderer.Renderer
	{
		public ParticleSystemRenderer(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// m_ApplyActiveColorSpace previous value is false
			if (version.IsGreaterEqual(2018, 2))
			{
				return 6;
			}
			// EnableGPUInstancing previous value is false
			if (version.IsGreaterEqual(2018))
			{
				return 5;
			}
			if (version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2))
			{
				return 4;
			}
			if (version.IsGreaterEqual(5, 6))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasMinParticleSize(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasNormalDirection(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasShadowBias(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRenderAlignment(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasFlip(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.5.0b11 and greater
		/// </summary>
		public static bool HasUseCustomVertexStreams(UnityVersion version) => version.IsGreaterEqual(5, 5, 0, UnityVersionType.Beta, 11);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasEnableGPUInstancing(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasApplyActiveColorSpace(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasAllowRoll(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasStretching(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 5.5.0 to 5.6.0 exclusive
		/// </summary>
		public static bool HasVertexStreamMask(UnityVersion version) => version.IsGreaterEqual(5, 5) && version.IsLess(5, 6);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasVertexStreams(UnityVersion version) => version.IsGreaterEqual(5, 6);

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasMeshes(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasMaskInteraction(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);
		
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasMeshDistribution(UnityVersion version) => version.IsGreaterEqual(2021, 2);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasMeshWeighting(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsModeShort(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsSortModeFirst(UnityVersion version) => version.IsGreaterEqual(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RenderMode = IsModeShort(reader.Version) ? (ParticleSystemRenderMode)reader.ReadUInt16() : (ParticleSystemRenderMode)reader.ReadInt32();

			if (HasMeshDistribution(reader.Version))
			{
				reader.ReadByte(); //meshdistribution
				SortMode = (ParticleSystemSortMode)reader.ReadByte();
			}
			else if (IsSortModeFirst(reader.Version))
			{
				SortMode = (ParticleSystemSortMode)reader.ReadUInt16();
			}

			if (HasMinParticleSize(reader.Version))
			{
				MinParticleSize = reader.ReadSingle();
			}
			MaxParticleSize = reader.ReadSingle();
			CameraVelocityScale = reader.ReadSingle();
			VelocityScale = reader.ReadSingle();
			LengthScale = reader.ReadSingle();
			SortingFudge = reader.ReadSingle();

			if (HasNormalDirection(reader.Version))
			{
				NormalDirection = reader.ReadSingle();
			}
			if (HasShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			if (!IsSortModeFirst(reader.Version))
			{
				SortMode = (ParticleSystemSortMode)reader.ReadInt32();
			}

			if (HasRenderAlignment(reader.Version))
			{
				RenderAlignment = (ParticleSystemRenderSpace)reader.ReadInt32();
				Pivot.Read(reader);
			}
			else
			{
				RenderAlignment = RenderMode == ParticleSystemRenderMode.Mesh ? ParticleSystemRenderSpace.Local : ParticleSystemRenderSpace.View;
			}
			if (HasFlip(reader.Version))
			{
				Flip.Read(reader);
			}

			if (HasUseCustomVertexStreams(reader.Version))
			{
				UseCustomVertexStreams = reader.ReadBoolean();
				if (HasEnableGPUInstancing(reader.Version))
				{
					EnableGPUInstancing = reader.ReadBoolean();
				}
				if (HasApplyActiveColorSpace(reader.Version))
				{
					ApplyActiveColorSpace = reader.ReadBoolean();
				}
				if (HasAllowRoll(reader.Version))
				{
					AllowRoll = reader.ReadBoolean();
				}

				if (HasStretching(reader.Version))
				{
					FreeformStretching = reader.ReadBoolean();
					RotateWithStretchDirection = reader.ReadBoolean();
				}
				reader.AlignStream();

			}
			if (HasVertexStreamMask(reader.Version))
			{
				int vertexStreamMask = reader.ReadInt32();
				List<byte> vertexStreams = new List<byte>(8);
				for (byte i = 0; i < 8; i++)
				{
					if ((vertexStreamMask & (1 << i)) != 0)
					{
						vertexStreams.Add(i);
					}
				}
				VertexStreams = vertexStreams.ToArray();
			}
			if (HasVertexStreams(reader.Version))
			{
				VertexStreams = reader.ReadByteArray();
				reader.AlignStream();
			}

			Mesh.Read(reader);
			if (HasMeshes(reader.Version))
			{
				Mesh1.Read(reader);
				Mesh2.Read(reader);
				Mesh3.Read(reader);
			}

			if (HasMeshWeighting(reader.Version))
			{
				reader.ReadSingle(); //0
				reader.ReadSingle(); //1
				reader.ReadSingle(); //2
				reader.ReadSingle(); //3
			}
			if (HasMaskInteraction(reader.Version))
			{
				MaskInteraction = (SpriteMaskInteraction)reader.ReadInt32();
			}
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Mesh, MeshName);
			if (HasMeshes(context.Version))
			{
				yield return context.FetchDependency(Mesh1, Mesh1Name);
				yield return context.FetchDependency(Mesh2, Mesh2Name);
				yield return context.FetchDependency(Mesh3, Mesh3Name);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RenderModeName, (short)RenderMode);
			node.Add(SortModeName, (short)SortMode);
			node.Add(MinParticleSizeName, MinParticleSize);
			node.Add(MaxParticleSizeName, MaxParticleSize);
			node.Add(CameraVelocityScaleName, CameraVelocityScale);
			node.Add(VelocityScaleName, VelocityScale);
			node.Add(LengthScaleName, LengthScale);
			node.Add(SortingFudgeName, SortingFudge);
			node.Add(NormalDirectionName, GetNormalDirection(container.Version));
			node.Add(RenderAlignmentName, (int)RenderAlignment);
			node.Add(PivotName, Pivot.ExportYAML(container));
			node.Add(UseCustomVertexStreamsName, UseCustomVertexStreams);
			node.Add(VertexStreamsName, GetVertexStreams(container.Version).ExportYAML());
			node.Add(MeshName, Mesh.ExportYAML(container));
			node.Add(Mesh1Name, Mesh1.ExportYAML(container));
			node.Add(Mesh2Name, Mesh2.ExportYAML(container));
			node.Add(Mesh3Name, Mesh3.ExportYAML(container));
			node.Add(MaskInteractionName, (int)MaskInteraction);
			return node;
		}

		private float GetNormalDirection(UnityVersion version)
		{
			return HasNormalDirection(version) ? NormalDirection : 1.0f;
		}
		private byte[] GetVertexStreams(UnityVersion version)
		{
			return HasUseCustomVertexStreams(version) ? VertexStreams : new byte[] { 0, 1, 3, 4, 5 };
		}

		public ParticleSystemRenderMode RenderMode { get; set; }
		public ParticleSystemSortMode SortMode { get; set; }
		public float MinParticleSize { get; set; }
		public float MaxParticleSize { get; set; }
		public float CameraVelocityScale { get; set; }
		public float VelocityScale { get; set; }
		public float LengthScale { get; set; }
		public float SortingFudge { get; set; }
		public float NormalDirection { get; set; }
		public float ShadowBias { get; set; }
		public ParticleSystemRenderSpace RenderAlignment { get; set; }
		public bool UseCustomVertexStreams { get; set; }
		public bool EnableGPUInstancing { get; set; }
		public bool ApplyActiveColorSpace { get; set; }
		public bool AllowRoll { get; set; }
		public bool FreeformStretching { get; set; }
		public bool RotateWithStretchDirection { get; set; }
		public byte[] VertexStreams { get; set; }
		public SpriteMaskInteraction MaskInteraction { get; set; }

		public const string RenderModeName = "m_RenderMode";
		public const string SortModeName = "m_SortMode";
		public const string MinParticleSizeName = "m_MinParticleSize";
		public const string MaxParticleSizeName = "m_MaxParticleSize";
		public const string CameraVelocityScaleName = "m_CameraVelocityScale";
		public const string VelocityScaleName = "m_VelocityScale";
		public const string LengthScaleName = "m_LengthScale";
		public const string SortingFudgeName = "m_SortingFudge";
		public const string NormalDirectionName = "m_NormalDirection";
		public const string RenderAlignmentName = "m_RenderAlignment";
		public const string PivotName = "m_Pivot";
		public const string UseCustomVertexStreamsName = "m_UseCustomVertexStreams";
		public const string VertexStreamsName = "m_VertexStreams";
		public const string MeshName = "m_Mesh";
		public const string Mesh1Name = "m_Mesh1";
		public const string Mesh2Name = "m_Mesh2";
		public const string Mesh3Name = "m_Mesh3";
		public const string MaskInteractionName = "m_MaskInteraction";

		public Vector3f Pivot;
		public Vector3f Flip;
		public PPtr<Mesh.Mesh> Mesh;
		public PPtr<Mesh.Mesh> Mesh1;
		public PPtr<Mesh.Mesh> Mesh2;
		public PPtr<Mesh.Mesh> Mesh3;
	}
}
