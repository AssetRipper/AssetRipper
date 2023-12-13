namespace AssetRipper.Web.Content;

public abstract class HtmlPage : WebContent
{
	public override void Write(TextWriter writer) => writer.Write("<!DOCTYPE html>");

	public sealed override string ContentType => "text/html";
}
