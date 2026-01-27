using AssetParser.TypeTreeUtils;
using System.Text;

namespace AssetClassGenerator;

public class ClassReferenceNode : IEquatable<ClassReferenceNode>
{
    private static readonly HashSet<string> s_definedValueTypes =
    [
        "Colorf", "Matrix4x4f", "Quaternionf", "Vector2f", "Vector3f", "Vector4f", "GUID", "Hash128"
    ];

    public readonly ClassReferenceNode? parent;
    public readonly ClassReferenceNode? classRefParent = null;

    public readonly TypeTreeNode typeTreeNode;

    public readonly List<ClassReferenceNode> children = [];
    public readonly List<ClassReferenceNode> classRefChildren = [];

    public string? modifiedClassName = null;

    public int ChildClassCount { get { return classRefChildren.Count; } }

    private bool IsWellKnownClass()
    {
        if (s_definedValueTypes.Contains(typeTreeNode.type))
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

        if (typeTreeNode.DataType == NodeDataType.Class && !IsWellKnownClass() && parent != null)
        {
            manager.classRefNodeList.Add(this);
            var ancestor = parent;
            while (ancestor is not null && ancestor.typeTreeNode.DataType != NodeDataType.Class)
            {
                ancestor = ancestor.parent;
            }
            classRefParent = ancestor;
            classRefParent!.classRefChildren.Add(this);
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
                NodeDataType.ByteArray or NodeDataType.Typeless => "byte[]",
                NodeDataType.String => "string",
                NodeDataType.Class => IdentifierUtils.RemapTypeName(typeTreeNode.type),
                _ => throw new InvalidOperationException($"Should not reach here: {typeTreeNode.DataType}"),
            };
        }
    }

    private string RootClassPreProcess()
    {
        var firstField = typeTreeNode.children[0];
        if (firstField.DataType == NodeDataType.String && firstField.name == "m_Name")
        {
            children.RemoveAt(0);
            return "NamedObject";
        }
        else
        {
            return "UnityObject";
        }
    }

    public string GenerateCSharpClass()
    {
        if (typeTreeNode.DataType != NodeDataType.Class) 
        {
            throw new InvalidOperationException($"Node {typeTreeNode.name}({typeTreeNode.type}, {typeTreeNode.DataType}) is not a class.");
        }
        var sb = new StringBuilder();
        sb.AppendLine("[GenerateSerde]");
        sb.Append($"public partial record {GetTypeName()}");
        if (parent is null)
        {
            sb.Append($" : {RootClassPreProcess()}");
        }
        sb.AppendLine();
        sb.AppendLine("{");
        foreach (var childNode in children)
        {
            string varName;
            if (IdentifierUtils.IsValidIdentifier(childNode.typeTreeNode.name))
            {
                varName = childNode.typeTreeNode.name;
            }
            else
            {
                varName = IdentifierUtils.ToCamelCaseIdentifier(childNode.typeTreeNode.name);
                sb.AppendLine($"    [SerdeMemberOptions(Rename = \"{childNode.typeTreeNode.name}\")]");
            }
            var typeName = childNode.GetTypeName();
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
}
