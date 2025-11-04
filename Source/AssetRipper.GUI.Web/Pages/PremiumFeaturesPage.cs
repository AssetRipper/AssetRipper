using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.GUI.Web.Pages;

public sealed class PremiumFeaturesPage : DefaultPage
{
	public static PremiumFeaturesPage Instance { get; } = new();

	public override string? GetTitle() => Localization.PremiumFeatures;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new Table(writer).WithClass("table").End())
		{
			using (new Tbody(writer).End())
			{
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.ShaderDecompilation);
					new Td(writer).Close(GetFeatureStatus<IShader>(null, ShaderDecompilationSupported) switch
					{
						FeatureStatus.NoFilesLoaded => Localization.NoFilesLoaded,
						FeatureStatus.NoAssetsOfType => Localization.NoShadersFound,
						FeatureStatus.NoAssetsNeedFeature => null,
						FeatureStatus.NoAssetsAreSupported => Localization.NotSupported,
						FeatureStatus.Supported => Localization.Supported,
						_ => null,
					});
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.StaticMeshSeparation);
					new Td(writer).Close(GetFeatureStatus<IMesh>(mesh => mesh.IsCombinedMesh(), null) switch
					{
						FeatureStatus.NoFilesLoaded => Localization.NoFilesLoaded,
						FeatureStatus.NoAssetsOfType => Localization.NoMeshesFound,
						FeatureStatus.NoAssetsNeedFeature => Localization.NoStaticMeshesFound,
						FeatureStatus.NoAssetsAreSupported => Localization.NotSupported,
						FeatureStatus.Supported => Localization.Supported,
						_ => null,
					});
				}
			}
		}
	}

	private static bool ShaderDecompilationSupported(IShader shader)
	{
		return shader.GetPlatforms()?.Any(platform => platform is GPUPlatform.vulkan || (platform.IsDirectX() && OperatingSystem.IsWindows())) ?? false;
	}

	private static FeatureStatus GetFeatureStatus<T>(Func<T, bool>? needsFeatureFunction, Func<T, bool>? isSupportedFunction)
		where T : class
	{
		if (!GameFileLoader.IsLoaded)
		{
			return FeatureStatus.NoFilesLoaded;
		}

		bool hasAssetsOfType = false;
		bool hasAssetsNeedingFeature = false;
		bool supportedAssetFound = false;
		foreach (T asset in GameFileLoader.GameBundle.FetchAssets().OfType<T>())
		{
			hasAssetsOfType = true;
			if (needsFeatureFunction?.Invoke(asset) ?? true)
			{
				hasAssetsNeedingFeature = true;
				if (isSupportedFunction?.Invoke(asset) ?? true)
				{
					supportedAssetFound = true;
					break;
				}
			}
		}
		if (!hasAssetsOfType)
		{
			return FeatureStatus.NoAssetsOfType;
		}
		if (!hasAssetsNeedingFeature)
		{
			return FeatureStatus.NoAssetsNeedFeature;
		}
		return supportedAssetFound ? FeatureStatus.Supported : FeatureStatus.NoAssetsAreSupported;
	}

	private enum FeatureStatus
	{
		NoFilesLoaded,
		NoAssetsOfType,
		NoAssetsNeedFeature,
		NoAssetsAreSupported,
		Supported,
	}
}
