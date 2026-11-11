using AssetClassGenerator.TypeTree;
using AssetParser.TypeTreeUtils;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace AssetClassGenerator;

public class MainClass
{
    private const string CLASS_TPK_PATH = @"D:\self\Programs\AzurPromiliaProject\temp\uncompressed.tpk";
    private const string ASSEMBLY_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\GameAssembly.dll";
    private const string METADATA_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\global-metadata_dec.dat";
    private readonly static UnityVersion DefaultVersion = new("2022.3.62f3");

    public static async Task<int> Main(string[] args)
    {
        var manager = new AssetsManager();
        manager.LoadClassPackage(CLASS_TPK_PATH);
        //var tree = TypeTreeUtils.GetBuiltinTypeTree(
        //    manager,
        //    DefaultVersion,
        //    AssetClassID.MonoBehaviour
        //);
        var tree = TypeTreeUtils.GetCustomMonoBehaviourTypeTree(
            manager, DefaultVersion, METADATA_PATH, ASSEMBLY_PATH,
            "Assembly-CSharp", "Lens.Gameplay.Modules.BigWorld.PlayableAnimator", "AnimationStreamingProfile");
        foreach (var node in tree.Nodes)
        {
            Console.WriteLine($"{node.Level} {node.GetTypeString(tree.StringBufferBytes, tree.CommonStringBufferBytes)} {node.GetNameString(tree.StringBufferBytes, tree.CommonStringBufferBytes)}");
        }
        return 0;
    }

    static async Task<int> Test(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No files provided. Drag-and-drop file(s) onto this executable (or run with file paths).");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return 1;
        }

        foreach (var path in args)
        {
            try
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"Processing file: {path}");
                    var fileName = Path.GetFileName(path);
                    using var sr = new StreamReader(path);
                    string? line;
                    Stack<AssetParser.TypeTreeUtils.TypeTreeNode> stack = new();
                    while ((line = await sr.ReadLineAsync()) is not null)
                    {
                        if (line.Length == 0)
                        {
                            continue;
                        }
                        if (!TypeTreeHelper.TryParseExportNodeLine(line, out var node))
                        {
                            throw new Exception($"Parse node failed: \"{line}\"");
                        }
                        stack.AddNode(node);
                    }
                    while (stack.Count > 1)
                    {
                        stack.Pop();
                    }
                    var rootNode = stack.Pop();
                    Directory.CreateDirectory("./out");
                    using var outputFile = new StreamWriter(Path.Combine("./out", fileName));
                    var generator = new ClassCodeGenerator(rootNode);
                    generator.WriteCode(outputFile);
                }
                else if (Directory.Exists(path))
                {
                    Console.WriteLine($"Dropped item is a directory: {path}");
                }
                else
                {
                    Console.WriteLine($"Path not found: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing '{path}': {ex.Message}");
            }
        }

        Console.WriteLine("Done. Press any key to exit.");
        Console.ReadKey();
        return 0;
    }
}
