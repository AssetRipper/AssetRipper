using System;

namespace AssetRipper.Core.VersionHandlers
{
	/// <summary>
	/// Holds reflection info on meshes for a Unity version
	/// </summary>
	public class MeshHandler
	{
		public UnityHandler UnityHandler { get; }

		public MeshHandler(UnityHandler unityHandler, Type meshType)
		{
			UnityHandler = unityHandler;

			//Reflection on important members
		}
	}
}
