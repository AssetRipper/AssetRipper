using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Exceptions
{
	public sealed class UnbackedPropertyException : Exception
	{
		public IUnityAssetBase Asset { get; }
		public string PropertyName { get; }

		public UnbackedPropertyException(IUnityAssetBase asset, string propertyName)
		{
			Asset = asset;
			PropertyName = propertyName;
		}

		public override string Message => $"{Asset.GetType().Name}.{PropertyName} is not backed by a field.";
	}
}
