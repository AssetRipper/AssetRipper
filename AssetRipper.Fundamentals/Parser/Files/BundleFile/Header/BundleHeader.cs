using AssetRipper.Core.Parser.Files.BundleFile.Parser;
using AssetRipper.IO.Endian;

namespace AssetRipper.Core.Parser.Files.BundleFile.Header
{
	public sealed class BundleHeader
	{
		public BundleType Signature { get; set; }
		public BundleVersion Version { get; set; }
		/// <summary>
		/// Generation version
		/// </summary>
		public string? UnityWebBundleVersion { get; set; }
		/// <summary>
		/// Actual engine version
		/// </summary>
		public UnityVersion UnityWebMinimumRevision { get; set; }
		public BundleFlags Flags
		{
			get
			{
				if (Signature == BundleType.UnityFS)
				{
					return FileStream!.Flags;
				}
				return 0;
			}
		}

		public BundleRawWebHeader? RawWeb { get; set; }
		public BundleFileStreamHeader? FileStream { get; set; }

		public void Read(EndianReader reader)
		{
			string signature = reader.ReadStringZeroTerm();
			Signature = ParseSignature(signature);
			Version = (BundleVersion)reader.ReadInt32();
			UnityWebBundleVersion = reader.ReadStringZeroTerm();
			string engineVersion = reader.ReadStringZeroTerm();
			UnityWebMinimumRevision = UnityVersion.Parse(engineVersion);

			switch (Signature)
			{
				case BundleType.UnityRaw:
				case BundleType.UnityWeb:
					RawWeb = new BundleRawWebHeader(reader, Version);//ReadHeaderAndBlocksInfo
					break;
				case BundleType.UnityFS:
					FileStream = new BundleFileStreamHeader(reader);//ReadHeader
					break;
				case BundleType.UnityArchive:
					throw new NotSupportedException("UnityArchives are not currently supported");
				default:
					throw new Exception($"Unknown bundle signature '{Signature}'");
			}
		}

		public static BundleType ParseSignature(string signature)
		{
			return TryParseSignature(signature, out BundleType bundleType)
				? bundleType
				: throw new ArgumentException($"Unsupported signature '{signature}'");
		}

		public static bool TryParseSignature(string signatureString, out BundleType type)
		{
			switch (signatureString)
			{
				case nameof(BundleType.UnityWeb):
					type = BundleType.UnityWeb;
					return true;
				case nameof(BundleType.UnityRaw):
					type = BundleType.UnityRaw;
					return true;
				case nameof(BundleType.UnityFS):
					type = BundleType.UnityFS;
					return true;
				case nameof(BundleType.UnityArchive):
					type = BundleType.UnityArchive;
					return true;
				default:
					type = default;
					return false;
			}
		}

		internal static bool IsBundleHeader(EndianReader reader)
		{
			const int MaxLength = 0x20;
			if (reader.BaseStream.Length >= MaxLength)
			{
				long position = reader.BaseStream.Position;
				bool isRead = reader.ReadStringZeroTerm(MaxLength, out string? signature);
				reader.BaseStream.Position = position;
				if (isRead)
				{
					return TryParseSignature(signature!, out BundleType _);
				}
			}
			return false;
		}
	}
}
