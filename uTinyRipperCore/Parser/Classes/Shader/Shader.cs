using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Materials;
using uTinyRipper.Classes.Shaders;
using System.Text;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Converters.Shaders;

namespace uTinyRipper.Classes
{
	public sealed class Shader : TextAsset
	{
		public Shader(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
		
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsSerialized(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.3.0 to 5.4.0
		/// </summary>
		public static bool IsEncoded(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool IsReadFallback(Version version)
		{
			return version.IsLess(2);
		}
		/// <summary>
		/// Less than 3.2.0
		/// </summary>
		public static bool IsReadDefaultProperties(Version version)
		{
			return version.IsLess(3, 2);
		}
		/// <summary>
		/// 2.0.0 to 3.0.0 exclusive
		/// </summary>
		public static bool IsReadStaticProperties(Version version)
		{
			return version.IsGreaterEqual(2) && version.IsLess(3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadDependencies(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadNonModifiableTextures(Version version)
		{
			return version.IsGreaterEqual(2018);
		}		
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadShaderIsBaked(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 3.4.0 to 5.5.0 exclusive and Not Release
		/// </summary>
		public static bool IsReadErrors(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 4.2.0 and greater and Not Release
		/// </summary>
		public static bool IsReadDefaultTextures(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 4.5.0 and greater and Not Release and Not Buildin
		/// </summary>
		public static bool IsReadCompileInfo(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsBuiltinResources() && version.IsGreaterEqual(4, 5);
		}

		public override void Read(AssetReader reader)
		{
			if (IsSerialized(reader.Version))
			{
				ReadBase(reader);

				ParsedForm.Read(reader);

				Platforms = reader.ReadEnum32Array((t) => (GPUPlatform)t);
				uint[] offsets = reader.ReadUInt32Array();
				uint[] compressedLengths = reader.ReadUInt32Array();
				uint[] decompressedLengths = reader.ReadUInt32Array();
				byte[] compressedBlob = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);

				SubProgramBlobs = new ShaderSubProgramBlob[Platforms.Length];
				using (MemoryStream memStream = new MemoryStream(compressedBlob))
				{
					for(int i = 0; i < Platforms.Length; i++)
					{
						uint offset = offsets[i];
						uint compressedLength = compressedLengths[i];
						uint decompressedLength = decompressedLengths[i];

						memStream.Position = offset;
						byte[] decompressedBuffer = new byte[decompressedLength];
						using (Lz4DecodeStream lz4Stream = new Lz4DecodeStream(memStream, (int)compressedLength))
						{
							lz4Stream.ReadBuffer(decompressedBuffer, 0, decompressedBuffer.Length);
						}

						using (MemoryStream blobMem = new MemoryStream(decompressedBuffer))
						{
							using (AssetReader blobReader = new AssetReader(blobMem, reader.Version, reader.Platform, reader.Flags))
							{
								ShaderSubProgramBlob blob = new ShaderSubProgramBlob();
								blob.Read(blobReader);
								SubProgramBlobs[i] = blob;
							}
						}
					}
				}
			}
			else
			{
				base.Read(reader);
				
				if (IsEncoded(reader.Version))
				{
					uint decompressedSize = reader.ReadUInt32();
					int comressedSize = reader.ReadInt32();
					if (comressedSize > 0 && decompressedSize > 0)
					{
						byte[] subProgramBlob = new byte[comressedSize];
						reader.ReadBuffer(subProgramBlob, 0, comressedSize);

						byte[] decompressedBuffer = new byte[decompressedSize];
						using (MemoryStream memStream = new MemoryStream(subProgramBlob))
						{
							using (Lz4DecodeStream lz4Stream = new Lz4DecodeStream(memStream))
							{
								lz4Stream.ReadBuffer(decompressedBuffer, 0, decompressedBuffer.Length);
							}
						}

						using (MemoryStream memStream = new MemoryStream(decompressedBuffer))
						{
							using (AssetReader blobReader = new AssetReader(memStream, reader.Version, reader.Platform, reader.Flags))
							{
								SubProgramBlob.Read(blobReader);
							}
						}
					}
					reader.AlignStream(AlignType.Align4);
				}

				if (IsReadFallback(reader.Version))
				{
					Fallback.Read(reader);
				}
				if (IsReadDefaultProperties(reader.Version))
				{
					DefaultProperties.Read(reader);
				}
				if (IsReadStaticProperties(reader.Version))
				{
					StaticProperties.Read(reader);
				}
			}
			
			if (IsReadDependencies(reader.Version))
			{
				Dependencies = reader.ReadAssetArray<PPtr<Shader>>();
			}
			if (IsReadNonModifiableTextures(reader.Version))
			{
				NonModifiableTextures = new Dictionary<string, PPtr<Texture>>();
				NonModifiableTextures.Read(reader);
			}
			if (IsReadShaderIsBaked(reader.Version))
			{
				ShaderIsBaked = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}

#if UNIVERSAL
			if (IsReadErrors(reader.Version, reader.Flags))
			{
				Errors = reader.ReadAssetArray<ShaderError>();
			}
			if (IsReadDefaultTextures(reader.Version, reader.Flags))
			{
				DefaultTextures = new Dictionary<string, PPtr<Texture>>();
				DefaultTextures.Read(reader);
			}
			if (IsReadCompileInfo(reader.Version, reader.Flags))
			{
				CompileInfo.Read(reader);
			}
#endif
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			ExportBinary(container, stream, DefaultShaderExporterInstantiator);
		}

		public void ExportBinary(IExportContainer container, Stream stream, Func<Version, GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (IsSerialized(container.Version))
			{
				using (ShaderWriter writer = new ShaderWriter(stream, this, exporterInstantiator))
				{
					ParsedForm.Export(writer);
				}
			}
			else if (IsEncoded(container.Version))
			{
				using (ShaderWriter writer = new ShaderWriter(stream, this, exporterInstantiator))
				{
					string header = Encoding.UTF8.GetString(Script);
					SubProgramBlob.Export(writer, header);
				}
			}
			else
			{
				base.ExportBinary(container, stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (IsReadDependencies(context.Version))
			{
				foreach (Object asset in context.FetchDependencies(Dependencies, DependenciesName))
				{
					yield return asset;
				}
			}
		}

		public static ShaderTextExporter DefaultShaderExporterInstantiator(Version version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.unknown:
					return new ShaderTextExporter();

				case GPUPlatform.openGL:
				case GPUPlatform.gles:
				case GPUPlatform.gles3:
				case GPUPlatform.glcore:
					return new ShaderGLESExporter();

				case GPUPlatform.metal:
					return new ShaderMetalExporter();

				default:
					return new ShaderUnknownExporter(graphicApi);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public override string ExportExtension => "shader";

		public override string ValidName => IsSerialized(File.Version) ? ParsedForm.Name : base.ValidName;

		public GPUPlatform[] Platforms { get; set; }
		public ShaderSubProgramBlob[] SubProgramBlobs { get; set; }
		public PPtr<Shader>[] Dependencies { get; set; }
		public Dictionary<string, PPtr<Texture>> NonModifiableTextures { get; set; }
		public bool ShaderIsBaked { get; private set; }
#if UNIVERSAL
		public ShaderError[] Errors { get; set; }
		public Dictionary<string, PPtr<Texture>> DefaultTextures { get; set; }
#endif

		public const string ErrorsName = "errors";
		public const string DependenciesName = "m_Dependencies";

		public SerializedShader ParsedForm;
		public ShaderSubProgramBlob SubProgramBlob;
		public PPtr<Shader> Fallback;
		public UnityPropertySheet DefaultProperties;
		public UnityPropertySheet StaticProperties;
#if UNIVERSAL
		public ShaderCompilationInfo CompileInfo;
#endif
	}
}
