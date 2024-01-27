using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Import.AssetCreation;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class HexTab : HtmlTab
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
		return (Asset as RawDataObject)?.RawData ?? [];
	}
}
