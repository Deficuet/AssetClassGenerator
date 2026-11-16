using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetClassGenerator.Model;

public sealed class DerivedClassModel
{
    public string OriginalName { get; }

    public string Name { get; set; }

    public List<MemberModel> Members { get; }

    public bool IsRecordStruct { get; }

    public int PoolIndex { get; }

    public string StructuralSignature { get; }

    public string OutputNamespace { get; set; }

    public DerivedClassModel(
        string originalName,
        List<MemberModel> members,
        int poolIndex,
        string structuralSignature)
    {
        OriginalName = originalName ?? throw new ArgumentNullException(nameof(originalName));
        Name = originalName;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        IsRecordStruct = members.All(member => member.Type.IsValueType);
        PoolIndex = poolIndex;
        StructuralSignature = structuralSignature ?? throw new ArgumentNullException(nameof(structuralSignature));
    }
}
