using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetClassGenerator.Info;
using AssetClassGenerator.Model;
using AssetClassGenerator.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Testura.Code;
using Testura.Code.Builders;
using Testura.Code.Generators.Class;
using Testura.Code.Models.Properties;
using Testura.Code.Models.Types;

namespace AssetClassGenerator.CodeGen;

public static class ModelCodeGenerator
{
    private const string TypeTreeSourceAttributeName = "global::AssetManager.TypeTree.TypeTreeSource";
    private const string TypeTreeClassAttributeName = "global::AssetManager.TypeTree.TypeTreeClass";
    private const string PropertyShapeAttributeName = "global::PolyType.PropertyShape";

    public static void Generate(
        GeneratorOptions options,
        IReadOnlyList<UnityObjectModel> builtinModels,
        IReadOnlyList<UnityObjectModel> customModels,
        IReadOnlyList<DerivedClassModel> derivedModels)
    {
        string generatedRoot = Path.Combine(Environment.CurrentDirectory, "Generated");
        if (Directory.Exists(generatedRoot))
        {
            Directory.Delete(generatedRoot, true);
        }
        Directory.CreateDirectory(generatedRoot);

        bool sameNamespace = string.Equals(
            options.BuiltinClassNamespace,
            options.CustomClassNamespace,
            StringComparison.Ordinal
        );
        string builtinDirectory = GetNamespaceDirectory(
            generatedRoot,
            options.BuiltinClassNamespace
        );

        foreach (UnityObjectModel model in builtinModels
            .OrderBy(model => model.Name, StringComparer.Ordinal))
        {
            WriteTypeFile(
                Path.Combine(builtinDirectory, model.Name + ".cs"),
                options.BuiltinClassNamespace,
                new[] { CreateUnityObjectClass(model) }
            );
        }

        string customDirectory = null;
        if (!sameNamespace && customModels.Count > 0)
        {
            customDirectory = GetNamespaceDirectory(
                generatedRoot,
                options.CustomClassNamespace
            );
        }

        foreach (UnityObjectModel model in customModels
            .OrderBy(model => model.Name, StringComparer.Ordinal))
        {
            WriteTypeFile(
                Path.Combine(
                    sameNamespace ? builtinDirectory : customDirectory,
                    model.Name + ".cs"
                ),
                model.OutputNamespace,
                new[] { CreateUnityObjectClass(model) }
            );
        }

        List<DerivedClassModel> builtinDerived = derivedModels
            .Where(model => string.Equals(
                model.OutputNamespace,
                options.BuiltinClassNamespace,
                StringComparison.Ordinal))
            .OrderBy(model => model.Name, StringComparer.Ordinal)
            .ToList();
        if (builtinDerived.Count > 0)
        {
            WriteTypeFile(
                Path.Combine(builtinDirectory, "classes.cs"),
                options.BuiltinClassNamespace,
                builtinDerived.Select(CreateDerivedClass)
            );
        }

        if (!sameNamespace && customDirectory != null)
        {
            List<DerivedClassModel> customDerived = derivedModels
                .Where(model => string.Equals(
                    model.OutputNamespace,
                    options.CustomClassNamespace,
                    StringComparison.Ordinal))
                .OrderBy(model => model.Name, StringComparer.Ordinal)
                .ToList();
            if (customDerived.Count > 0)
            {
                WriteTypeFile(
                    Path.Combine(customDirectory, "classes.cs"),
                    options.CustomClassNamespace,
                    customDerived.Select(CreateDerivedClass)
                );
            }
        }
    }

    private static string GetNamespaceDirectory(
        string generatedRoot,
        string outputNamespace)
    {
        string directory = generatedRoot;
        foreach (string segment in outputNamespace.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries))
        {
            directory = Path.Combine(directory, segment);
        }
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static TypeDeclarationSyntax CreateUnityObjectClass(
        UnityObjectModel model)
    {
        ClassBuilder builder = new ClassBuilder(
            model.Name,
            model.OutputNamespace,
            NamespaceType.Classic
        )
            .WithModifiers(Modifiers.Public)
            .WithAttributes(SyntaxFactory.List(new[]
            {
                CreateTypeTreeSourceAttribute(model)
            }))
            .WithProperties(model.Members
                .Select(member => CreateProperty(member, includeOrder: true))
                .ToArray());

        string baseTypeName = model.BaseModel != null
            ? $"global::{model.BaseModel.OutputNamespace}.{model.BaseModel.Name}"
            : $"global::{KnownClassMap.UnityObjectFullyQualifiedName}";
        builder = builder.ThatInheritFrom(CustomType.Create(baseTypeName));

        return EnsureBody(builder.BuildWithoutNamespace());
    }

    private static TypeDeclarationSyntax CreateDerivedClass(DerivedClassModel model)
    {
        RecordBuilder builder = new RecordBuilder(
            model.Name,
            model.OutputNamespace,
            NamespaceType.Classic
        )
            .WithModifiers(Modifiers.Public)
            .WithAttributes(SyntaxFactory.List(new[]
            {
                CreateTypeTreeClassAttribute(model.OriginalName)
            }))
            .WithProperties(model.Members
                .Select(member => CreateProperty(member, includeOrder: false))
                .ToArray());

        TypeDeclarationSyntax type = builder.BuildWithoutNamespace();
        if (model.IsRecordStruct)
        {
            type = type.InsertStructKeyword();
        }

        return EnsureBody(type);
    }

    private static PropertyDeclarationSyntax CreateProperty(
        MemberModel member,
        bool includeOrder)
    {
        PropertyDeclarationSyntax property = PropertyGenerator.Create(
            new AutoProperty(
                member.Name,
                CustomType.Create(member.Type.GetTypeName()),
                PropertyTypes.GetAndSet,
                new[] { Modifiers.Public }
            )
        );

        return property.WithAttributeLists(SyntaxFactory.List(new[]
        {
            CreateTypeTreeClassAttribute(member.Type.OriginalName),
            CreatePropertyShapeAttribute(
                member.OriginalName,
                includeOrder ? member.Order : null
            )
        }));
    }

    private static AttributeListSyntax CreateTypeTreeSourceAttribute(UnityObjectModel model)
    {
        if (model.CustomSource != null)
        {
            return CreateAttribute(
                TypeTreeSourceAttributeName,
                CreateStringArgument(model.CustomSource.AssemblyName),
                CreateStringArgument(model.CustomSource.Namespace),
                CreateStringArgument(model.CustomSource.ClassName)
            );
        }

        return CreateAttribute(
            TypeTreeSourceAttributeName,
            CreateStringArgument("UnityEngine"),
            CreateStringArgument("UnityEngine"),
            CreateStringArgument(model.OriginalName)
        );
    }

    private static AttributeListSyntax CreateTypeTreeClassAttribute(string typeName)
    {
        return CreateAttribute(
            TypeTreeClassAttributeName,
            CreateStringArgument(typeName)
        );
    }

    private static AttributeListSyntax CreatePropertyShapeAttribute(
        string name,
        int? order)
    {
        var arguments = new List<AttributeArgumentSyntax>
        {
            SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("Name"),
                null,
                CreateStringLiteral(name)
            )
        };

        if (order.HasValue)
        {
            arguments.Add(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals("Order"),
                    null,
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(order.Value)
                    )
                )
            );
        }

        arguments.Add(
            SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("IsRequired"),
                null,
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.TrueLiteralExpression
                )
            )
        );

        return CreateAttribute(
            PropertyShapeAttributeName,
            arguments.ToArray()
        );
    }

    private static AttributeListSyntax CreateAttribute(
        string name,
        params AttributeArgumentSyntax[] arguments)
    {
        AttributeSyntax attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName(name)
        );
        if (arguments.Length > 0)
        {
            attribute = attribute.WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(arguments)
                )
            );
        }

        return SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute)
        );
    }

    private static AttributeArgumentSyntax CreateStringArgument(string value)
    {
        return SyntaxFactory.AttributeArgument(CreateStringLiteral(value));
    }

    private static LiteralExpressionSyntax CreateStringLiteral(string value)
    {
        return SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(value)
        );
    }

    private static TypeDeclarationSyntax EnsureBody(TypeDeclarationSyntax type)
    {
        if (!type.SemicolonToken.IsKind(SyntaxKind.None))
        {
            return type
                .WithSemicolonToken(default)
                .WithOpenBraceToken(
                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }

        if (type.OpenBraceToken.IsKind(SyntaxKind.None)
            || type.CloseBraceToken.IsKind(SyntaxKind.None))
        {
            return type
                .WithOpenBraceToken(
                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }

        return type;
    }

    private static void WriteTypeFile(
        string path,
        string outputNamespace,
        IEnumerable<TypeDeclarationSyntax> types)
    {
        NamespaceDeclarationSyntax namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(
                SyntaxFactory.ParseName(outputNamespace)
            )
            .WithMembers(SyntaxFactory.List(
                types.Cast<MemberDeclarationSyntax>()
            ));

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
            .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                namespaceDeclaration))
            .NormalizeWhitespace();

        string text = compilationUnit.ToFullString();
        if (!text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            text += Environment.NewLine;
        }
        File.WriteAllText(path, text);
    }
}
