//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using Mono.Cecil.Cil;

namespace Mono.Cecil {

	static partial class Mixin {

		public static bool TryGetUniqueDocument (this MethodDebugInformation info, out Document document)
		{
			document = info.SequencePoints [0].Document;

			for (int i = 1; i < info.SequencePoints.Count; i++) {
				var sequence_point = info.SequencePoints [i];
				if (sequence_point.Document != document)
					return false;
			}

			return true;
		}
	}
}
