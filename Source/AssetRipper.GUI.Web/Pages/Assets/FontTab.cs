using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class FontTab : AssetTab
{
	public byte[] Data { get; }

	public string? FileName { get; }

	public override string DisplayName => Localization.AssetTabFont;

	public override string HtmlName => "font";

	public override bool Enabled => Data.Length > 0;

	public FontTab(IUnityObjectBase asset)
	{
		if (asset is IFont font)
		{
			Data = font.FontData;
			if (Data is { Length: >= 4 })
			{
				FileName = $"{font.GetBestName()}.{(Data[0], Data[1], Data[2], Data[3]) switch
				{
					(0x4F, 0x54, 0x54, 0x4F) => "otf",
					(0x00, 0x01, 0x00, 0x00) => "ttf",
					(0x74, 0x74, 0x63, 0x66) => "ttc",
					_ => "",
				}}";
			}
		}
		else
		{
			Data = [];
		}
	}

	public override void Write(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			DataSaveButton.Write(writer, FileName, Data);
		}
	}
}
