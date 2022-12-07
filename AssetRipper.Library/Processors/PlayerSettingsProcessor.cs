using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.SourceGenerated.Classes.ClassID_129;
using System.Linq;
using System.Reflection;

namespace AssetRipper.Library.Processors
{
	public sealed class PlayerSettingsProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			IUnityObjectBase? asset = gameBundle.Collections
				.FirstOrDefault(collection => collection.Name == "globalgamemanagers")?
				.Assets
				.Values
				.FirstOrDefault(asset => asset.ClassName == "PlayerSettings");

			if (asset == null)
			{
				return;
			}

			SetAllowUnsafeCode(asset, true);
		}

		private static void SetAllowUnsafeCode(IUnityObjectBase asset, bool value)
		{
			FieldInfo? allowUnsafeCodeField = asset.GetType()
				.GetField("m_AllowUnsafeCode", BindingFlags.Instance | BindingFlags.Public);

			allowUnsafeCodeField?.SetValue(asset, value);
		}
	}
}
