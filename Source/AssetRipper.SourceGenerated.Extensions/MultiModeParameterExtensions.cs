using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.MultiModeParameter;
using AssetRipper.SourceGenerated.Subclasses.MultiModeParameter_MeshSpawn;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MultiModeParameterExtensions
{
	public static ParticleSystemShapeMultiModeValue GetMode(this IMultiModeParameter parameter)
	{
		return (ParticleSystemShapeMultiModeValue)parameter.Mode;
	}

	public static void SetValues(this IMultiModeParameter parameter, UnityVersion version, float value)
	{
		parameter.Value = value;
		parameter.Mode = (int)ParticleSystemShapeMultiModeValue.Random;
		parameter.Spread = 0.0f;
		parameter.Speed.SetValues(version, 1.0f);
	}

	public static ParticleSystemShapeMultiModeValue GetMode(this IMultiModeParameter_MeshSpawn parameter)
	{
		return (ParticleSystemShapeMultiModeValue)parameter.Mode;
	}

	public static void SetValues(this IMultiModeParameter_MeshSpawn parameter, UnityVersion version)
	{
		parameter.Mode = (int)ParticleSystemShapeMultiModeValue.Random;
		parameter.Spread = 0.0f;
		parameter.Speed.SetValues(version, 1.0f);
	}
}
