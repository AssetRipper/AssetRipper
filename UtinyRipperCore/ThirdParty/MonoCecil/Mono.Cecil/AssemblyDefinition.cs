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
using System.IO;

using Mono.Collections.Generic;

namespace Mono.Cecil {

	public sealed class AssemblyDefinition : ICustomAttributeProvider, ISecurityDeclarationProvider, IDisposable {

		AssemblyNameDefinition name;

		internal ModuleDefinition main_module;
		Collection<ModuleDefinition> modules;
		Collection<CustomAttribute> custom_attributes;
		Collection<SecurityDeclaration> security_declarations;

		public AssemblyNameDefinition Name {
			get { return name; }
			set { name = value; }
		}

		public string FullName {
			get { return name != null ? name.FullName : string.Empty; }
		}

		public MetadataToken MetadataToken {
			get { return new MetadataToken (TokenType.Assembly, 1); }
			set { }
		}

		public Collection<ModuleDefinition> Modules {
			get {
				if (modules != null)
					return modules;

				if (main_module.HasImage)
					return main_module.Read (ref modules, this, (_, reader) => reader.ReadModules ());

				return modules = new Collection<ModuleDefinition> (1) { main_module };
			}
		}

		public ModuleDefinition MainModule {
			get { return main_module; }
		}

		public MethodDefinition EntryPoint {
			get { return main_module.EntryPoint; }
			set { main_module.EntryPoint = value; }
		}

		public bool HasCustomAttributes {
			get {
				if (custom_attributes != null)
					return custom_attributes.Count > 0;

				return this.GetHasCustomAttributes (main_module);
			}
		}

		public Collection<CustomAttribute> CustomAttributes {
			get { return custom_attributes ?? (this.GetCustomAttributes (ref custom_attributes, main_module)); }
		}

		public bool HasSecurityDeclarations {
			get {
				if (security_declarations != null)
					return security_declarations.Count > 0;

				return this.GetHasSecurityDeclarations (main_module);
			}
		}

		public Collection<SecurityDeclaration> SecurityDeclarations {
			get { return security_declarations ?? (this.GetSecurityDeclarations (ref security_declarations, main_module)); }
		}

		internal AssemblyDefinition ()
		{
		}

		public void Dispose ()
		{
			if (this.modules == null) {
				main_module.Dispose ();
				return;
			}

			var modules = this.Modules;
			for (int i = 0; i < modules.Count; i++)
				modules [i].Dispose ();
		}

		public static AssemblyDefinition ReadAssembly (string fileName)
		{
			return ReadAssembly (ModuleDefinition.ReadModule (fileName));
		}

		public static AssemblyDefinition ReadAssembly (string fileName, ReaderParameters parameters)
		{
			return ReadAssembly (ModuleDefinition.ReadModule (fileName, parameters));
		}

		public static AssemblyDefinition ReadAssembly (Stream stream)
		{
			return ReadAssembly (ModuleDefinition.ReadModule (stream));
		}

		public static AssemblyDefinition ReadAssembly (Stream stream, ReaderParameters parameters)
		{
			return ReadAssembly (ModuleDefinition.ReadModule (stream, parameters));
		}

		static AssemblyDefinition ReadAssembly (ModuleDefinition module)
		{
			var assembly = module.Assembly;
			if (assembly == null)
				throw new ArgumentException ();

			return assembly;
		}
		
		public override string ToString ()
		{
			return this.FullName;
		}
	}
}
