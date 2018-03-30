using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UtinyRipper.Classes;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets, .unity3d, but may also have no extension at all
	/// </summary>
	internal class SerializedFile : ISerializedFile
	{
		public SerializedFile(IAssetCollection collection, string filePath, string fileName)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			Collection = collection;
			FilePath = filePath;
			Name = fileName.ToLower();
			
			Header = new SerializedFileHeader(Name);
			Metadata = new SerializedFileMetadata(Name);
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsTableAtTheEnd(FileGeneration generation)
		{
			return generation <= FileGeneration.FG_300_342;
		}

		public void Load(string assetPath)
		{
			if (!File.Exists(assetPath))
			{
				throw new Exception($"Asset at path '{assetPath}' doesn't exist");
			}

			using (FileStream stream = File.OpenRead(assetPath))
			{
				Read(stream);
				if (stream.Position != stream.Length)
				{
					//throw new Exception($"Read {read} but expected {m_length}");
				}
			}
		}

		public void Read(byte[] buffer)
		{
			using (MemoryStream memStream = new MemoryStream(buffer))
			{
				Read(memStream);
				if (memStream.Position != buffer.Length)
				{
					//throw new Exception($"Read {read} but expected {m_length}");
				}
			}
		}

		public void Read(Stream baseStream)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long startPosition = baseStream.Position;
				Header.Read(stream);

				stream.EndianType = Header.Endianess ? EndianType.BigEndian : EndianType.LittleEndian;
				if (IsTableAtTheEnd(Header.Generation))
				{
					stream.BaseStream.Position = startPosition + Header.FileSize - Header.MetadataSize;
					stream.BaseStream.Position++;
				}
				
				using (SerializedFileStream fileStream = new SerializedFileStream(stream, Header.Generation))
				{
					Metadata.Read(fileStream);
				}

				if (RTTIClassHierarchyDescriptor.IsReadSignature(Header.Generation))
				{
					ReadAssets(stream, startPosition);
				}
				else
				{
					Logger.Log(LogType.Warning, LogCategory.Import, $"Can't determine file version for generation {Header.Generation} for file '{Name}'");
					string[] versions = GetGenerationVersions(Header.Generation);
					for (int i = 0; i < versions.Length; i++)
					{
						string version = versions[i];
						Logger.Log(LogType.Debug, LogCategory.Import, $"Try parse {nameof(SerializedFile)} as {version} version");
						Version.Parse(version);
						try
						{
							ReadAssets(stream, startPosition);
							break;
						}
						catch
						{
							Logger.Log(LogType.Debug, LogCategory.Import, "Faild");
							if (i == versions.Length - 1)
							{
								throw;
							}
						}
					}
				}
			}
		}

		public Classes.Object GetObject(int fileIndex, long pathID)
		{
			return FindObject(fileIndex, pathID, false);
		}

		public Classes.Object GetObject(long pathID)
		{
			Classes.Object @object = FindObject(pathID);
			if(@object == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}

			return @object;
		}

		public Classes.Object FindObject(int fileIndex, long pathID)
		{
			return FindObject(fileIndex, pathID, true);
		}

		public Classes.Object FindObject(long pathID)
		{
			if(m_assets.TryGetValue(pathID, out Classes.Object asset))
			{
				return asset;
			}
			return null;
		}

		public ObjectInfo GetObjectInfo(long pathID)
		{
			return Metadata.Objects[pathID];
		}

		public ClassIDType GetClassID(long pathID)
		{
			ObjectInfo info = Metadata.Objects[pathID];
			if (ObjectInfo.IsReadTypeIndex(Header.Generation))
			{
				return Metadata.Hierarchy.Types[info.TypeIndex].ClassID;
			}
			else
			{
				return info.ClassID;
			}
		}

		public IEnumerable<Classes.Object> FetchAssets()
		{
			foreach(Classes.Object asset in m_assets.Values)
			{
				yield return asset;
			}
		}

		public override string ToString()
		{
			return Name;
		}
		
		private Classes.Object FindObject(int fileIndex, long pathID, bool isSafe)
		{
			ISerializedFile file;
			if (fileIndex == 0)
			{
				file = this;
			}
			else
			{
				fileIndex--;
				if (fileIndex >= Metadata.Dependencies.Count)
				{
					throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies");
				}

				FileIdentifier fileRef = Metadata.Dependencies[fileIndex];
				file = Collection.FindSerializedFile(fileRef);
			}

			if (file == null)
			{
				if(isSafe)
				{
					return null;
				}
				throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in collection");
			}

			Classes.Object @object = file.FindObject(pathID);
			if (@object == null)
			{
				if(isSafe)
				{
					return null;
				}
				throw new Exception($"Object with path ID {pathID} was not found");
			}
			return @object;
		}
		
		private void ReadAssets(EndianStream stream, long startPosition)
		{
			m_assets.Clear();
			using (AssetStream ustream = new AssetStream(stream.BaseStream, Version, Platform))
			{
				foreach (ObjectInfo info in Metadata.Objects.Values)
				{
					AssetInfo assetInfo;
					if (ObjectInfo.IsReadTypeIndex(Header.Generation))
					{
						RTTIBaseClassDescriptor typemeta = Metadata.Hierarchy.Types[info.TypeIndex];
						assetInfo = new AssetInfo(this, info.PathID, typemeta.ClassID);
					}
					else
					{
						assetInfo = new AssetInfo(this, info.PathID, (ClassIDType)info.ClassID);
					}
					
					long pathID = info.PathID;
					Classes.Object asset = ReadAsset(ustream, assetInfo, startPosition + Header.DataOffset + info.DataOffset, info.DataSize);
					if(asset != null)
					{
						m_assets.Add(pathID, asset);
					}
				}

			}
		}

		private static string[] GetGenerationVersions(FileGeneration generation)
		{
			if (generation < FileGeneration.FG_120_200)
			{
				return new[] { "1.2.2" };
			}

			switch (generation)
			{
				case FileGeneration.FG_120_200:
					return new[] { "2.0.0", "1.6.0", "1.5.0", "1.2.2"};
				case FileGeneration.FG_210_261:
					return new[] { "2.6.1", "2.6.0", "2.5.1",  "2.5.0", "2.1.0", };
				case FileGeneration.FG_300b:
					return new[] { "3.0.0b1" };
				default:
					throw new NotSupportedException();
			}
		}

		private static Classes.Object ReadAsset(AssetStream stream, AssetInfo assetInfo, long offset, int size)
		{
			Classes.Object asset = null;
			switch (assetInfo.ClassID)
			{
				case ClassIDType.GameObject:
					asset = new GameObject(assetInfo);
					break;

				case ClassIDType.Transform:
					asset = new Transform(assetInfo);
					break;

				case ClassIDType.Material:
					asset = new Material(assetInfo);
					break;

				case ClassIDType.MeshRenderer:
					asset = new MeshRenderer(assetInfo);
					break;

				case ClassIDType.Texture2D:
					asset = new Texture2D(assetInfo);
					break;

				case ClassIDType.MeshFilter:
					asset = new MeshFilter(assetInfo);
					break;

				case ClassIDType.Mesh:
					asset = new Mesh(assetInfo);
					break;

				case ClassIDType.Shader:
					asset = new Shader(assetInfo);
					break;

				case ClassIDType.TextAsset:
					asset = new TextAsset(assetInfo);
					break;

				case ClassIDType.AnimationClip:
					asset = new AnimationClip(assetInfo);
					break;

				case ClassIDType.AudioClip:
					asset = new AudioClip(assetInfo);
					break;

				case ClassIDType.Cubemap:
					asset = new Cubemap(assetInfo);
					break;

				case ClassIDType.Avatar:
					asset = new Avatar(assetInfo);
					break;

				case ClassIDType.AnimatorController:
					asset = new AnimatorController(assetInfo);
					break;

				case ClassIDType.Animator:
					asset = new Animator(assetInfo);
					break;

				case ClassIDType.Light:
					asset = new Light(assetInfo);
					break;

				case ClassIDType.Animation:
					asset = new Animation(assetInfo);
					break;

				case ClassIDType.MonoScript:
					asset = new MonoScript(assetInfo);
					break;

				case ClassIDType.NewAnimationTrack:
					asset = new NewAnimationTrack(assetInfo);
					break;

				case ClassIDType.Font:
					asset = new Font(assetInfo);
					break;

				case ClassIDType.SkinnedMeshRenderer:
					asset = new SkinnedMeshRenderer(assetInfo);
					break;

				case ClassIDType.BuildSettings:
					asset = new BuildSettings(assetInfo);
					break;

				case ClassIDType.AssetBundle:
					asset = new AssetBundle(assetInfo);
					break;

				/*case ClassIDType.ParticleSystem:
					asset = new ParticleSystem(assetInfo);
					break;*/

				case ClassIDType.SpriteRenderer:
					asset = new SpriteRenderer(assetInfo);
					break;

				case ClassIDType.Sprite:
					asset = new Sprite(assetInfo);
					break;

				case ClassIDType.AnimatorOverrideController:
					asset = new AnimatorOverrideController(assetInfo);
					break;

				case ClassIDType.CanvasRenderer:
					asset = new CanvasRenderer(assetInfo);
					break;

				case ClassIDType.Canvas:
					asset = new Canvas(assetInfo);
					break;

				case ClassIDType.RectTransform:
					asset = new RectTransform(assetInfo);
					break;

				case ClassIDType.SpriteAtlas:
					asset = new SpriteAtlas(assetInfo);
					break;

				default:
					return null;
			}

			stream.BaseStream.Position = offset;
			if (Config.IsGenerateGUIDByContent)
			{
				byte[] data = stream.ReadBytes(size);
				asset.Read(data);

				using (MD5 md5 = MD5.Create())
				{
					byte[] md5Hash = md5.ComputeHash(data);
					assetInfo.GUID = new UtinyGUID(md5Hash);
				}
			}
			else
			{
				stream.AlignPosition = offset;
				asset.Read(stream);
				long read = stream.BaseStream.Position - offset;
				if (read != size)
				{
					throw new Exception($"Read {read} but expected {size} for object {asset.GetType().Name}");
				}
			}
			return asset;
		}

		public string Name { get; }
		public string FilePath { get; }
		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public Version Version => Metadata.Hierarchy.Version;
		public Platform Platform => Metadata.Hierarchy.Platform;

		public IReadOnlyCollection<Classes.Object> Assets => m_assets.Values;
		public IAssetCollection Collection { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Dependencies;
		
		private readonly Dictionary<long, Classes.Object> m_assets = new Dictionary<long, Classes.Object>();
	}
}
