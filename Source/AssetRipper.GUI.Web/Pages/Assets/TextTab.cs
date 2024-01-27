using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Decompilation.CSharp;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class TextTab : HtmlTab
{
	public string Text { get; }

	public string? FileName { get; }

	public override string DisplayName => Localization.AssetTabText;

	public override string HtmlName => "text";

	public override bool Enabled => !string.IsNullOrEmpty(Text);

	public TextTab(IUnityObjectBase asset)
	{
		Text = TryGetText(asset);
		if (Enabled)
		{
			FileName = GetFileName(asset);
		}
	}

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Text);
		using (new Div(writer).WithClass("text-center").End())
		{
			TextSaveButton.Write(writer, FileName, Text);
		}
	}

	public static void Write(TextWriter writer, string? fileName, string? text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(text);
			using (new Div(writer).WithClass("text-center").End())
			{
				TextSaveButton.Write(writer, fileName, text);
			}
		}
	}

	public static string TryGetText(IUnityObjectBase asset)
	{
		return asset switch
		{
			IShader shader => DumpShaderDataAsText(shader),
			IMonoScript monoScript => DecompileMonoScript(monoScript),
			ITextAsset textAsset => textAsset.Script_C49,
			_ => "",
		};
	}

	public static string GetFileName(IUnityObjectBase asset)
	{
		return asset switch
		{
			IShader => $"{asset.GetBestName()}.shader",
			IMonoScript monoScript => $"{monoScript.ClassName_R}.cs",
			ITextAsset textAsset => $"{asset.GetBestName()}.{GetTextAssetExtension(textAsset)}",
			_ => $"{asset.GetBestName()}.txt",
		};

		static string GetTextAssetExtension(ITextAsset textAsset)
		{
			return string.IsNullOrEmpty(textAsset.OriginalExtension) ? "txt" : textAsset.OriginalExtension;
		}
	}

	private static string DumpShaderDataAsText(IShader shader)
	{
		InvariantStringWriter writer = new();
		DummyShaderTextExporter.ExportShader(shader, writer);
		return writer.ToString();
	}

	private static string DecompileMonoScript(IMonoScript monoScript)
	{
		IAssemblyManager assemblyManager = GameFileLoader.AssemblyManager;
		if (!monoScript.IsScriptPresents(assemblyManager))
		{
			return EmptyScript.GetContent(monoScript);
		}
		else
		{
			try
			{
				TypeDefinition type = monoScript.GetTypeDefinition(assemblyManager);
				return CSharpDecompiler.Decompile(type);
			}
			catch (Exception ex)
			{
				return $"{Localization.AnErrorOccuredDuringDecompilation}\n\n{ex}";
			}
		}
	}
}
