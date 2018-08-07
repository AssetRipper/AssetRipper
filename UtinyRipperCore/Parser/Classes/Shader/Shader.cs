using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Materials;
using UtinyRipper.Classes.Shaders;
using UtinyRipper.SerializedFiles;
using UtinyRipper.Classes.Shaders.Exporters;
using System.Text;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			if(IsSerialized(stream.Version))
			{
				ReadBase(stream);

				ParsedForm.Read(stream);
#if DEBUG
				Name = ParsedForm.Name;
#endif

				m_platforms = stream.ReadEnum32Array((t) => (GPUPlatform)t);
				uint[] offsets = stream.ReadUInt32Array();
				uint[] compressedLengths = stream.ReadUInt32Array();
				uint[] decompressedLengths = stream.ReadUInt32Array();
				byte[] compressedBlob = stream.ReadByteArray();
				stream.AlignStream(AlignType.Align4);

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
						using (Lz4Stream lz4Stream = new Lz4Stream(memStream, (int)compressedLength))
						{
							int read = lz4Stream.Read(decompressedBuffer, 0, decompressedBuffer.Length);
							if (read != decompressedLength)
							{
								throw new Exception($"Can't properly decode shader blob. Read {read} but expected {decompressedLength}");
							}
						}

						using (MemoryStream blobMem = new MemoryStream(decompressedBuffer))
						{
							using (AssetStream blobStream = new AssetStream(blobMem, stream.Version, stream.Platform))
							{
								ShaderSubProgramBlob blob = new ShaderSubProgramBlob();
								blob.Read(blobStream);
								m_subProgramBlobs[i] = blob;
							}
						}
					}
				}
			}
			else
			{
				base.Read(stream);
				
				if(IsEncoded(stream.Version))
				{
					uint decompressedSize = stream.ReadUInt32();
					int comressedSize = stream.ReadInt32();

					byte[] subProgramBlob = new byte[comressedSize];
					stream.Read(subProgramBlob, 0, comressedSize);
					stream.AlignStream(AlignType.Align4);

					if (comressedSize > 0 && decompressedSize > 0)
					{
						byte[] decompressedBuffer = new byte[decompressedSize];
						using (MemoryStream memStream = new MemoryStream(subProgramBlob))
						{
							using (Lz4Stream lz4Stream = new Lz4Stream(memStream))
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
							using (AssetStream blobStream = new AssetStream(memStream, stream.Version, stream.Platform))
							{
								SubProgramBlob.Read(blobStream);
							}
						}
					}
				}

				if (IsReadFallback(stream.Version))
				{
					Fallback.Read(stream);
				}
				if (IsReadDefaultProperties(stream.Version))
				{
					DefaultProperties.Read(stream);
				}
				if (IsReadStaticProperties(stream.Version))
				{
					StaticProperties.Read(stream);
				}
			}
			
			if (IsReadDependencies(stream.Version))
			{
				m_dependencies = stream.ReadArray<PPtr<Shader>>();
			}
			if(IsReadNonModifiableTextures(stream.Version))
			{
				m_nonModifiableTextures = stream.ReadArray<PPtr<Texture>>();
			}
			if (IsReadShaderIsBaked(stream.Version))
			{
				ShaderIsBaked = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			ExportBinary(container, stream, DefaultShaderExporterInstantiator);
		}

		public void ExportBinary(IExportContainer container, Stream stream, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			if (IsSerialized(container.Version))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					ParsedForm.Export(writer, this, exporterInstantiator);
				}
			}
			else if (IsEncoded(container.Version))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					string header = Encoding.UTF8.GetString(Script);
					SubProgramBlob.Export(writer, header, exporterInstantiator);
				}
			}
			else
			{
				base.ExportBinary(container, stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if (IsReadDependencies(file.Version))
			{
				foreach (PPtr<Shader> shader in Dependencies)
				{
					yield return shader.FetchDependency(file, isLog, ToLogString, "m_dependencies");
				}
			}
		}

		public static ShaderTextExporter DefaultShaderExporterInstantiator(ShaderGpuProgramType programType)
		{
			if(programType.IsGL())
			{
				return new ShaderGLESExporter();
			}
			if(programType.IsMetal())
			{
				return new ShaderMetalExporter();
			}
			return new ShaderUnknownExporter(programType);
		}

		public override string ExportExtension => "shader";

		public string ShaderName => IsSerialized(File.Version) ? ParsedForm.Name : Name;

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
