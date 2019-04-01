using System;
using System.Collections;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ParticleSystemRenderers;
using uTinyRipper.Classes.SpriteRenderers;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class ParticleSystemRenderer : Renderer
	{
		public ParticleSystemRenderer(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadMinParticleSize(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadNormalDirection(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadShadowBias(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRenderAlignment(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadFlip(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.5.0b11 and greater
		/// </summary>
		public static bool IsReadUseCustomVertexStreams(Version version)
		{
			return version.IsGreaterEqual(5, 5, 0, VersionType.Beta, 11);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadEnableGPUInstancing(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadApplyActiveColorSpace(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadAllowRoll(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.5.0 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadVertexStreamMask(Version version)
		{
			return version.IsGreaterEqual(5, 5) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadVertexStreams(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadMeshes(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadMaskInteraction(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsModeShort(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsSortModeFirst(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		private static int GetSerializedVersion(Version version)
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
			if (version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2))
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RenderMode = IsModeShort(reader.Version) ? (ParticleSystemRenderMode)reader.ReadUInt16() : (ParticleSystemRenderMode)reader.ReadInt32();
			if (IsSortModeFirst(reader.Version))
			{
				SortMode = (ParticleSystemSortMode)reader.ReadUInt16();
			}

			if (IsReadMinParticleSize(reader.Version))
			{
				MinParticleSize = reader.ReadSingle();
			}
			MaxParticleSize = reader.ReadSingle();
			CameraVelocityScale = reader.ReadSingle();
			VelocityScale = reader.ReadSingle();
			LengthScale = reader.ReadSingle();
			SortingFudge = reader.ReadSingle();

			if (IsReadNormalDirection(reader.Version))
			{
				NormalDirection = reader.ReadSingle();
			}
			if (IsReadShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			if (!IsSortModeFirst(reader.Version))
			{
				SortMode = (ParticleSystemSortMode)reader.ReadInt32();
			}

			if (IsReadRenderAlignment(reader.Version))
			{
				RenderAlignment = (ParticleSystemRenderSpace)reader.ReadInt32();
				Pivot.Read(reader);
			}
			else
			{
				RenderAlignment = RenderMode == ParticleSystemRenderMode.Mesh ? ParticleSystemRenderSpace.Local : ParticleSystemRenderSpace.View;
			}
			if (IsReadFlip(reader.Version))
			{
				Flip.Read(reader);
			}

			if (IsReadUseCustomVertexStreams(reader.Version))
			{
				UseCustomVertexStreams = reader.ReadBoolean();
				if (IsReadEnableGPUInstancing(reader.Version))
				{
					EnableGPUInstancing = reader.ReadBoolean();
				}
				if (IsReadApplyActiveColorSpace(reader.Version))
				{
					ApplyActiveColorSpace = reader.ReadBoolean();
				}
				if (IsReadAllowRoll(reader.Version))
				{
					AllowRoll = reader.ReadBoolean();
				}
				reader.AlignStream(AlignType.Align4);

			}
			if (IsReadVertexStreamMask(reader.Version))
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
				m_vertexStreams = vertexStreams.ToArray();
			}
			if (IsReadVertexStreams(reader.Version))
			{
				m_vertexStreams = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);
			}

			Mesh.Read(reader);
			if (IsReadMeshes(reader.Version))
			{
				Mesh1.Read(reader);
				Mesh2.Read(reader);
				Mesh3.Read(reader);
			}
			if (IsReadMaskInteraction(reader.Version))
			{
				MaskInteraction = (SpriteMaskInteraction)reader.ReadInt32();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Mesh.FetchDependency(file, isLog, ToLogString, MeshName);
			if (IsReadMeshes(file.Version))
			{
				yield return Mesh1.FetchDependency(file, isLog, ToLogString, Mesh1Name);
				yield return Mesh2.FetchDependency(file, isLog, ToLogString, Mesh2Name);
				yield return Mesh3.FetchDependency(file, isLog, ToLogString, Mesh3Name);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
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

		private float GetNormalDirection(Version version)
		{
			return IsReadNormalDirection(version) ? NormalDirection : 1.0f;
		}
		private IReadOnlyList<byte> GetVertexStreams(Version version)
		{
			return IsReadUseCustomVertexStreams(version) ? VertexStreams : new byte[] { 0, 1, 3, 4, 5 };
		}

		public ParticleSystemRenderMode RenderMode { get; private set; }
		public ParticleSystemSortMode SortMode { get; private set; }
		public float MinParticleSize { get; private set; }
		public float MaxParticleSize { get; private set; }
		public float CameraVelocityScale { get; private set; }
		public float VelocityScale { get; private set; }
		public float LengthScale { get; private set; }
		public float SortingFudge { get; private set; }
		public float NormalDirection { get; private set; }
		public float ShadowBias { get; private set; }
		public ParticleSystemRenderSpace RenderAlignment { get; private set; }
		public bool UseCustomVertexStreams { get; private set; }
		public bool EnableGPUInstancing { get; private set; }
		public bool ApplyActiveColorSpace { get; private set; }
		public bool AllowRoll { get; private set; }
		public IReadOnlyList<byte> VertexStreams => m_vertexStreams;
		public SpriteMaskInteraction MaskInteraction { get; private set; }

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
		public PPtr<Mesh> Mesh;
		public PPtr<Mesh> Mesh1;
		public PPtr<Mesh> Mesh2;
		public PPtr<Mesh> Mesh3;

		private byte[] m_vertexStreams;
	}
}
