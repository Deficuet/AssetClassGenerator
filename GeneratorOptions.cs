using AssetManager.TypeTree;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace AssetClassGenerator;

public sealed class GeneratorOptions
{
    public string ClassTpkPath { get; set; }

    public string BuiltinClassNamespace { get; set; }

    public string GameAssemblyPath { get; set; }

    public string MetadataPath { get; set; }

    public string CustomClassNamespace { get; set; }

    public List<CustomClassSource> CustomClassSources { get; set; } = [];

    public static GeneratorOptions Load(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Options file was not found: {fullPath}",
                fullPath
            );
        }

        GeneratorOptions options;
        try
        {
            options = JsonConvert.DeserializeObject<GeneratorOptions>(
                File.ReadAllText(fullPath)
            );
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Failed to parse options file '{fullPath}': {ex.Message}",
                ex
            );
        }

        if (options is null)
        {
            throw new InvalidDataException(
                $"Options file '{fullPath}' does not contain a JSON object."
            );
        }

        options.ResolvePaths(Path.GetDirectoryName(fullPath));
        options.Validate();
        return options;
    }

    private void ResolvePaths(string baseDirectory)
    {
        ClassTpkPath = ResolvePath(ClassTpkPath, baseDirectory);
        GameAssemblyPath = ResolvePath(GameAssemblyPath, baseDirectory);
        MetadataPath = ResolvePath(MetadataPath, baseDirectory);
    }

    private static string ResolvePath(string path, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }
        return Path.GetFullPath(
            Path.IsPathRooted(path) ? path : Path.Combine(baseDirectory, path)
        );
    }

    private void Validate()
    {
        RequireNonEmpty(ClassTpkPath, nameof(ClassTpkPath));
        RequireNonEmpty(BuiltinClassNamespace, nameof(BuiltinClassNamespace));
        RequireValidNamespace(BuiltinClassNamespace, nameof(BuiltinClassNamespace));
        RequireExistingFile(ClassTpkPath, nameof(ClassTpkPath));

        CustomClassSources ??= new List<CustomClassSource>();
        if (CustomClassSources.Count == 0)
        {
            return;
        }

        RequireNonEmpty(GameAssemblyPath, nameof(GameAssemblyPath));
        RequireNonEmpty(MetadataPath, nameof(MetadataPath));
        RequireNonEmpty(CustomClassNamespace, nameof(CustomClassNamespace));
        RequireValidNamespace(CustomClassNamespace, nameof(CustomClassNamespace));
        RequireExistingFile(GameAssemblyPath, nameof(GameAssemblyPath));
        RequireExistingFile(MetadataPath, nameof(MetadataPath));

        for (int i = 0; i < CustomClassSources.Count; ++i)
        {
            CustomClassSource source = CustomClassSources[i];
            if (source is null)
            {
                throw new InvalidDataException(
                    $"CustomClassSources[{i}] must not be null."
                );
            }

            RequireNonEmpty(
                source.AssemblyName,
                $"CustomClassSources[{i}].AssemblyName"
            );
            RequireNonEmpty(
                source.ClassName,
                $"CustomClassSources[{i}].ClassName"
            );

            // Namespace is allowed to be empty for the global namespace.
            source.Namespace ??= string.Empty;
        }
    }

    private static void RequireNonEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException(
                $"Required option '{fieldName}' is missing or empty."
            );
        }
    }

    private static void RequireExistingFile(string path, string fieldName)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException(
                $"File specified by '{fieldName}' does not exist: {path}"
            );
        }
    }

    private static void RequireValidNamespace(string value, string fieldName)
    {
        var compilationUnit = SyntaxFactory.ParseCompilationUnit(
            $"namespace {value}\n{{\n}}\n"
        );
        if (compilationUnit.ContainsDiagnostics
            || compilationUnit.Members.Count != 1
            || compilationUnit.Members[0].Kind() != SyntaxKind.NamespaceDeclaration)
        {
            throw new InvalidDataException(
                $"Option '{fieldName}' is not a valid C# namespace: {value}"
            );
        }
    }
}
