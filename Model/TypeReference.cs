using System;

namespace AssetClassGenerator.Model;

public abstract class TypeReference
{
    public abstract bool IsValueType { get; }

    public abstract string OriginalName { get; }

    internal abstract string Signature { get; }

    public abstract string GetTypeName();
}

public sealed class KnownTypeReference : TypeReference
{
    private readonly string generatedName;

    public override bool IsValueType { get; }

    public override string OriginalName { get; }

    internal override string Signature => "K:" + generatedName;

    public KnownTypeReference(string originalName, string generatedName, bool isValueType)
    {
        OriginalName = originalName ?? throw new ArgumentNullException(nameof(originalName));
        this.generatedName = generatedName ?? throw new ArgumentNullException(nameof(generatedName));
        IsValueType = isValueType;
    }

    public override string GetTypeName()
    {
        return generatedName;
    }
}

public sealed class DerivedTypeReference : TypeReference
{
    public DerivedClassModel Model { get; }

    public override bool IsValueType => Model.IsRecordStruct;

    public override string OriginalName => Model.OriginalName;

    internal override string Signature => "D:" + Model.StructuralSignature;

    public DerivedTypeReference(DerivedClassModel model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public override string GetTypeName()
    {
        return $"global::{Model.OutputNamespace}.{Model.Name}";
    }
}

public sealed class ArrayTypeReference : TypeReference
{
    private readonly string rootTypeName;

    public TypeReference Element { get; }

    public override bool IsValueType => false;

    public override string OriginalName => $"{rootTypeName}<{Element.OriginalName}>";

    internal override string Signature => "A:" + Element.Signature;

    public ArrayTypeReference(string rootTypeName, TypeReference element)
    {
        this.rootTypeName = rootTypeName ?? throw new ArgumentNullException(nameof(rootTypeName));
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public override string GetTypeName()
    {
        return $"{Element.GetTypeName()}[]";
    }
}

public sealed class MapTypeReference : TypeReference
{
    private readonly string rootTypeName;
    private readonly string definitionName;

    public TypeReference Key { get; }

    public TypeReference Value { get; }

    public override bool IsValueType => false;

    public override string OriginalName => $"{rootTypeName}<{Key.OriginalName}, {Value.OriginalName}>";

    internal override string Signature => $"M:{definitionName}<{Key.Signature},{Value.Signature}>";

    public MapTypeReference(
        string rootTypeName,
        string definitionName,
        TypeReference key,
        TypeReference value)
    {
        this.rootTypeName = rootTypeName ?? throw new ArgumentNullException(nameof(rootTypeName));
        this.definitionName = definitionName ?? throw new ArgumentNullException(nameof(definitionName));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override string GetTypeName()
    {
        return $"{definitionName}<{Key.GetTypeName()}, {Value.GetTypeName()}>";
    }
}
