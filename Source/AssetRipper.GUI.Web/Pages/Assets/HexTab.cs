using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Import.AssetCreation;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class HexTab : AssetTab
{
	public byte[] Data { get; }

	public string? FileName { get; }

	public override string DisplayName => Localization.AssetTabHex;

	public override string HtmlName => "hex";

	public override bool Enabled => Data.Length > 0;

	public HexTab(IUnityObjectBase asset)
	{
		Data = GetData(asset);
		if (Data.Length > 0)
		{
			FileName = $"{asset.GetBestName()}.dat";
		}
	}

	public override void Write(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			DataSaveButton.Write(writer, FileName, Data);
		}
	}

	private static byte[] GetData(IUnityObjectBase Asset)
	{
		if (Asset is RawDataObject rawData)
		{
			return rawData.RawData;
		}
		else
		{
			MemoryStream stream = new();
			AssetWriter writer = new(stream, Asset.Collection);
			try
			{
				Asset.Write(writer);
			}
			catch (NotSupportedException)
			{
				//This can only happen if an asset type is not fully implemented, like custom injected assets.
				return Array.Empty<byte>();
			}
			return stream.ToArray();
		}
	}
}
