using AssetParser.TypeTreeUtils;
using System.Text;

namespace AssetClassGenerator;

public class ClassReferenceNode : IEquatable<ClassReferenceNode>
{
    private static readonly HashSet<string> s_wellKnownValueTypes =
    [
        "Colorf", "Matrix4x4f", "Quaternionf", "Vector2f", "Vector3f", "Vector4f", 
        "GUID", "Hash128", "float3",
    ];

    private static readonly Dictionary<string, string> s_valueTypesDelegateMap = new()
    {
        { "Matrix4x4", "Matrix4x4Proxy" },
        { "Quaternion", "QuaternionProxy" },
        { "Vector2", "Vector2Proxy" },
        { "Vector3", "Vector3Proxy" },
        { "Vector4", "Vector4Proxy" },
    };

    public readonly ClassReferenceNode? parent;
    public readonly ClassReferenceNode? classRefParent = null;

    public readonly TypeTreeNode typeTreeNode;

    public readonly List<ClassReferenceNode> children = [];
    public readonly List<ClassReferenceNode> classRefChildren = [];
    public readonly List<ClassReferenceNode> unknownClassRefChildren = [];

    public string? modifiedClassName = null;

    public int UnknownChildClassCount { get { return unknownClassRefChildren.Count; } }

    private bool IsWellKnownClass()
    {
        if (s_wellKnownValueTypes.Contains(typeTreeNode.type))
        {
            return true;
        }
        if (typeTreeNode.type.StartsWith("PPtr<"))
        {
            return true;
        }
        return false;
    }

    public ClassReferenceNode(TypeTreeNode typeTreeNode, ClassReferenceNode? parent, ClassCodeGenerator manager)
    {
        this.typeTreeNode = typeTreeNode;
        this.parent = parent;

        if (typeTreeNode.DataType == NodeDataType.Array)
        {
            children.Add(new(typeTreeNode.children[0].children[1], this, manager));
        }
        else if (typeTreeNode.DataType == NodeDataType.Map)
        {
            foreach (var child in typeTreeNode.children[0].children[1].children)
            {
                children.Add(new(child, this, manager));
            }
        }
        else if (typeTreeNode.DataType == NodeDataType.Pair || typeTreeNode.DataType == NodeDataType.Class)
        {
            foreach (var child in typeTreeNode.children)
            {
                children.Add(new(child, this, manager));
            }
        }

        if (typeTreeNode.DataType == NodeDataType.Class && parent != null)
        {
            var classParent = parent;
            while (classParent is not null && classParent.typeTreeNode.DataType != NodeDataType.Class)
            {
                classParent = classParent.parent;
            }
            classRefParent = classParent;
            classRefParent!.classRefChildren.Add(this);
            if (!IsWellKnownClass())
            {
                manager.unknownClassRefNodeList.Add(this);
                classRefParent!.unknownClassRefChildren.Add(this);
            }
        }
    }

    public string GetTypeName()
    {
        if (typeTreeNode.DataType == NodeDataType.Array)
        {
            return $"{children[0].GetTypeName()}[]";
        }
        else if (typeTreeNode.DataType == NodeDataType.Pair)
        {
            return $"MapPair<{children[0].GetTypeName()}, {children[1].GetTypeName()}>";
        }
        else if (typeTreeNode.DataType == NodeDataType.Map)
        {
            return $"MultiDictionary<{children[0].GetTypeName()}, {children[1].GetTypeName()}>";
        }
        else
        {
            if (modifiedClassName is not null)
            {
                return modifiedClassName;
            }
            return typeTreeNode.DataType switch
            {
                NodeDataType.Int8 => "sbyte",
                NodeDataType.Int16 => "short",
                NodeDataType.Int32 => "int",
                NodeDataType.Int64 => "long",
                NodeDataType.UInt8 => "byte",
                NodeDataType.UInt16 => "ushort",
                NodeDataType.UInt32 => "uint",
                NodeDataType.UInt64 => "ulong",
                NodeDataType.Float => "float",
                NodeDataType.Double => "double",
                NodeDataType.Char or NodeDataType.WideChar => "char",
                NodeDataType.Bool => "bool",
                NodeDataType.Guid => "Guid",
                NodeDataType.Hash128 => "Hash128",
                NodeDataType.ByteArray or NodeDataType.Typeless => "byte[]",
                NodeDataType.String => "string",
                NodeDataType.Class => IdentifierUtils.RemapTypeName(typeTreeNode.type),
                _ => throw new InvalidOperationException($"Should not reach here: {typeTreeNode.DataType}"),
            };
        }
    }

    private string RootClassPreProcess()
    {
        if (typeTreeNode.children[0] is { DataType: NodeDataType.String, name: "m_Name" })
        {
            children.RemoveAt(0);
            return "NamedObject";
        }
        else if (typeTreeNode.children[0] is { DataType: NodeDataType.Class, type: "PPtr<GameObject>", name: "m_GameObject" } )
        {
            children.RemoveAt(0);
            if (typeTreeNode.children[1] is { DataType: NodeDataType.UInt8, name: "m_Enabled" })
            {
                children.RemoveAt(0);
                if (typeTreeNode.children[2] is { DataType: NodeDataType.Class, type: "PPtr<MonoScript>", name: "m_Script" } &&
                    typeTreeNode.children[3] is { DataType: NodeDataType.String, name: "m_Name" })
                {
                    children.RemoveRange(0, 2);
                    return "MonoBehaviour";
                }
                return "Behaviour";
            }
            return "UnityComponent";
        }
        return "UnityObject";
    }

    public string GenerateCSharpClass()
    {
        if (typeTreeNode.DataType != NodeDataType.Class) 
        {
            throw new InvalidOperationException($"Node {typeTreeNode.name}({typeTreeNode.type}, {typeTreeNode.DataType}) is not a class.");
        }
        var externalProxyNames = new HashSet<string>();

        var sb = new StringBuilder();
        sb.AppendLine("[GenerateSerde]");
        foreach (var classRef in classRefChildren)
        {
            var classRefName = classRef.GetTypeName();
            if (s_valueTypesDelegateMap.ContainsKey(classRefName))
            {
                externalProxyNames.Add(classRefName);
            }
        }
        foreach (var typeName in externalProxyNames)
        {
            sb.AppendLine($"[UseProxy(ForType = typeof({typeName}), Proxy = typeof({s_valueTypesDelegateMap[typeName]}))]");
        }
        sb.Append($"public partial record {GetTypeName()}");
        if (parent is null)
        {
            sb.Append($" : {RootClassPreProcess()}");
        }
        sb.AppendLine();
        sb.AppendLine("{");
        foreach (var childNode in children)
        {
            var typeName = childNode.GetTypeName();
            var memberOptionsMap = new Dictionary<string, string>();
            string varName;
            if (IdentifierUtils.IsValidIdentifier(childNode.typeTreeNode.name))
            {
                varName = childNode.typeTreeNode.name;
            }
            else
            {
                varName = IdentifierUtils.ToCamelCaseIdentifier(childNode.typeTreeNode.name);
                memberOptionsMap.Add("Rename", $"\"{childNode.typeTreeNode.name}\"");
            }
            if (s_valueTypesDelegateMap.ContainsKey(typeName))
            {
                externalProxyNames.Add(typeName);
            }
            if (memberOptionsMap.Count > 0)
            {
                sb.Append("    [SerdeMemberOptions(");
                var memberOptionsStr =
                    from item in memberOptionsMap
                    select $"{item.Key} = {item.Value}";
                sb.Append(string.Join(", ", memberOptionsStr));
                sb.AppendLine(")]");
            }
            sb.Append(new string(' ', 4));
            sb.Append("public required ");
            sb.Append($"{typeName} ");
            sb.Append($"{varName};");
            sb.AppendLine();
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not ClassReferenceNode) return false;
        return Equals((ClassReferenceNode)obj);
    }

    public bool Equals(ClassReferenceNode? other)
    {
        if (other is null) return false;
        if (typeTreeNode.type != other.typeTreeNode.type) return false;
        if (children.Count != other.children.Count) return false;
        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            var otherChild = other.children[i];
            if (child.GetTypeName() != otherChild.GetTypeName()) return false;
            if (child.typeTreeNode.name != otherChild.typeTreeNode.name) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(typeTreeNode.type);
        hc.Add(children.Count);
        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            hc.Add(child.GetTypeName());
            hc.Add(child.typeTreeNode.name);
        }
        return hc.ToHashCode();
    }

    public bool IsOffsetPtr(out ClassReferenceNode pointedNode)
    {
        if (this is { typeTreeNode.type: "OffsetPtr", children: [{ } underlyingNode] })
        {
            pointedNode = underlyingNode;
            return true;
        }
        pointedNode = default!;
        return false;
    }
}
