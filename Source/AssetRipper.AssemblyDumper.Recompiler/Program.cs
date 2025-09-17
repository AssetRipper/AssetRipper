using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Recompiler;

internal static class Program
{
	private static int Year { get; } = DateTime.Now.Year;
	private static string? Version { get; set; }
	private static string CsProjContent => $"""
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net9.0</TargetFramework>
		<DisableImplicitNamespaceImports>true</DisableImplicitNamespaceImports>
		<Nullable>enable</Nullable>
		<AotCompatible>true</AotCompatible>
		<RootNamespace>AssetRipper</RootNamespace>

		<AssemblyName>AssetRipper.SourceGenerated</AssemblyName>
		<Copyright>Copyright © {Year}</Copyright>
		<Authors>ds5678</Authors>
		<Company>AssetRipper</Company>
		<Version>{Version}</Version>
		<AssemblyVersion>$(Version)</AssemblyVersion>

		<PackageId>AssetRipper.SourceGenerated</PackageId>
		<PackageTags>C# unity unity3d</PackageTags>
		<RepositoryUrl>https://github.com/AssetRipper/AssetRipper</RepositoryUrl>
		<PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
		<RepositoryType>git</RepositoryType>
		<PackageProjectUrl>https://github.com/AssetRipper/AssetRipper</PackageProjectUrl>
		<Copyright>Copyright (c) {Year} ds5678</Copyright>
		<Description>Internal source generated library for AssetRipper</Description>
		<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
		<DocumentationFile>bin\AssetRipper.SourceGenerated.xml</DocumentationFile>

		<NoWarn>1591,8600,8601,8602,8603,8604</NoWarn>
	</PropertyGroup>

	<ItemGroup>
		<Using Remove="@(Using)" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\..\..\..\AssetRipper.Assets\AssetRipper.Assets.csproj" />
		<ProjectReference Include="..\..\..\..\AssetRipper.Numerics\AssetRipper.Numerics.csproj" />
	</ItemGroup>

</Project>
""";
	static void Main(string[] args)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		string assemblyPath = args[0];
		string outputDirectory = args[1];
		Version = args[2];
		ClearOrCreateDirectory(outputDirectory);
		UniversalAssemblyResolver resolver = new UniversalAssemblyResolver(assemblyPath, true, ".NETCoreApp, Version=9.0");
		WholeProjectDecompiler decompiler = new WholeProjectDecompiler(resolver);
		decompiler.Settings.SetLanguageVersion(LanguageVersion.Latest);
		decompiler.Settings.UseSdkStyleProjectFormat = true;
		decompiler.Settings.UseNestedDirectoriesForNamespaces = true;
		decompiler.ProgressIndicator = new ProgressIndicator();
		decompiler.DecompileProject(new PEFile(assemblyPath), outputDirectory, TextWriter.Null);
		DeleteBadFilesAndDirectories(outputDirectory);
		WriteCsProjFile(outputDirectory);
		stopwatch.Stop();
		Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds / 1000} seconds!");
	}

	private static void DeleteBadFilesAndDirectories(string outputDirectory)
	{
		Directory.Delete(Path.Combine(outputDirectory, "Properties"), true);
	}

	private static void WriteCsProjFile(string outputDirectory)
	{
		File.WriteAllText(Path.Combine(outputDirectory, "AssetRipper.SourceGenerated.csproj"), CsProjContent);
	}

	private static void ClearOrCreateDirectory(string directory)
	{
		if (Directory.Exists(directory))
		{
			Directory.Delete(directory, true);
		}
		Directory.CreateDirectory(directory);
	}

	private sealed class ProgressIndicator : IProgress<DecompilationProgress>
	{
		private readonly Lock lockObject = new();
		public void Report(DecompilationProgress value)
		{
			lock (lockObject)
			{
				Console.WriteLine($"{value.UnitsCompleted}/{value.TotalUnits} File: {value.Status}");
			}
		}
	}
}
