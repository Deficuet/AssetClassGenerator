using System;
using System.Collections.Generic;
using AssetClassGenerator.Info;
using AssetManager.TypeTree;
using AssetsTools.NET.Extra;

namespace AssetClassGenerator.Model;

public sealed class TypeModelBuilder
{
    private const string AssetPPtrTypeName = "global::AssetsTools.NET.AssetPPtr";

    private readonly DerivedClassPool pool;

    public TypeModelBuilder(DerivedClassPool pool)
    {
        this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    public UnityObjectModel BuildUnityObject(
        TypeTree root,
        string name,
        AssetClassID? classId,
        CustomClassSource customSource,
        string outputNamespace,
        ISet<DerivedClassModel> referencedDerived)
    {
        List<MemberModel> members = BuildMembers(root.Children, referencedDerived);
        return new UnityObjectModel(
            name,
            classId,
            customSource,
            outputNamespace,
            members
        );
    }

    private List<MemberModel> BuildMembers(
        IEnumerable<TypeTree> children,
        ISet<DerivedClassModel> referencedDerived)
    {
        var members = new List<MemberModel>();
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (TypeTree child in children)
        {
            TypeReference type = BuildTypeReference(child, referencedDerived);
            string baseName = IdentifierUtils.IsValidIdentifier(child.Name)
                ? child.Name
                : IdentifierUtils.ToCamelCaseIdentifier(child.Name);
            string name = IdentifierUtils.MakeUniqueIdentifier(baseName, usedNames);
            members.Add(new MemberModel(type, child.Name, name));
        }
        return members;
    }

    private TypeReference BuildTypeReference(
        TypeTree node,
        ISet<DerivedClassModel> referencedDerived)
    {
        if (KnownClassMap.TypeInfoMap.TryGetValue(node.Type, out TypeInfo[] typeInfos))
        {
            foreach (TypeInfo typeInfo in typeInfos)
            {
                if (typeInfo.ExpectedType == node.DataType)
                {
                    return Known(
                        node,
                        QualifyKnownType(typeInfo.FullQualifiedName),
                        typeInfo.ExpectedType.IsValueType()
                    );
                }
            }
        }

        if (node.DataType == NodeDataType.PPtr)
        {
            return Known(node, AssetPPtrTypeName, false);
        }

        switch (node.DataType)
        {
            case NodeDataType.Int8:
                return Known(node, "sbyte", true);
            case NodeDataType.Int16:
                return Known(node, "short", true);
            case NodeDataType.Int32:
                return Known(node, "int", true);
            case NodeDataType.Int64:
                return Known(node, "long", true);
            case NodeDataType.UInt8:
                return Known(node, "byte", true);
            case NodeDataType.UInt16:
                return Known(node, "ushort", true);
            case NodeDataType.UInt32:
                return Known(node, "uint", true);
            case NodeDataType.UInt64:
                return Known(node, "ulong", true);
            case NodeDataType.Float:
                return Known(node, "float", true);
            case NodeDataType.Double:
                return Known(node, "double", true);
            case NodeDataType.Char:
            case NodeDataType.WideChar:
                return Known(node, "char", true);
            case NodeDataType.Bool:
                return Known(node, "bool", true);
            case NodeDataType.String:
                return Known(node, "string", false);
            case NodeDataType.ByteArray:
                return BuildByteArrayType(node);
            case NodeDataType.Typeless:
                return BuildTypelessType(node);
            case NodeDataType.Array:
                return new ArrayTypeReference(
                    node.Type,
                    BuildTypeReference(node.Children[0].Children[1], referencedDerived)
                );
            case NodeDataType.Map:
                return BuildMapType(node, NodeDataType.Map, referencedDerived);
            case NodeDataType.Pair:
                return BuildMapType(node, NodeDataType.Pair, referencedDerived);
            case NodeDataType.Class:
            case NodeDataType.Pos2f:
            case NodeDataType.Pos3f:
            case NodeDataType.Pos4f:
            case NodeDataType.ColorRGBA4f:
            case NodeDataType.ColorRGBA32:
            case NodeDataType.Matrix4x4f:
            case NodeDataType.Guid:
            case NodeDataType.Hash128:
                return BuildDerivedType(node, referencedDerived);
            default:
                throw new InvalidOperationException(
                    $"Cannot build a type reference from node '{node.Name}' " +
                    $"with data type {node.DataType}."
                );
        }
    }

    private static ArrayTypeReference BuildByteArrayType(TypeTree node)
    {
        TypeTree elementNode = node.Children[0].Children[1];
        var element = new KnownTypeReference(elementNode.Type, "byte", true);
        return new ArrayTypeReference(node.Type, element);
    }

    private static ArrayTypeReference BuildTypelessType(TypeTree node)
    {
        TypeTree elementNode = node.Children[1];
        var element = new KnownTypeReference(elementNode.Type, "byte", true);
        return new ArrayTypeReference(node.Type, element);
    }

    private MapTypeReference BuildMapType(
        TypeTree node,
        NodeDataType kind,
        ISet<DerivedClassModel> referencedDerived)
    {
        TypeTree pairNode = kind == NodeDataType.Map
            ? node.Children[0].Children[1]
            : node;

        return new MapTypeReference(
            node.Type,
            QualifyKnownType(KnownClassMap.NodeTypeMap[kind]),
            BuildTypeReference(pairNode.Children[0], referencedDerived),
            BuildTypeReference(pairNode.Children[1], referencedDerived)
        );
    }

    private DerivedTypeReference BuildDerivedType(
        TypeTree node,
        ISet<DerivedClassModel> referencedDerived)
    {
        DerivedClassModel model = GetOrAddDerived(node, referencedDerived);
        return new DerivedTypeReference(model);
    }

    private DerivedClassModel GetOrAddDerived(
        TypeTree node,
        ISet<DerivedClassModel> referencedDerived)
    {
        List<MemberModel> members = BuildMembers(node.Children, referencedDerived);
        DerivedClassModel model = pool.GetOrAdd(node.Type, members);
        AddDerivedReference(model, referencedDerived);
        return model;
    }

    private static KnownTypeReference Known(
        TypeTree node,
        string generatedName,
        bool isValueType)
    {
        return new KnownTypeReference(node.Type, generatedName, isValueType);
    }

    private static string QualifyKnownType(string typeName)
    {
        return typeName.StartsWith("global::", StringComparison.Ordinal)
            ? typeName
            : "global::" + typeName;
    }

    private static void AddDerivedReference(
        DerivedClassModel model,
        ISet<DerivedClassModel> referencedDerived)
    {
        if (!referencedDerived.Add(model))
        {
            return;
        }

        foreach (MemberModel member in model.Members)
        {
            AddTypeReference(member.Type, referencedDerived);
        }
    }

    private static void AddTypeReference(
        TypeReference type,
        ISet<DerivedClassModel> referencedDerived)
    {
        switch (type)
        {
            case DerivedTypeReference derived:
                AddDerivedReference(derived.Model, referencedDerived);
                break;
            case ArrayTypeReference array:
                AddTypeReference(array.Element, referencedDerived);
                break;
            case MapTypeReference map:
                AddTypeReference(map.Key, referencedDerived);
                AddTypeReference(map.Value, referencedDerived);
                break;
        }
    }
}
