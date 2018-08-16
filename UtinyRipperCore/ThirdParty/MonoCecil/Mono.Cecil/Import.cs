//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace Mono.Cecil {

	static partial class Mixin {

		public static void CheckModule (ModuleDefinition module)
		{
			if (module == null)
				throw new ArgumentNullException (Argument.module.ToString ());
		}

		public static bool TryGetAssemblyNameReference (this ModuleDefinition module, AssemblyNameReference name_reference, out AssemblyNameReference assembly_reference)
		{
			var references = module.AssemblyReferences;

			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];
				if (!Equals (name_reference, reference))
					continue;

				assembly_reference = reference;
				return true;
			}

			assembly_reference = null;
			return false;
		}

		static bool Equals (byte [] a, byte [] b)
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a == null)
				return false;
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++)
				if (a [i] != b [i])
					return false;
			return true;
		}

		static bool Equals<T> (T a, T b) where T : class, IEquatable<T>
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a == null)
				return false;
			return a.Equals (b);
		}

		static bool Equals (AssemblyNameReference a, AssemblyNameReference b)
		{
			if (ReferenceEquals (a, b))
				return true;
			if (a.Name != b.Name)
				return false;
			if (!Equals (a.Version, b.Version))
				return false;
			if (a.Culture != b.Culture)
				return false;
			if (!Equals (a.PublicKeyToken, b.PublicKeyToken))
				return false;
			return true;
		}
	}
}
