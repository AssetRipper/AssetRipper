using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.VersionHandling
{
	public static class VersionManager
	{
		private static readonly Dictionary<UnityVersion, UnityHandlerBase> handlers = new(); 
		public static UnityHandlerBase LegacyHandler { get; set; }
		public static UnityHandlerBase SpecialHandler { get; set; }

		public static UnityHandlerBase GetHandler(UnityVersion version)
		{
			if(SpecialHandler != null)
			{
				return SpecialHandler;
			}
			else if(handlers.TryGetValue(version, out UnityHandlerBase handler))
			{
				return handler;
			}
			else if(LegacyHandler != null)
			{
				return LegacyHandler;
			}
			else
			{
				throw new NotSupportedException("No handler for that version");
			}
		}
	}
}
