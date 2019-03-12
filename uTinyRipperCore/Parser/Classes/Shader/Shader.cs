using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Materials;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Shaders.Exporters;
using System.Text;

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
		/// 5.3.0 to 5.4.x
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

		public override void Read(AssetReader reader)
		{
			if(IsSerialized(reader.Version))
			{
				ReadBase(reader);

				ParsedForm.Read(reader);

				m_platforms = reader.ReadEnum32Array((t) => (GPUPlatform)t);
				uint[] offsets = reader.ReadUInt32Array();
				uint[] compressedLengths = reader.ReadUInt32Array();
				uint[] decompressedLengths = reader.ReadUInt32Array();
				byte[] compressedBlob = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);

				m_subProgramBlobs = new ShaderSubProgramBlob[m_platforms.Length];
				using (MemoryStream memStream = new MemoryStream(compressedBlob))
				{
					for(int i = 0; i < m_platforms.Length; i++)
					{
						uint offset = offsets[i];
						uint compressedLength = compressedLengths[i];
						uint decompressedLength = decompressedLengths[i];

						memStream.Position = offset;
						byte[] decompressedBuffer = new byte[decompressedLength];
						using (Lz4DecodeStream lz4Stream = new Lz4DecodeStream(memStream, (int)compressedLength))
						{
							int read = lz4Stream.Read(decompressedBuffer, 0, decompressedBuffer.Length);
							if (read != decompressedLength)
							{
								throw new Exception($"Can't properly decode shader blob. Read {read} but expected {decompressedLength}");
							}
						}

						using (MemoryStream blobMem = new MemoryStream(decompressedBuffer))
						{
							using (AssetReader blobReader = new AssetReader(blobMem, reader.Version, reader.Platform, reader.Flags))
							{
								ShaderSubProgramBlob blob = new ShaderSubProgramBlob();
								blob.Read(blobReader);
								m_subProgramBlobs[i] = blob;
							}
						}
					}
				}
			}
			else
			{
				base.Read(reader);
				
				if(IsEncoded(reader.Version))
				{
					uint decompressedSize = reader.ReadUInt32();
					int comressedSize = reader.ReadInt32();

					byte[] subProgramBlob = new byte[comressedSize];
					reader.Read(subProgramBlob, 0, comressedSize);
					reader.AlignStream(AlignType.Align4);

					if (comressedSize > 0 && decompressedSize > 0)
					{
						byte[] decompressedBuffer = new byte[decompressedSize];
						using (MemoryStream memStream = new MemoryStream(subProgramBlob))
						{
							using (Lz4DecodeStream lz4Stream = new Lz4DecodeStream(memStream))
							{
								int read = lz4Stream.Read(decompressedBuffer, 0, decompressedBuffer.Length);
								if (read != decompressedSize)
								{
									throw new Exception($"Can't properly decode sub porgram blob. Read {read} but expected {decompressedSize}");
								}
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
				m_dependencies = reader.ReadAssetArray<PPtr<Shader>>();
			}
			if(IsReadNonModifiableTextures(reader.Version))
			{
				m_nonModifiableTextures = reader.ReadAssetArray<PPtr<Texture>>();
			}
			if (IsReadShaderIsBaked(reader.Version))
			{
				ShaderIsBaked = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			ExportBinary(container, stream, DefaultShaderExporterInstantiator);
		}

		public void ExportBinary(IExportContainer container, Stream stream, Func<Version, ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			if (IsReadDependencies(file.Version))
			{
				foreach (PPtr<Shader> shader in Dependencies)
				{
					yield return shader.FetchDependency(file, isLog, ToLogString, "m_dependencies");
				}
			}
		}

		public static ShaderTextExporter DefaultShaderExporterInstantiator(Version version, ShaderGpuProgramType programType)
		{
			if(programType.IsGL())
			{
				return new ShaderGLESExporter();
			}
			if(programType.IsMetal())
			{
				return new ShaderMetalExporter(version);
			}
			return new ShaderUnknownExporter(programType);
		}

		public override string ExportExtension => "shader";

		public override string ValidName => IsSerialized(File.Version) ? ParsedForm.Name : base.ValidName;

		public IReadOnlyList<GPUPlatform> Platforms => m_platforms;
		public IReadOnlyList<ShaderSubProgramBlob> SubProgramBlobs => m_subProgramBlobs;
		public IReadOnlyList<PPtr<Shader>> Dependencies => m_dependencies;
		public IReadOnlyList<PPtr<Texture>> NonModifiableTextures => m_nonModifiableTextures;
		public bool ShaderIsBaked { get; private set; }
		
		public SerializedShader ParsedForm;
		public ShaderSubProgramBlob SubProgramBlob;
		public PPtr<Shader> Fallback;
		public UnityPropertySheet DefaultProperties;
		public UnityPropertySheet StaticProperties;
		
		private GPUPlatform[] m_platforms;
		private ShaderSubProgramBlob[] m_subProgramBlobs;
		private PPtr<Shader>[] m_dependencies;
		private PPtr<Texture>[] m_nonModifiableTextures;
	}
}
