using AssetParser.Collections;
using AssetParser.TypeTreeUtils;

namespace AssetClassGenerator;

public class ClassCodeGenerator
{
    private readonly ClassReferenceNode rootRefNode;

    public readonly List<ClassReferenceNode> classRefNodeList = [];
    private readonly MultiDictionary<string, List<ClassReferenceNode>> classNameNodeMap = [];

    public ClassCodeGenerator(TypeTreeNode rootNode)
    {
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
}
