using System;
using System.Collections.Generic;

namespace AssetClassGenerator.Model;

public sealed class DerivedClassPool
{
    private readonly Dictionary<string, DerivedClassModel> modelsBySignature = new(StringComparer.Ordinal);

    private readonly List<DerivedClassModel> models = [];

    public IReadOnlyList<DerivedClassModel> Models => models;

    public DerivedClassModel GetOrAdd(string originalName, List<MemberModel> members)
    {
        string signature = StructureSignature.Build(originalName, members);
        if (modelsBySignature.TryGetValue(signature, out DerivedClassModel existing))
        {
            return existing;
        }

        var model = new DerivedClassModel(
            originalName,
            members,
            models.Count,
            signature
        );
        models.Add(model);
        modelsBySignature.Add(signature, model);
        return model;
    }
}
