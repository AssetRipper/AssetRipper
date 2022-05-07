using AssetRipper.Core.Attributes;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetRipper.Core.VersionHandling
{
	public static class VersionManager
	{
		public static UnityHandlerBase LegacyHandler { get; set; }

		public static UnityHandlerBase GetHandler(UnityVersion version)
		{
			return LegacyHandler;
		}
	}
}
