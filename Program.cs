using AssetParser.TypeTreeUtils;

namespace AssetClassGenerator;

public class MainClass
{
    static async Task<int> Main(string[] args)
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
                    Stack<TypeTreeNode> stack = new();
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
                    //generator.Generate();
                    //generator.WriteGeneratedCode(outputFile);
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
