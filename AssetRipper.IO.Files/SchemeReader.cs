using AssetRipper.IO.Files.BundleFiles.Archive;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Web;
using AssetRipper.IO.Files.CompressedFiles.Brotli;
using AssetRipper.IO.Files.CompressedFiles.GZip;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.WebFiles;
using System.Collections.Generic;

namespace AssetRipper.IO.Files
{
	public static class SchemeReader
	{
		private static readonly Stack<IScheme> schemes = new()
		{
			new SerializedFileScheme(),
			new GZipFileScheme(),
			new BrotliFileScheme(),
			new WebFileScheme(),
			new ArchiveBundleScheme(),
			new WebBundleScheme(),
			new RawBundleScheme(),
			new FileStreamBundleScheme(),
		};

		public static File LoadFile(string filePath) => LoadFile(filePath, System.IO.Path.GetFileName(filePath));

		public static File LoadFile(string filePath, string fileName)
		{
			SmartStream stream = SmartStream.OpenRead(filePath);
			return ReadFile(stream, filePath, fileName);
		}

		public static File ReadFile(byte[] buffer, string filePath, string fileName)
		{
			SmartStream smartStream = SmartStream.CreateMemory(buffer, 0, buffer.Length, false);
			return ReadFile(smartStream, filePath, fileName);
		}

		public static File ReadFile(SmartStream stream, string filePath, string fileName)
		{
			foreach (IScheme scheme in schemes)
			{
				if (scheme.CanRead(stream))
				{
					return scheme.Read(stream, filePath, fileName);
				}
			}

			return new ResourceFile(stream, filePath, fileName);
		}

		public static File ReadFile(ResourceFile file)
		{
			return ReadFile(file.Stream.CreateReference(), file.FilePath, file.Name);
		}

		/// <summary>
		/// Required for the initialization of <see cref="schemes"/>
		/// </summary>
		private static void Add(this Stack<IScheme> stack, IScheme scheme) => stack.Push(scheme);
	}
}
