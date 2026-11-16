using System;
using System.Collections.Generic;
using AssetManager.TypeTree;
using AssetsTools.NET.Extra;

namespace AssetClassGenerator.Model;

public sealed class UnityObjectModel
{
    public string Name { get; set; }

    public string OriginalName { get; }

    public AssetClassID? ClassId { get; }

    public CustomClassSource CustomSource { get; }

    public bool IsCustom => CustomSource != null;

    public List<MemberModel> AllMembers { get; }

    public List<MemberModel> Members { get; set; }

    public UnityObjectModel BaseModel { get; set; }

    public string OutputNamespace { get; }

    public string StructuralSignature { get; }

    public UnityObjectModel(
        string name,
        AssetClassID? classId,
        CustomClassSource customSource,
        string outputNamespace,
        List<MemberModel> allMembers)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OriginalName = name;
        ClassId = classId;
        CustomSource = customSource;
        OutputNamespace = outputNamespace ?? throw new ArgumentNullException(nameof(outputNamespace));
        AllMembers = allMembers ?? throw new ArgumentNullException(nameof(allMembers));
        Members = [.. allMembers];
        StructuralSignature = StructureSignature.Build(name, allMembers);
    }
}
