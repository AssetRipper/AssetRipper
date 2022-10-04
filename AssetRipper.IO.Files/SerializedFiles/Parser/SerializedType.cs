using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser
{
	public sealed class SerializedType : SerializedTypeBase
	{
		public int[] TypeDependencies { get; set; } = Array.Empty<int>();

		protected override bool UseScriptTypeIndex(FormatVersion formatVersion, UnityVersion unityVersion)
		{
			//This code is wrong
			//Needs unit-tested
			//Might depend on whether or not the version was stripped
			//return unityVersion < WriteIDHashForScriptTypeVersion;
			return true;
		}

		protected override void ReadTypeDependencies(SerializedReader reader)
		{
			TypeDependencies = reader.ReadInt32Array();
		}

		/// <summary>
		/// Unknown
		/// </summary>
		private static UnityVersion WriteIDHashForScriptTypeVersion => default;
	}
}
