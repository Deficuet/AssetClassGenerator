using AssetParser.Collections;
using AssetParser.TypeTreeUtils;

namespace AssetClassGenerator;

public class ClassCodeGenerator
{
    private readonly ClassReferenceNode rootRefNode;

    public readonly List<ClassReferenceNode> unknownClassRefNodeList = [];
    private readonly MultiDictionary<string, ClassReferenceNode[]> classNameNodesMap = [];

    public ClassCodeGenerator(TypeTreeNode rootNode)
    {
        rootRefNode = new(rootNode, null, this);
        classNameNodesMap.Add(rootRefNode.typeTreeNode.type, [rootRefNode]);
        RenameClasses();
        RestoreUniqueClassName();
    }

    private static void RenameNode(ClassReferenceNode node, string rawClassName, int renameIndex)
    {
        if (node.IsOffsetPtr(out var pointedNode))
        {
            node.modifiedClassName = $"OffsetPtr_{pointedNode.typeTreeNode.type}";
            return;
        }
        node.modifiedClassName = $"{rawClassName}_{renameIndex}";
    }

    private void RenameClasses()
    {
        while (unknownClassRefNodeList.Count > 0)
        {
            var topMostClassGroups = 
                from node in unknownClassRefNodeList
                where node.UnknownChildClassCount == 0
                group node by node into g
                select g.ToArray();
            foreach (var group in topMostClassGroups)
            {
                var className = group[0].typeTreeNode.type;
                int groupCount;
                if (classNameNodesMap.TryGetValue(className, out var groupArr))
                {
                    groupCount = groupArr.Count;
                }
                else
                {
                    groupCount = 0;
                }
                foreach (var node in group)
                {
                    RenameNode(node, className, groupCount + 1);
                    unknownClassRefNodeList.Remove(node);
                    node.classRefParent?.unknownClassRefChildren.Remove(node);
                }
                classNameNodesMap.Add(className, group);
            }
        }
    }

    private void RestoreUniqueClassName()
    {
        foreach (var (className, classArr) in classNameNodesMap)
        {
            if (classArr.Count == 0)
            {
                throw new InvalidOperationException($"Empty class list {className}");
            }
            else if (classArr.Count == 1)
            {
                foreach (var node in classArr[0])
                {
                    if (!node.IsOffsetPtr(out _))
                    {
                        node.modifiedClassName = null;
                    }
                }
            }
        }
    }

    public void WriteCode(StreamWriter writer)
    {
        foreach (var classList in classNameNodesMap.Values)
        {
            foreach (var classNodes in classList)
            {
                writer.WriteLine(classNodes[0].GenerateCSharpClass());
            }
        }
    }
}
