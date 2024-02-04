using AssetRipper.Import;

namespace AssetRipper.GUI.Web;

public abstract class VuePage : DefaultPage
{
	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		if (AssetRipperRuntimeInformation.Build.Debug)
		{
			writer.Write("""<script src="https://unpkg.com/vue@3/dist/vue.global.js"></script>""");
		}
		else
		{
			writer.Write("""<script src="https://unpkg.com/vue@3/dist/vue.global.prod.js"></script>""");
		}
	}
}
