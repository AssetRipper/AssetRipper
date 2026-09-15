using AssetRipper.Assets;
using AssetRipper.Export.PrimaryContent;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Processing.Extended.Deduplication;

/// <summary>
/// Hashes an asset by its serialized content.
/// </summary>
/// <remarks>
/// The hash covers PPtr fields too, so two assets that reference different things never
/// collide. That biases the result toward false negatives (a duplicate pair that is missed)
/// and away from false positives (two distinct assets wrongly merged). Missing a duplicate
/// costs disk space; merging distinct assets corrupts the export, so this is the safe bias.
/// </remarks>
public static class AssetContentHasher
{
	/// <summary>
	/// Streams the asset's serialized form into the hash rather than materializing it.
	/// A single Texture2D can serialize to megabytes of text.
	/// </summary>
	private sealed class HashingTextWriter(IncrementalHash hash) : TextWriter
	{
		private readonly byte[] buffer = new byte[512];

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(char value)
		{
			Span<char> chars = [value];
			int written = Encoding.UTF8.GetBytes(chars, buffer);
			hash.AppendData(buffer, 0, written);
		}

		public override void Write(string? value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			int maxBytes = Encoding.UTF8.GetMaxByteCount(value.Length);
			if (maxBytes <= buffer.Length)
			{
				int written = Encoding.UTF8.GetBytes(value, buffer);
				hash.AppendData(buffer, 0, written);
			}
			else
			{
				hash.AppendData(Encoding.UTF8.GetBytes(value));
			}
		}
	}

	public static string Compute(IUnityObjectBase asset)
	{
		using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

		// The class id participates so two different types can never share a hash.
		hash.AppendData(BitConverter.GetBytes(asset.ClassID));

		using (HashingTextWriter writer = new(hash))
		{
			asset.WalkStandard(new DefaultJsonWalker(writer));
			writer.Flush();
		}

		return System.Convert.ToHexString(hash.GetHashAndReset());
	}
}
