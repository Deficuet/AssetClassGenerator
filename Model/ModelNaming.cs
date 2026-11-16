using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetClassGenerator.Info;

namespace AssetClassGenerator.Model;

public static class ModelNaming
{
    private static readonly HashSet<string> KnownTypeNames = new(
        KnownClassMap.TypeInfoMap.Keys,
        StringComparer.Ordinal
    );

    public static void AssignDerivedOutputNamespaces(
        IEnumerable<DerivedClassModel> derivedModels,
        ISet<DerivedClassModel> builtinDerived,
        ISet<DerivedClassModel> customDerived,
        string builtinNamespace,
        string customNamespace)
    {
        foreach (DerivedClassModel model in derivedModels)
        {
            if (builtinDerived.Contains(model))
            {
                model.OutputNamespace = builtinNamespace;
            }
            else if (customDerived.Contains(model))
            {
                model.OutputNamespace = customNamespace;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Derived class '{model.OriginalName}' is not referenced by any output namespace."
                );
            }
        }
    }

    public static void ResolveUnityObjectNames(
        IReadOnlyList<UnityObjectModel> builtinModels,
        IReadOnlyList<UnityObjectModel> customModels,
        string builtinNamespace,
        string customNamespace)
    {
        IGrouping<string, UnityObjectModel> duplicateBuiltin =
            builtinModels
                .GroupBy(model => model.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

        UnityObjectModel reservedUnityObject = builtinModels.FirstOrDefault(
            model => string.Equals(model.Name, "UnityObject", StringComparison.Ordinal)
        );
        if (reservedUnityObject != null)
        {
            throw new InvalidDataException(
                "A built-in class is named 'UnityObject', which conflicts with " +
                "the generated UnityObject base class."
            );
        }

        if (duplicateBuiltin != null)
        {
            string classIds = string.Join(
                ", ",
                duplicateBuiltin.Select(model => model.ClassId?.ToString() ?? "<unknown>")
            );
            throw new InvalidDataException(
                $"Built-in class name '{duplicateBuiltin.Key}' is produced by multiple " +
                $"AssetClassIDs: {classIds}."
            );
        }

        bool sameNamespace = string.Equals(
            builtinNamespace,
            customNamespace,
            StringComparison.Ordinal
        );
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        if (sameNamespace)
        {
            usedNames.Add("UnityObject");
            foreach (UnityObjectModel model in builtinModels)
            {
                usedNames.Add(model.Name);
            }
        }

        foreach (IGrouping<string, UnityObjectModel> group in customModels
            .GroupBy(model => model.Name, StringComparer.Ordinal))
        {
            bool conflict = group.Count() > 1 || usedNames.Contains(group.Key);
            foreach (UnityObjectModel model in group)
            {
                if (conflict)
                {
                    string hashedName =
                        model.Name + "_" + StructureHash.Compute(model.StructuralSignature);
                    model.Name = MakeUniqueName(hashedName, usedNames);
                }
                else
                {
                    usedNames.Add(model.Name);
                }
            }
        }
    }

    public static void RenameDerivedClasses(
        IReadOnlyList<DerivedClassModel> derivedModels,
        IReadOnlyList<UnityObjectModel> builtinModels,
        IReadOnlyList<UnityObjectModel> customModels,
        string builtinNamespace,
        string customNamespace)
    {
        var reservedNamesByNamespace = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            [builtinNamespace] = new HashSet<string>(
                builtinModels.Select(model => model.Name),
                StringComparer.Ordinal
            )
        };
        reservedNamesByNamespace[builtinNamespace].Add("UnityObject");
        if (string.Equals(builtinNamespace, customNamespace, StringComparison.Ordinal))
        {
            reservedNamesByNamespace[builtinNamespace].UnionWith(
                customModels.Select(model => model.Name)
            );
        }
        else if (customNamespace != null)
        {
            reservedNamesByNamespace[customNamespace] = new HashSet<string>(
                customModels.Select(model => model.Name),
                StringComparer.Ordinal
            );
        }

        foreach (IGrouping<string, DerivedClassModel> group in derivedModels
            .GroupBy(model => model.OriginalName, StringComparer.Ordinal))
        {
            foreach (DerivedClassModel model in group)
            {
                HashSet<string> reserved = reservedNamesByNamespace[model.OutputNamespace];
                bool conflict = group.Count() > 1
                    || reserved.Contains(model.OriginalName)
                    || !IdentifierUtils.IsValidIdentifier(model.OriginalName)
                    || KnownTypeNames.Contains(model.OriginalName);
                model.Name = conflict
                    ? model.OriginalName + "_" + StructureHash.Compute(model.StructuralSignature)
                    : model.OriginalName;
            }
        }

        var usedNamesByNamespace = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, HashSet<string>> entry in reservedNamesByNamespace)
        {
            usedNamesByNamespace[entry.Key] = new HashSet<string>(
                entry.Value,
                StringComparer.Ordinal
            );
        }

        foreach (DerivedClassModel model in derivedModels)
        {
            HashSet<string> usedNames = usedNamesByNamespace[model.OutputNamespace];
            if (!usedNames.Add(model.Name))
            {
                throw new InvalidDataException(
                    $"Derived class '{model.OriginalName}' was renamed to '{model.Name}', " +
                    $"which collides with another generated type."
                );
            }
        }
    }

    private static string MakeUniqueName(string name, ISet<string> usedNames)
    {
        string candidate = name;
        int suffix = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = name + "_" + suffix;
            ++suffix;
        }
        return candidate;
    }
}
