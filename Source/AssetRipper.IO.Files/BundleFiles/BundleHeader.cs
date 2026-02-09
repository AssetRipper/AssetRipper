using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Web;
using AssetRipper.IO.Files.Streams.Smart;
using System.Diagnostics;

namespace AssetRipper.IO.Files.BundleFiles;

public abstract record class BundleHeader : IEndianWritable
{
	internal BundleHeader() { }
	protected abstract string MagicString { get; }
	public BundleVersion Version { get; set; }
	/// <summary>
	/// Generation version
	/// </summary>
	public string? UnityWebBundleVersion { get; set; }
	/// <summary>
	/// Actual engine version
	/// </summary>
	public string? UnityWebMinimumRevision { get; set; }

	public virtual void Read(EndianReader reader)
	{
		string signature = reader.ReadStringZeroTerm();
		Debug.Assert(signature == MagicString);
		Version = (BundleVersion)reader.ReadInt32();
		Debug.Assert(Version >= 0);
		UnityWebBundleVersion = reader.ReadStringZeroTerm();
		UnityWebMinimumRevision = reader.ReadStringZeroTerm();
	}

	public virtual void Write(EndianWriter writer)
	{
		writer.WriteStringZeroTerm(MagicString);
		writer.Write((int)Version);
		writer.WriteStringZeroTerm(UnityWebBundleVersion ?? throw new NullReferenceException(nameof(UnityWebBundleVersion)));
		writer.WriteStringZeroTerm(UnityWebMinimumRevision ?? throw new NullReferenceException(nameof(UnityWebMinimumRevision)));
	}

	private protected static bool IsBundleHeader(EndianReader reader, string magicString)
	{
		const int MaxLength = 0x20;
		if (reader.BaseStream.Length >= MaxLength)
		{
			long position = reader.BaseStream.Position;
			bool isRead = reader.ReadStringZeroTerm(MaxLength, out string? signature);
			reader.BaseStream.Position = position;
			return isRead && signature == magicString;
		}
		return false;
	}

	public static bool IsBundleHeader(string path, FileSystem fileSystem)
	{
		using SmartStream stream = SmartStream.OpenRead(path, fileSystem);
		using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
		return FileStreamBundleHeader.IsBundleHeader(reader)
			|| RawBundleHeader.IsBundleHeader(reader)
			|| WebBundleHeader.IsBundleHeader(reader);
	}
}
