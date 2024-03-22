using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class FontTab : HtmlTab
{
	public bool Validate { get; }

	public byte[] Data { get; }

	public string Source { get; }

	public string? FileName { get; }

	public string? MimeType { get; }

	public override string DisplayName => Localization.AssetTabFont;

	public override string HtmlName => "font";

	public override bool Enabled => Validate && Data.Length > 0;

	public FontTab(IUnityObjectBase asset)
	{
		Validate = TryDecode(asset, out byte[] data, out string source, out string? fileName, out string? mimeType);
		Source = source;
		Data = data;
		FileName = fileName;
		MimeType = mimeType;
	}

	public override void Write(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			new H1(writer).WithStyle($"font-family: {FileName}").Close("Preview Font (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)");

			DataSaveButton.Write(writer, $"{FileName}.{MimeType}", Data);

			new Script(writer).Close(
				$$"""
				  const fontFace = new FontFace(`{{FileName}}`, `url({{Source}})`);
				  document.fonts.add(fontFace);
				  fontFace.load().then().catch(function(error) {
				    console.error(`Font loading failed: ${error}`);
				  });
				  """);
		}
	}

	private static bool TryDecode(IUnityObjectBase asset, out byte[] data, out string source,out string? fileName, out string? mimeType)
	{
		if (asset is IFont font)
		{
			data = font.FontData;

			if (data is { Length: >= 4 })
			{
				fileName = font.GetBestName();
				(mimeType, string fontName) = (data[0], data[1], data[2], data[3]) switch
				{
					(0x4F, 0x54, 0x54, 0x4F) => ("otf", "opentype"),
					(0x00, 0x01, 0x00, 0x00) => ("ttf", "truetype"),
					(0x74, 0x74, 0x63, 0x66) => ("ttc", "collection"),
					_ => (string.Empty, string.Empty),
				};
				source = $"data:font/{fontName};base64,{Convert.ToBase64String(data, Base64FormattingOptions.None)}";

				return true;
			}
		}

		data = Array.Empty<byte>();
		source = string.Empty;
		fileName = null;
		mimeType = null;

		return false;
	}
}
