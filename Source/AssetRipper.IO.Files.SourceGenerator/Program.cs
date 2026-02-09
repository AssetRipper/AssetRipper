using System.Reflection;
using System.Text;

namespace AssetRipper.IO.Files.SourceGenerator;

internal static class Program
{
	public const string SourceDirectory = "../../../../";
	public const string GeneratorProjectDirectory = SourceDirectory + "AssetRipper.IO.Files.SourceGenerator/";
	public const string OutputDirectory = SourceDirectory + "AssetRipper.IO.Files/";
	static void Main(string[] args)
	{
		SerializedFileClassGenerator.GenerateSerializedFileClasses();
		WriteFileSystemClass();
		WriteLocalFileSystemClass();
		WriteVirtualFileSystemClass();
	}

	/// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>
	private static void WriteFileSystemClass()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(OutputDirectory, "FileSystem");
		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.IO.Files");
		writer.WriteLineNoTabs();
		writer.WriteLine("public abstract partial class FileSystem");
		using (new CurlyBrackets(writer))
		{
			foreach ((string classPropertyName, List<FileSystemApi> classApiList) in apiDictionary)
			{
				writer.WriteLine($"public abstract {classPropertyName}Implementation {classPropertyName} {{ get; }}");
				writer.WriteLineNoTabs();
				writer.WriteLine($"public abstract partial class {classPropertyName}Implementation(FileSystem fileSystem)");
				using (new CurlyBrackets(writer))
				{
					writer.WriteLine("protected FileSystem Parent { get; } = fileSystem;");
					foreach (string otherClassPropertyName in apiDictionary.Keys)
					{
						if (otherClassPropertyName != classPropertyName)
						{
							writer.WriteLine($"protected {otherClassPropertyName}Implementation {otherClassPropertyName} => Parent.{otherClassPropertyName};");
						}
						else
						{
							writer.WriteLine($"protected {otherClassPropertyName}Implementation {otherClassPropertyName} => this;");
						}
					}

					foreach (FileSystemApi api in classApiList)
					{
						// Inherit documentation from System.IO
						writer.WriteLine($"/// <inheritdoc cref=\"{api.FullName}({api.ParametersWithoutNames})\"/>");

						string virtualKeyword = api.Type is FileSystemApiType.Sealed ? "" : "virtual ";
						writer.WriteLine($"public {virtualKeyword}{api.BaseReturnType} {api.Name}({api.ParametersWithTypes})");
						using (new CurlyBrackets(writer))
						{
							if (api.Type is FileSystemApiType.Throw)
							{
								writer.WriteLine("throw new global::System.NotSupportedException();");
							}
							else
							{
								string returnKeyword = api.VoidReturn ? "" : "return ";
								writer.WriteLine($"{returnKeyword}{api.FullName}({api.ParametersWithoutTypes});");
							}
						}
						writer.WriteLineNoTabs();
					}
					writer.WriteComment("Override methods below to provide custom implementation");
					writer.WriteLine("public sealed override string ToString() => base.ToString();");
					writer.WriteLine("public sealed override bool Equals(object obj) => base.Equals(obj);");
					writer.WriteLine("public sealed override int GetHashCode() => base.GetHashCode();");
				}
				writer.WriteLineNoTabs();
			}
		}
	}

	private static void WriteLocalFileSystemClass()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(OutputDirectory, "LocalFileSystem");
		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.IO.Files");
		writer.WriteLineNoTabs();
		writer.WriteLine("public sealed partial class LocalFileSystem : FileSystem");
		using (new CurlyBrackets(writer))
		{
			foreach ((string classPropertyName, List<FileSystemApi> classApiList) in apiDictionary)
			{
				writer.WriteLine($"public override Local{classPropertyName}Implementation {classPropertyName} {{ get; }}");
				writer.WriteLineNoTabs();
				writer.WriteLine($"public sealed partial class Local{classPropertyName}Implementation(LocalFileSystem fileSystem) : {classPropertyName}Implementation(fileSystem)");
				using (new CurlyBrackets(writer))
				{
					foreach (FileSystemApi api in classApiList)
					{
						if (api.Type is not FileSystemApiType.Throw)
						{
							continue;
						}

						writer.WriteLine($"public override {api.DerivedReturnType} {api.Name}({api.ParametersWithTypes})");
						using (new CurlyBrackets(writer))
						{
							string returnKeyword = api.VoidReturn ? "" : "return ";
							writer.WriteLine($"{returnKeyword}{api.FullName}({api.ParametersWithoutTypes});");
						}
						writer.WriteLineNoTabs();
					}
				}
				writer.WriteLineNoTabs();
			}

			writer.WriteLine("public LocalFileSystem()");
			using (new CurlyBrackets(writer))
			{
				foreach (string classPropertyName in apiDictionary.Keys)
				{
					writer.WriteLine($"{classPropertyName} = new(this);");
				}
			}
		}
	}

	private static void WriteVirtualFileSystemClass()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(OutputDirectory, "VirtualFileSystem");
		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.IO.Files");
		writer.WriteLineNoTabs();
		writer.WriteLine("public sealed partial class VirtualFileSystem : FileSystem");
		using (new CurlyBrackets(writer))
		{
			foreach ((string classPropertyName, List<FileSystemApi> classApiList) in apiDictionary)
			{
				writer.WriteLine($"public override Virtual{classPropertyName}Implementation {classPropertyName} {{ get; }}");
				writer.WriteLineNoTabs();
				writer.WriteLine($"public sealed partial class Virtual{classPropertyName}Implementation(VirtualFileSystem fileSystem) : {classPropertyName}Implementation(fileSystem)");
				using (new CurlyBrackets(writer))
				{
					writer.WriteLine("private new VirtualFileSystem Parent => (VirtualFileSystem)base.Parent;");
					foreach (string otherClassPropertyName in apiDictionary.Keys)
					{
						if (otherClassPropertyName != classPropertyName)
						{
							writer.WriteLine($"private new Virtual{otherClassPropertyName}Implementation {otherClassPropertyName} => Parent.{otherClassPropertyName};");
						}
						else
						{
							writer.WriteLine($"private new Virtual{otherClassPropertyName}Implementation {otherClassPropertyName} => this;");
						}
					}
				}
				writer.WriteLineNoTabs();
			}

			writer.WriteLine("public VirtualFileSystem()");
			using (new CurlyBrackets(writer))
			{
				foreach (string classPropertyName in apiDictionary.Keys)
				{
					writer.WriteLine($"{classPropertyName} = new(this);");
				}
			}
		}
	}

	private static readonly Dictionary<string, List<FileSystemApi>> apiDictionary = new()
	{
		[nameof(File)] =
		[
			new((Func<string, FileStream>)File.Create),
			new(File.Delete),
			new(File.Exists),
			new(File.OpenRead),
			new(File.OpenWrite),
			new(File.ReadAllBytes),
			new((Func<string, string>)File.ReadAllText),
			new((Func<string, Encoding, string>)File.ReadAllText),
			new((Action<string, ReadOnlySpan<byte>>)File.WriteAllBytes),
			new((Action<string, ReadOnlySpan<char>>)File.WriteAllText),
			new((Action<string, ReadOnlySpan<char>, Encoding>)File.WriteAllText),
		],
		[nameof(Directory)] =
		[
			new((Func<string, string, SearchOption, IEnumerable<string>>)Directory.EnumerateDirectories),
			new((Func<string, string, SearchOption, IEnumerable<string>>)Directory.EnumerateFiles),
			new((Func<string, string, SearchOption, IEnumerable<string>>)Directory.GetDirectories),
			new((Func<string, string, SearchOption, IEnumerable<string>>)Directory.GetFiles),
			new((Func<string, string, IEnumerable<string>>)Directory.EnumerateDirectories),
			new((Func<string, string, IEnumerable<string>>)Directory.EnumerateFiles),
			new((Func<string, string, IEnumerable<string>>)Directory.GetDirectories),
			new((Func<string, string, IEnumerable<string>>)Directory.GetFiles),
			new((Func<string, IEnumerable<string>>)Directory.EnumerateDirectories),
			new((Func<string, IEnumerable<string>>)Directory.EnumerateFiles),
			new((Func<string, IEnumerable<string>>)Directory.GetDirectories),
			new((Func<string, IEnumerable<string>>)Directory.GetFiles),
			new(Directory.Exists),
		],
		[nameof(Path)] =
		[
			new((Func<string, string, string>)Path.Join) { Type = FileSystemApiType.Virtual },
			new((Func<string, string, string, string>)Path.Join) { Type = FileSystemApiType.Virtual },
			new((Func<string, string, string, string, string>)Path.Join) { Type = FileSystemApiType.Virtual },
			new((Func<ReadOnlySpan<string?>, string>)Path.Join) { Type = FileSystemApiType.Virtual },
			new((Func<ReadOnlySpan<char>, ReadOnlySpan<char>>)Path.GetDirectoryName) { Type = FileSystemApiType.Sealed },
			new((Func<string?, string?>)Path.GetDirectoryName) { Type = FileSystemApiType.Sealed },
			new((Func<ReadOnlySpan<char>, ReadOnlySpan<char>>)Path.GetExtension) { Type = FileSystemApiType.Sealed },
			new((Func<string?, string?>)Path.GetExtension) { Type = FileSystemApiType.Sealed },
			new((Func<ReadOnlySpan<char>, ReadOnlySpan<char>>)Path.GetFileName) { Type = FileSystemApiType.Sealed },
			new((Func<string, string>)Path.GetFileName) { Type = FileSystemApiType.Sealed },
			new((Func<ReadOnlySpan<char>, ReadOnlySpan<char>>)Path.GetFileNameWithoutExtension) { Type = FileSystemApiType.Sealed },
			new((Func<string, string>)Path.GetFileNameWithoutExtension) { Type = FileSystemApiType.Sealed },
			new((Func<string, string>)Path.GetFullPath),
			new(Path.GetRelativePath) { Type = FileSystemApiType.Sealed },
			new((Func<ReadOnlySpan<char>, bool>)Path.IsPathRooted),
		],
	};

	private enum FileSystemApiType
	{
		Throw,
		Virtual,
		Sealed,
	}

	private sealed record class FileSystemApi
	{
		public required Delegate Delegate { get; init; }
		public MethodInfo MethodInfo => Delegate.Method;
		public FileSystemApiType Type { get; init; } = FileSystemApiType.Throw;
		public string DeclaringType => MethodInfo.DeclaringType!.GetGlobalQualifiedName();
		public string BaseReturnType
		{
			get
			{
				Type returnType = MethodInfo.ReturnType == typeof(FileStream) ? typeof(Stream) : MethodInfo.ReturnType;
				return returnType.GetGlobalQualifiedName();
			}
		}
		public string DerivedReturnType => MethodInfo.ReturnType.GetGlobalQualifiedName();
		public bool VoidReturn => MethodInfo.ReturnType == typeof(void);
		public string Name => MethodInfo.Name;
		public string FullName => $"{DeclaringType}.{Name}";
		public IEnumerable<(string, string)> Parameters => MethodInfo.GetParameters()
			.Select(parameter => (parameter.GetParamsPrefix() + parameter.ParameterType.GetGlobalQualifiedName(), parameter.Name!));
		public string ParametersWithTypes => string.Join(", ", Parameters.Select(parameter => $"{parameter.Item1} {parameter.Item2}"));
		public string ParametersWithoutTypes => string.Join(", ", Parameters.Select(parameter => parameter.Item2));
		public string ParametersWithoutNames => string.Join(", ", Parameters.Select(parameter => parameter.Item1));

		public FileSystemApi()
		{
		}

		[SetsRequiredMembers]
		public FileSystemApi(Delegate @delegate)
		{
			Delegate = @delegate;
		}
	}
}
