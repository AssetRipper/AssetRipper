using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
