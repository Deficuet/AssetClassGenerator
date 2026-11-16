using System;
using System.Collections.Generic;
using System.Linq;
using AssetsTools.NET.Extra;

namespace AssetClassGenerator.Model;

public static class InheritanceResolver
{
    public static void ResolveBuiltin(
        IReadOnlyDictionary<AssetClassID, UnityObjectModel> models,
        IReadOnlyDictionary<AssetClassID, AssetClassID?> baseTypeMap)
    {
        foreach (KeyValuePair<AssetClassID, UnityObjectModel> entry in models)
        {
            if (!baseTypeMap.TryGetValue(entry.Key, out AssetClassID? parentId))
            {
                continue;
            }
            if (parentId is null)
            {
                continue;
            }
            if (!models.TryGetValue(parentId.Value, out UnityObjectModel parent))
            {
                continue;
            }

            ApplyInheritance(entry.Value, parent);
        }
    }

    public static void ResolveCustom(
        IEnumerable<UnityObjectModel> customModels,
        UnityObjectModel monoBehaviourModel)
    {
        if (monoBehaviourModel is null)
        {
            return;
        }

        foreach (UnityObjectModel model in customModels)
        {
            ApplyInheritance(model, monoBehaviourModel);
        }
    }

    private static void ApplyInheritance(UnityObjectModel child, UnityObjectModel parent)
    {
        if (!IsPrefixMatch(child.AllMembers, parent.AllMembers))
        {
            return;
        }

        child.BaseModel = parent;
        child.Members = child.AllMembers.Skip(parent.AllMembers.Count).ToList();
    }

    private static bool IsPrefixMatch(
        List<MemberModel> childMembers,
        List<MemberModel> parentMembers)
    {
        if (parentMembers.Count > childMembers.Count)
        {
            return false;
        }

        for (int i = 0; i < parentMembers.Count; ++i)
        {
            MemberModel child = childMembers[i];
            MemberModel parent = parentMembers[i];
            if (!string.Equals(child.OriginalName, parent.OriginalName, StringComparison.Ordinal))
            {
                return false;
            }
            if (!string.Equals(child.Type.Signature, parent.Type.Signature, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
