using AssetClassGenerator.CodeGen;
using AssetClassGenerator.Info;
using AssetClassGenerator.Model;
using AssetManager.TypeTree;
using AssetsTools.NET;
using AssetsTools.NET.Cpp2IL;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        try
        {
            Generate(options);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }

        return 0;
    }

    private static void Generate(GeneratorOptions options)
    {
        var manager = new AssetsManager();
        manager.LoadClassPackage(options.ClassTpkPath);

        var unityVersion = new UnityVersion(options.UnityVersion);
        ClassDatabaseFile classDb = manager.LoadClassDatabaseFromPackage(unityVersion);

        var pool = new DerivedClassPool();
        var builder = new TypeModelBuilder(pool);
        var builtinModels = new List<UnityObjectModel>();
        var builtinById = new Dictionary<AssetClassID, UnityObjectModel>();
        var builtinDerived = new HashSet<DerivedClassModel>();

        int builtinCount = 0;
        int builtinSkipped = 0;
        foreach (AssetClassID classId in Enum.GetValues<AssetClassID>().Distinct())
        {
            TypeTreeResult result;
            try
            {
                if (!TypeTreeUtils.TryGetBuiltinTypeTree(
                    classDb, classId, out result))
                {
                    Console.WriteLine($"skip builtin {classId}: no type tree");
                    ++builtinSkipped;
                    continue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"skip builtin {classId}: {ex.Message}");
                ++builtinSkipped;
                continue;
            }

            TypeTree tree;
            try
            {
                tree = TypeTree.Build(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"skip builtin {classId}: {ex.Message}");
                ++builtinSkipped;
                continue;
            }

            ClassDatabaseType databaseType =
                classDb.FindAssetClassByID((int)classId);
            string className = databaseType != null
                ? classDb.GetString(databaseType.Name)
                : classId.ToString();
            if (string.IsNullOrEmpty(className))
            {
                className = classId.ToString();
            }
            UnityObjectModel model = builder.BuildUnityObject(
                tree,
                className,
                classId,
                null,
                options.BuiltinClassNamespace,
                builtinDerived
            );
            builtinModels.Add(model);
            builtinById[classId] = model;
            ++builtinCount;
        }
        Console.WriteLine(
            $"builtin classes: {builtinCount} generated, {builtinSkipped} skipped"
        );

        var customModels = new List<UnityObjectModel>();
        var customDerived = new HashSet<DerivedClassModel>();
        if (options.CustomClassSources != null
            && options.CustomClassSources.Count > 0)
        {
            ClassDatabaseType monoBehaviourType =
                classDb.FindAssetClassByID((int)AssetClassID.MonoBehaviour);
            AssetTypeTemplateField monoBehaviourBaseField = null;
            if (monoBehaviourType != null)
            {
                monoBehaviourBaseField = new AssetTypeTemplateField();
                monoBehaviourBaseField.FromClassDatabase(
                    classDb,
                    monoBehaviourType
                );
            }
            else
            {
                Console.Error.WriteLine(
                    "warning: MonoBehaviour was not found in the class database; " +
                    "custom classes will be generated without base fields and " +
                    "will inherit UnityObject."
                );
                monoBehaviourBaseField = new AssetTypeTemplateField
                {
                    Name = "Base",
                    Type = "MonoBehaviour",
                    Children = new List<AssetTypeTemplateField>()
                };
            }

            if (monoBehaviourBaseField != null)
            {
                var generator = new Cpp2IlTempGenerator(
                    options.MetadataPath,
                    options.GameAssemblyPath
                );
                try
                {
                    foreach (CustomClassSource source in options.CustomClassSources)
                    {
                        TypeTreeResult result;
                        try
                        {
                            if (!TypeTreeUtils.TryGetCustomMonoBehaviourTypeTree(
                                monoBehaviourBaseField,
                                unityVersion,
                                generator,
                                source,
                                out result))
                            {
                                Console.WriteLine(
                                    $"skip custom {Describe(source)}: no type tree"
                                );
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"skip custom {Describe(source)}: {ex.Message}"
                            );
                            continue;
                        }

                        TypeTree tree;
                        try
                        {
                            tree = TypeTree.Build(result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"skip custom {Describe(source)}: {ex.Message}"
                            );
                            continue;
                        }

                        UnityObjectModel model = builder.BuildUnityObject(
                            tree,
                            source.ClassName,
                            null,
                            source,
                            options.CustomClassNamespace,
                            customDerived
                        );
                        customModels.Add(model);
                    }
                }
                finally
                {
                    generator.Dispose();
                }
            }
        }

        InheritanceResolver.ResolveBuiltin(
            builtinById,
            UnityObjectInheritanceInfo.BaseTypeMap
        );
        builtinById.TryGetValue(
            AssetClassID.MonoBehaviour,
            out UnityObjectModel monoBehaviourModel
        );
        if (customModels.Count > 0 && monoBehaviourModel == null)
        {
            Console.Error.WriteLine(
                "warning: built-in MonoBehaviour was not generated; " +
                "custom classes will inherit UnityObject instead."
            );
        }
        InheritanceResolver.ResolveCustom(customModels, monoBehaviourModel);

        List<DerivedClassModel> activeDerived = [.. pool.Models
            .Where(model =>
                builtinDerived.Contains(model) || customDerived.Contains(model))];

        ModelNaming.AssignDerivedOutputNamespaces(
            activeDerived,
            builtinDerived,
            customDerived,
            options.BuiltinClassNamespace,
            options.CustomClassNamespace
        );
        ModelNaming.ResolveUnityObjectNames(
            builtinModels,
            customModels,
            options.BuiltinClassNamespace,
            options.CustomClassNamespace
        );
        ModelNaming.RenameDerivedClasses(
            activeDerived,
            builtinModels,
            customModels,
            options.BuiltinClassNamespace,
            options.CustomClassNamespace
        );

        ModelCodeGenerator.Generate(
            options,
            builtinModels,
            customModels,
            activeDerived
        );
    }

    private static string Describe(CustomClassSource source)
    {
        return string.IsNullOrEmpty(source.Namespace)
            ? $"{source.AssemblyName}/{source.ClassName}"
            : $"{source.AssemblyName}/{source.Namespace}.{source.ClassName}";
    }

    //private const string CLASS_TPK_PATH = @"D:\self\Programs\AzurPromiliaProject\temp\uncompressed.tpk";
    //private const string ASSEMBLY_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\GameAssembly.dll";
    //private const string METADATA_PATH = @"D:\self\Programs\AzurPromiliaTest3\reveng\il2cpp\global-metadata_dec.dat";
    //private readonly static UnityVersion DefaultVersion = new("2022.3.62f3");

    //private static int Main2(string[] args)
    //{
    //    var manager = new AssetsManager();
    //    var pkgFile = manager.LoadClassPackage(CLASS_TPK_PATH);
    //    var dataFile = pkgFile.GetClassDatabase(DefaultVersion);

    //    TypeTreeUtils.TryGetBuiltinTypeTree(dataFile, AssetClassID.PlayableDirector, out var tree);

    //    //var existClassIdList = new List<int>();
    //    //foreach (var classId in Enum.GetValues<AssetClassID>())
    //    //{
    //    //    try
    //    //    {
    //    //        var tree = TypeTreeUtils.GetBuiltinTypeTree(
    //    //            manager, DefaultVersion, classId);
    //    //        existClassIdList.Add((int)classId);
    //    //    }
    //    //    catch
    //    //    {

    //    //    }
    //    //}
    //    //Console.WriteLine(string.Join(',', existClassIdList));

    //    //var classIDSet = Enum.GetValues<AssetClassID>().ToHashSet();
    //    //var existSet = UnityObjectInheritanceInfo.BaseTypeMap.Keys.ToHashSet();
    //    //var existCount = 0;
    //    //var nonExistCount = 0;
    //    //foreach (var classID in existSet)
    //    //{
    //    //    try
    //    //    {
    //    //        var tree = TypeTreeUtils.GetBuiltinTypeTree(
    //    //            manager, DefaultVersion, classID);
    //    //        //Console.WriteLine($"{classID} has structure in version {DefaultVersion}");
    //    //        ++existCount;
    //    //    }
    //    //    catch
    //    //    {
    //    //        //Console.WriteLine($"{classID} has no structure in version {DefaultVersion}");
    //    //        ++nonExistCount;
    //    //    }
    //    //}
    //    //Console.WriteLine($"existCount {existCount} nonExistCount {nonExistCount}");

    //    //var tree = TypeTreeUtils.GetCustomMonoBehaviourTypeTree(
    //    //    manager, DefaultVersion, METADATA_PATH, ASSEMBLY_PATH,
    //    //    "Assembly-CSharp", "Lens.Gameplay.Modules.BigWorld.PlayableAnimator", "AnimationStreamingProfile");

    //    foreach (var node in tree.Nodes)
    //    {
    //        Console.WriteLine($"{node.Level} {tree.GetTypeString(node)} {tree.GetNameString(node)}");
    //    }
    //    return 0;
    //}
}
