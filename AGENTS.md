# AssetClassGenerator

## Project

- `net10.0` console generator that reads a TPK class database and optional IL2CPP metadata, then emits C# declarations for Unity object classes and nested runtime types.
- Entrypoint is `Program.Main`. It accepts exactly one argument: a JSON options path. `GeneratorOptions.Load` parses and validates the JSON.
- `README.md` documents the JSON schema, generated attributes, output layout, and the `AnimatorController` example.
- The generator replaces the old AssetStudio type-tree text workflow. It does not use AssetParser or Serde.

## Build / verify

- There is no Linux `dotnet` in this WSL shell. Use the Windows SDK with a Windows project path:
  `"/mnt/c/Program Files/dotnet/dotnet.exe" build 'D:\self\Programs\AzurPromiliaProject\app\AssetClassGenerator\AssetClassGenerator.csproj'`
- There are no tests, CI, lint, formatter, or analyzer config. Verification is a clean build.
- Current baseline is 0 errors and 0 warnings.
- C# sources are UTF-8 BOM + CRLF.
- For generation changes, run against a real TPK/IL2CPP set and syntax-check the emitted C# with Roslyn when practical.

## Pipeline

`GeneratorOptions` -> `AssetsManager`/`ClassDatabaseFile` -> `TypeTreeUtils.TryGetBuiltinTypeTree` / `TryGetCustomMonoBehaviourTypeTree` -> `TypeTreeBranch.Build` -> `TypeModelBuilder` -> `DerivedClassPool` -> `InheritanceResolver` -> `ModelNaming` -> `ModelCodeGenerator`.

- `Model/TypeModelBuilder.cs` builds the model tree. It must consult `Info/KnownClassMap.TypeInfoMap` first; a node is known only when `node.Type` is a key and one listed `TypeInfo.ExpectedType` equals `node.DataType`.
- `PPtr` is special-cased: it maps to `AssetsTools.NET.AssetPPtr` and is not a value type.
- `int2_storage` and `int3_storage` map to `UnityEngine.Vector2Int` and `UnityEngine.Vector3Int` through `Pos2Int` / `Pos3Int`.
- `float3` maps to `UnityEngine.Vector3`; `float4` maps to `UnityEngine.Quaternion`.
- Unmatched structures fall through to ordinary derived-class generation.
- `InheritanceResolver` uses `Info/UnityObjectInheritanceInfo.BaseTypeMap` and prefix member matching; matched parent members are removed from the generated child.
- `KnownClassMap.UnityObjectFullyQualifiedName` is the root base for generated Unity object classes. It defaults to `AssetManager.Classes.IUnityObject`; no `UnityObject` class is generated.
- `DerivedClassPool` deduplicates by `StructureSignature`. `ModelNaming` appends `_<8 hex>` for same-name different structures, known-type-name collisions, Unity object name collisions, and invalid C# identifiers.
- `ModelNaming` takes its known-name set from `KnownClassMap.TypeInfoMap.Keys`.
- `ModelCodeGenerator` uses Testura.Code `BuildWithoutNamespace` plus Roslyn `NamespaceDeclarationSyntax`. `RoslynUtils.InsertStructKeyword` converts records to record structs.
- `ModelCodeGenerator.CreatePropertyShapeAttribute` always emits `Name` and `IsRequired = true`; for Unity object class members it also emits `Order` from `MemberModel.Order`. Derived class members never receive `Order`.
- `MemberModel.Order` is the zero-based index of the member node in the original type-tree `Children` list. Inheritance removal keeps the surviving members' original `Order` values unchanged.
- `MapPair` and `MultiDictionary` map to `AssetManager.Collections.MapPair` and `AssetManager.Collections.MultiDictionary`.

## Output

- `Generated` is recreated at the start of every run in the process current directory.
- Namespaces become nested folders: `Sample.Builtin` -> `Generated/Sample/Builtin/`.
- Each Unity object class is a separate `.cs` file; all derived classes for a namespace go into `classes.cs`.
- No `UnityObject.cs` is generated. Classes without a generated parent implement `KnownClassMap.UnityObjectFullyQualifiedName` directly.
- Generated files contain no `using` directives. Primitives/`string` use keywords; all other types, base classes, and attributes use `global::` fully qualified names.
- Every member is annotated with `[TypeTreeClass(...)]` and `[PropertyShape(Name = "...", IsRequired = true)]`. Unity object class members additionally include `Order`; derived class members do not.

## Gotchas

- Built-in generation iterates every `AssetClassID`. Non-object IDs and IDs without usable type trees are skipped and printed; this is expected.
- Some built-in type trees have a root `Type` equal to a base class (for example `Behaviour` has root type `Component`). Use `ClassDatabaseType.Name` (`classDb.GetString(type.Name)`) as the generated Unity object class name, not `TypeTreeBranch.Type`.
- `ColorRGBA` has two known structures: four float channels maps to `UnityEngine.Color`, packed `rgba` maps to `UnityEngine.Color32`.
- `float4` maps to `UnityEngine.Quaternion`; do not expect a generated `record struct float4`.
- `float3` maps to `UnityEngine.Vector3`.
- `int2_storage` maps to `UnityEngine.Vector2Int`; `int3_storage` maps to `UnityEngine.Vector3Int`.
- `PropertyShape.Order` is only for Unity object class members. It is the original type-tree child index and must not be recomputed when inheritance removes parent members.
- Every generated member is required for PolyType (`IsRequired = true`); do not drop it when changing code generation.
- Cpp2IL-generated custom type trees can report `ByteSize = -1` for class-like nodes. Do not add `ByteSize` requirements to structural `NodeDataTypeHelper` predicates in AssetManager.
- The old `TestMain` block in `Program.cs` is commented out and is not the entrypoint.

## Boundaries

- Do not reintroduce `Deficuet.AssetParser` or Serde.
- Do not edit files under `Generated/`; they are disposable output.
- Do not edit `bin/`, `obj/`, or generated `Generated/` content as source.
- Ask before changing the JSON options schema or public generated-code shape.
- Keep the `Deficuet.AssetManager` package reference in sync with the published package used by the generator.
