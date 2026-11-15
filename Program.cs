using AssetClassGenerator.Info;
using AssetManager.TypeTree;
using AssetParser.TypeTreeUtils;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Testura.Code.Builders;

namespace AssetClassGenerator;

public class MainClass
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine(
                "Usage: AssetClassGenerator <path/to/options.json>"
            );
            return 1;
        }

        GeneratorOptions options;
        try
        {
            options = GeneratorOptions.Load(args[0]);
        }
        catch (Exception ex) when (
            ex is InvalidDataException
            || ex is IOException
            || ex is UnauthorizedAccessException
            || ex is ArgumentException
            || ex is NotSupportedException
        )
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }

        Generate(options);
        return 0;
    }

    private static void Generate(GeneratorOptions options)
    {
        // TODO: generate class declarations from the configured type trees.
    }

    /*
    private const string CLASS_TPK_PATH = @"D:\self\Programs\AzurPromiliaProject\temp\uncompressed.tpk";
    private const string ASSEMBLY_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\GameAssembly.dll";
    private const string METADATA_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\global-metadata_dec.dat";
    private readonly static UnityVersion DefaultVersion = new("2022.3.62f3");

    private static int TestMain(string[] args)
    {
        var manager = new AssetsManager();
        manager.LoadClassPackage(CLASS_TPK_PATH);

        //var tree = TypeTreeUtils.GetBuiltinTypeTree(
        //    manager, DefaultVersion, AssetClassID.IConstraint);

        //var existClassIdList = new List<int>();
        //foreach (var classId in Enum.GetValues<AssetClassID>())
        //{
        //    try
        //    {
        //        var tree = TypeTreeUtils.GetBuiltinTypeTree(
        //            manager, DefaultVersion, classId);
        //        existClassIdList.Add((int)classId);
        //    }
        //    catch
        //    {

        //    }
        //}
        //Console.WriteLine(string.Join(',', existClassIdList));

        var classIDSet = Enum.GetValues<AssetClassID>().ToHashSet();
        var existSet = UnityObjectInheritanceInfo.BaseTypeMap.Keys.ToHashSet();
        var existCount = 0;
        var nonExistCount = 0;
        foreach (var classID in existSet)
        {
            try
            {
                var tree = TypeTreeUtils.GetBuiltinTypeTree(
                    manager, DefaultVersion, classID);
                //Console.WriteLine($"{classID} has structure in version {DefaultVersion}");
                ++existCount;
            }
            catch
            {
                //Console.WriteLine($"{classID} has no structure in version {DefaultVersion}");
                ++nonExistCount;
            }
        }
        Console.WriteLine($"existCount {existCount} nonExistCount {nonExistCount}");

        //var tree = TypeTreeUtils.GetCustomMonoBehaviourTypeTree(
        //    manager, DefaultVersion, METADATA_PATH, ASSEMBLY_PATH,
        //    "Assembly-CSharp", "Lens.Gameplay.Modules.BigWorld.PlayableAnimator", "AnimationStreamingProfile");

        //foreach (var node in tree.Nodes)
        //{
        //    Console.WriteLine($"{node.Level} {tree.GetTypeString(node)} {tree.GetNameString(node)}");
        //}
        return 0;
    }
    */
}
