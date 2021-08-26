using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.VersionHandlers
{
	public class UnityHandler
	{
		public UnityVersion UnityVersion { get; }
		public MeshHandler MeshHandler { get; }

		private Assembly Assembly { get; }
		private string Namespace { get; }

		public UnityHandler(Assembly assembly, string @namespace, UnityVersion version)
		{
			Assembly = assembly;
			Namespace = @namespace;
			UnityVersion = version;

			MeshHandler = new MeshHandler(this, GetMeshType());
		}

		private Type GetMeshType() => throw new NotImplementedException();
	}
}
