using ICSharpCode.Decompiler.CSharp;
using AssetRipper.Library.Exporters.Scripts.Transforms;
using AssetRipper.Core.Logging;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Handles setting up decompilers to clean up code based on given settings.
	/// </summary>
	internal class CodeCleanupHandler
	{
		public CodeCleanupHandler(CodeCleanupSettings? settings = null)
		{
			this.Settings = settings ?? new CodeCleanupSettings();
		}

		/// <summary>
		/// Settings for code cleanup.
		/// </summary>
		public CodeCleanupSettings Settings { get; }

		public void SetupDecompiler(WholeAssemblyDecompiler decompiler)
		{
			if (Settings.RemoveInvalidMembers)
			{
				decompiler.CustomTransforms.Add(new RemoveInvalidMemberTransform());
			}

			if (Settings.EnsureOutParametersSet)
			{
				decompiler.CustomTransforms.Add(new EnsureOutParamsSetTransform());
			}

			if (Settings.EnsureStructFieldsSetInConstructor)
			{
				decompiler.CustomTransforms.Add(new EnsureStructFieldsSetTransform());
			}

			if (Settings.EnsureValidBaseConstructor)
			{
				decompiler.CustomTransforms.Add(new EnsureValidBaseConstructorTransform());
			}

			if (Settings.ValidateOptionalParameterValues)
			{
				decompiler.CustomTransforms.Add(new FixOptionalParametersTransform());
			}

			if (Settings.ValidateNullCasts)
			{
				decompiler.CustomTransforms.Add(new ValidateNullCastsTransform());
			}

			if (Settings.FixExplicitInterfaceImplementations)
			{
				decompiler.CustomTransforms.Add(new FixExplicitInterfaceImplementationTransform());
			}
		}
	}
}
