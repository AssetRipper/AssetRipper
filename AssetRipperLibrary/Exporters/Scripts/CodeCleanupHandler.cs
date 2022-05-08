using ICSharpCode.Decompiler.CSharp;
using AssetRipper.Library.Exporters.Scripts.Transforms;

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

		public void SetupDecompiler(CSharpDecompiler decompiler)
		{
			if (Settings.RemoveInvalidMembers)
			{
				decompiler.AstTransforms.Add(new RemoveInvalidMemberTransform());
			}

			if (Settings.EnsureOutParametersSet)
			{
				decompiler.AstTransforms.Add(new EnsureOutParamsSetTransform());
			}

			if (Settings.EnsureStructFieldsSetInConstructor)
			{
				decompiler.AstTransforms.Add(new EnsureStructFieldsSetTransform());
			}

			if (Settings.EnsureValidBaseConstructor)
			{
				decompiler.AstTransforms.Add(new EnsureValidBaseConstructorTransform());
			}
		}
	}
}
