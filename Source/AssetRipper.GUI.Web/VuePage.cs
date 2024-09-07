namespace AssetRipper.GUI.Web;

public abstract class VuePage : DefaultPage
{
	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		OnlineDependencies.Vue.WriteScriptReference(writer);
	}
}
