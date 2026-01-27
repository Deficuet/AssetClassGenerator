using AssetParser.Collections;
using AssetParser.TypeTreeUtils;
using System.Text;

namespace AssetClassGenerator;

public class ClassCodeGenerator
{
    private static readonly HashSet<string> s_definedValueTypes =
    [
        "Colorf", "Matrix4x4f", "Quaternionf", "Vector2f", "Vector3f", "Vector4f", "GUID",
    ];

    private readonly TypeTreeNode rootNode;
    private readonly ClassReferenceNode rootRefNode;

    public readonly List<ClassReferenceNode> classRefNodeList = [];
    private readonly MultiDictionary<string, List<ClassReferenceNode>> classNameNodeMap = [];

    public ClassCodeGenerator(TypeTreeNode rootNode)
    {
        this.rootNode = rootNode;
        rootRefNode = new(rootNode, null, this);
        classNameNodeMap.Add(rootRefNode.typeTreeNode.type, [rootRefNode]);
        RenameClasses();
        RestoreUniqueClassName();
    }

    private void RenameClasses()
    {
        while (classRefNodeList.Count > 0)
        {
            var topMostClassGroups = 
                from node in classRefNodeList
                where node.ChildClassCount == 0
                group node by node into g
                select g.ToList();
            foreach (var group in topMostClassGroups)
            {
                var className = group[0].typeTreeNode.type;
                int groupCount;
                if (classNameNodeMap.TryGetValue(className, out var groupList))
                {
                    groupCount = groupList.Count;
                }
                else
                {
                    groupCount = 0;
                }
                foreach (var node in group)
                {
                    node.modifiedClassName = $"{className}_{groupCount + 1}";
                    classRefNodeList.Remove(node);
                    node.classRefParent?.classRefChildren.Remove(node);
                }
                classNameNodeMap.Add(className, group);
            }
        }
    }

    private void RestoreUniqueClassName()
    {
        foreach (var (className, classList) in classNameNodeMap)
        {
            if (classList.Count == 0)
            {
                throw new InvalidOperationException($"Empty class list {className}");
            }
            else if (classList.Count == 1)
            {
                foreach (var node in classList[0])
                {
                    node.modifiedClassName = null;
                }
            }
        }
    }

    public void WriteCode(StreamWriter writer)
    {
        foreach (var classList in classNameNodeMap.Values)
        {
            foreach (var classNodes in classList)
            {
                writer.WriteLine(classNodes[0].GenerateCSharpClass());
            }
        }
    }

    private readonly MultiDictionary<string, string> classCodeMap = [];
    private readonly List<string> classNameList = [];

    public void Generate()
    {
        GenerateClass(rootNode);
    }

    private static bool ShouldGenerate(string className)
    {
        if (s_definedValueTypes.Contains(className))
        {
            return false;
        }
        if (className.StartsWith("PPtr<"))
        {
            return false;
        }
        return true;
    }

    private string GenerateClass(TypeTreeNode classNode)
    {
        if (!ShouldGenerate(classNode.type))
        {
            return classNode.type;
        }
        classNameList.Add(classNode.type);
        string classType;
        if (classNode.children.All((node) => { return node.DataType.IsPrimitiveType() || s_definedValueTypes.Contains(node.type); }))
        {
            classType = "struct";
        }
        else
        {
            classType = "record";
        }
        var sb = new StringBuilder();
        sb.AppendLine("[GenerateSerde]");
        sb.AppendLine($"public partial {classType} {classNode.type}");
        sb.AppendLine("{");
        foreach (var childNode in classNode.children)
        {
            string varName;
            if (IdentifierUtils.IsValidIdentifier(childNode.name))
            {
                varName = childNode.name;
            }
            else
            {
                varName = IdentifierUtils.ToCamelCaseIdentifier(childNode.name);
                sb.AppendLine($"    [SerdeMemberOptions(Rename = \"{childNode.name}\")]");
            }
            var typeName = GetTypeDesc(childNode);
            sb.Append(new string(' ', 4));
            sb.Append("public required ");
            sb.Append($"{typeName} ");
            sb.Append($"{varName};");
            sb.AppendLine();
        }
        sb.AppendLine("}");
        sb.AppendLine();
        classCodeMap.Add(classNode.type, sb.ToString());
        return classNode.type;
    }

    private string GetTypeDesc(TypeTreeNode node)
    {
        return node.DataType switch
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
            
            NodeDataType.Array => $"{GetTypeDescWithGen(node.children[0].children[1])}[]",
            NodeDataType.Map => $"MultiDictionary<{GetTypeDescWithGen(node.children[0].children[1].children[0])}, {GetTypeDescWithGen(node.children[0].children[1].children[1])}>",
            NodeDataType.Pair => $"MapPair<{GetTypeDescWithGen(node.children[0])}, {GetTypeDescWithGen(node.children[1])}>",
            NodeDataType.Class => GenerateClass(node),
            _ => throw new ArgumentException($"Node data type is {node.DataType}")
        };
    }

    private string GetTypeDescWithGen(TypeTreeNode node)
    {
        if (node.DataType == NodeDataType.Class)
        {
            GenerateClass(node);
        }
        return GetTypeDesc(node);
    }

    public void WriteGeneratedCode(StreamWriter writer)
    {
        foreach (var className in classNameList.Distinct())
        {
            foreach (var code in classCodeMap[className].Distinct())
            {
                writer.Write(code);
            }
        }
    }
}
