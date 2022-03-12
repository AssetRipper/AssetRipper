using AssetRipper.Core.Classes.Material;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.Blob;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class Shader : TextAsset, IShader
	{
		public Shader(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// double blob arrays (offsets, compressedLengths and decompressedLengths)
			if (version.IsGreaterEqual(2019, 3))
			{
				return 2;
			}
			return 1;
		}

		#region Version methods
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsSerialized(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasBlob(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool HasFallback(UnityVersion version) => version.IsLess(2);
		/// <summary>
		/// Less than 3.2.0
		/// </summary>
		public static bool HasDefaultProperties(UnityVersion version) => version.IsLess(3, 2);
		/// <summary>
		/// 2.0.0 to 3.0.0 exclusive
		/// </summary>
		public static bool HasStaticProperties(UnityVersion version) => version.IsGreaterEqual(2) && version.IsLess(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasDependencies(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasNonModifiableTextures(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasShaderIsBaked(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 3.4.0 to 5.5.0 as well as 2021.2.0a19 and greater while Not Release
		/// </summary>
		public static bool HasErrors(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && ((version.IsGreaterEqual(3, 4) && version.IsLess(5, 5, 0, UnityVersionType.Final, 3)) ||
			        (version.IsGreaterEqual(2021, 2, 0, UnityVersionType.Alpha, 19)));
		}
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public static bool HasDefaultTextures(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 4.5.0 and greater and Not Release and Not Buildin
		/// </summary>
		public static bool HasCompileInfo(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsBuiltinResources() && version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		private static bool IsDoubleArray(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2020.1.0b4 and greater and Not Release
		/// </summary>
		private static bool HasCompileSmokeTest(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2020, 1, 0, UnityVersionType.Beta, 4);
		#endregion

		public override void Read(AssetReader reader)
		{
			if (IsSerialized(reader.Version))
			{
				ReadNamedObject(reader);

				m_ParsedForm.Read(reader);
				Name = m_ParsedForm.Name; // Use the serialized shader name as asset name if available.

				Platforms = reader.ReadArray((t) => (GPUPlatform)t);
				if (IsDoubleArray(reader.Version))
				{
					Offsets2D = reader.ReadUInt32ArrayArray();
					CompressedLengths2D = reader.ReadUInt32ArrayArray();
					DecompressedLengths2D = reader.ReadUInt32ArrayArray();
					CompressedBlob = reader.ReadByteArray();
					reader.AlignStream();

					UnpackSubProgramBlobs(reader.Info, Offsets2D, CompressedLengths2D, DecompressedLengths2D, CompressedBlob);
				}
				else
				{
					Offsets1D = reader.ReadUInt32Array();
					CompressedLengths1D = reader.ReadUInt32Array();
					DecompressedLengths1D = reader.ReadUInt32Array();
					CompressedBlob = reader.ReadByteArray();
					reader.AlignStream();

					UnpackSubProgramBlobs(reader.Info, Offsets1D, CompressedLengths1D, DecompressedLengths1D, CompressedBlob);
				}
			}
			else
			{
				base.Read(reader);

				if (HasBlob(reader.Version))
				{
					DecompressedSize = reader.ReadUInt32();
					CompressedBlob = reader.ReadByteArray();
					reader.AlignStream();

					UnpackSubProgramBlobs(reader.Info, 0, (uint)CompressedBlob.Length, DecompressedSize, CompressedBlob );
				}

				if (HasFallback(reader.Version))
				{
					Fallback.Read(reader);
				}
				if (HasDefaultProperties(reader.Version))
				{
					DefaultProperties.Read(reader);
				}
				if (HasStaticProperties(reader.Version))
				{
					StaticProperties.Read(reader);
				}
			}

			if (HasDependencies(reader.Version))
			{
				Dependencies = reader.ReadAssetArray<PPtr<Shader>>();
			}
			if (HasNonModifiableTextures(reader.Version))
			{
				NonModifiableTextures = new Dictionary<string, PPtr<Texture>>();
				NonModifiableTextures.Read(reader);
			}
			if (HasShaderIsBaked(reader.Version))
			{
				ShaderIsBaked = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasDependencies(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Dependencies, DependenciesName))
				{
					yield return asset;
				}
			}

			if (HasNonModifiableTextures(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(NonModifiableTextures, NonModifiableTexturesName))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node;
			if (IsSerialized(container.ExportVersion))
			{
				node = ExportBaseYAMLRoot(container);
				node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
				node.Add("m_ParsedForm", m_ParsedForm.ExportYAML(container));
				node.Add("platforms", Array.ConvertAll(Platforms, value => (int)value).ExportYAML(false));
				if (IsDoubleArray(container.ExportVersion))
				{
					node.Add("offsets", Offsets2D.ExportYAML(false));
					node.Add("compressedLengths", CompressedLengths2D.ExportYAML(false));
					node.Add("decompressedLengths", DecompressedLengths2D.ExportYAML(false));
				}
				else
				{
					node.Add("offsets", Offsets1D.ExportYAML(false));
					node.Add("compressedLengths", CompressedLengths1D.ExportYAML(false));
					node.Add("decompressedLengths", DecompressedLengths1D.ExportYAML(false));
				}

				node.Add("compressedBlob", CompressedBlob.ExportYAML());
			}
			else //Shader as TextAsset (old format)
			{
				node = base.ExportYAMLRoot(container);
				node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));

				if (HasBlob(container.ExportVersion))
				{
					node.Add("decompressedSize", DecompressedSize);
					node.Add("m_SubProgramBlob", CompressedBlob.ExportYAML());
				}
			}

			if (HasDependencies(container.ExportVersion))
			{
				node.Add(DependenciesName, Dependencies.ExportYAML(container));
			}

			if (HasNonModifiableTextures(container.ExportVersion))
			{
				node.Add(NonModifiableTexturesName, NonModifiableTextures.ExportYAML(container));
			}

			if (HasShaderIsBaked(container.ExportVersion))
			{
				node.Add("m_ShaderIsBaked", ShaderIsBaked);
			}

			//Editor-Only
			if (HasErrors(container.ExportVersion, container.ExportFlags))
			{
				node.Add("errors", (new HashSet<ShaderError>()).ExportYAML(container));
			}

			if (HasDefaultTextures(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_DefaultTextures", (new Dictionary<string, PPtr<Texture>>()).ExportYAML(container));
			}

			if (HasCompileInfo(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_CompileInfo", (new ShaderCompilationInfo()).ExportYAML(container));
			}

			if (HasCompileSmokeTest(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_CompileSmokeTestAfterImport", default(bool));
			}

			return node;
		}

		private void UnpackSubProgramBlobs(LayoutInfo layout, uint offset, uint compressedLength, uint decompressedLength, byte[] compressedBlob)
		{
			if (compressedBlob.Length == 0)
			{
				Blobs = Array.Empty<ShaderSubProgramBlob>();
			}
			else
			{
				Blobs = new ShaderSubProgramBlob[1] { new() };
				uint[] offsets = new uint[] { offset };
				uint[] compressedLengths = new uint[] { compressedLength };
				uint[] decompressedLengths = new uint[] { decompressedLength };
				Blobs[0].Read(layout, compressedBlob, offsets, compressedLengths, decompressedLengths);
			}
		}

		private void UnpackSubProgramBlobs(LayoutInfo layout, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths, byte[] compressedBlob)
		{
			Blobs = new ShaderSubProgramBlob[offsets.Length];
			for (int i = 0; i < Blobs.Length; i++)
			{
				Blobs[i] = new();
				uint[] blobOffsets = new uint[] { offsets[i] };
				uint[] blobCompressedLengths = new uint[] { compressedLengths[i] };
				uint[] blobDecompressedLengths = new uint[] { decompressedLengths[i] };
				Blobs[i].Read(layout, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
		}

		private void UnpackSubProgramBlobs(LayoutInfo layout, uint[][] offsets, uint[][] compressedLengths, uint[][] decompressedLengths, byte[] compressedBlob)
		{
			Blobs = new ShaderSubProgramBlob[offsets.Length];
			for (int i = 0; i < Platforms.Length; i++)
			{
				Blobs[i] = new();
				uint[] blobOffsets = offsets[i];
				uint[] blobCompressedLengths = compressedLengths[i];
				uint[] blobDecompressedLengths = decompressedLengths[i];
				Blobs[i].Read(layout, compressedBlob, blobOffsets, blobCompressedLengths, blobDecompressedLengths);
			}
		}

		public override string ExportExtension => "shader";

		public bool HasParsedForm => IsSerialized(SerializedFile.Version);

		public GPUPlatform[] Platforms { get; set; }
		public ShaderSubProgramBlob[] Blobs { get; set; }

		//2D arrays starting from 2019.3
		public uint[][] Offsets2D { get; set; }
		public uint[][] CompressedLengths2D { get; set; }
		public uint[][] DecompressedLengths2D { get; set; }
		//1D arrays before 2019.3
		public uint[] Offsets1D { get; set; }
		public uint[] CompressedLengths1D { get; set; }
		public uint[] DecompressedLengths1D { get; set; }
		//Not serialized
		public uint DecompressedSize { get; set; }
		public byte[] CompressedBlob { get; set; }

		public PPtr<Shader>[] Dependencies { get; set; }
		public Dictionary<string, PPtr<Texture>> NonModifiableTextures { get; set; }
		public bool ShaderIsBaked { get; set; }

		public const string DependenciesName = "m_Dependencies";
		public const string NonModifiableTexturesName = "m_NonModifiableTextures";

		public SerializedShader.ISerializedShader ParsedForm => m_ParsedForm;
		private SerializedShader.SerializedShader m_ParsedForm = new();
		public PPtr<Shader> Fallback = new();
		public UnityPropertySheet DefaultProperties = new();
		public UnityPropertySheet StaticProperties = new();
	}
}
