using System;

namespace AssetClassGenerator.Model;

public sealed class MemberModel
{
    public TypeReference Type { get; }

    public string OriginalName { get; }

    public string Name { get; }

    public int? Order { get; }

    public MemberModel(
        TypeReference type,
        string originalName,
        string name,
        int? order = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        OriginalName = originalName ?? throw new ArgumentNullException(nameof(originalName));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Order = order;
    }
}
