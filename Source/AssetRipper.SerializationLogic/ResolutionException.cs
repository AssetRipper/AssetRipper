namespace AssetRipper.SerializationLogic;

internal class ResolutionException : Exception
{
	public ResolutionException()
	{
	}

	public ResolutionException(IFullNameProvider reference) : base($"Could not resolve {reference.FullName}")
	{
	}
}
