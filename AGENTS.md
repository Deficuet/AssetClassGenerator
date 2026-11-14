# AGENTS.md

## What this is

Single-project (`AssetClassGenerator.csproj`, `net10.0`, console) code generator: parses
AssetStudio-exported type-tree `.txt` files into C# records decorated with `[GenerateSerde]` for
Serde.NET. `README.md` documents the intended input/output format and naming rules; this file covers
what is easy to get wrong.

## Build / verify

- No Linux `dotnet` in this WSL shell. Use the Windows SDK with a **Windows** project path:
  `"/mnt/c/Program Files/dotnet/dotnet.exe" build 'D:\self\Programs\AzurPromiliaProject\app\AssetClassGenerator\AssetClassGenerator.csproj'`
- No tests, CI, lint, formatter, or analyzer config. Verification is a clean build only.
- Baseline is **0 errors, 7 CS8632 warnings**: `Nullable` is not enabled, but `?` reference-type
  annotations are used. Do not enable `Nullable` just to silence these unless the change is intended.
- Build artifacts (`bin/`, `obj/`) and generated `out/` are gitignored. Sources are UTF-8 BOM + CRLF.

## `Program.Main` is a scratch harness, not the documented CLI

`Main` currently ignores `args`, loads hardcoded machine-specific paths (`CLASS_TPK_PATH` `.tpk`,
`ASSEMBLY_PATH` `GameAssembly.dll`, `METADATA_PATH` `global-metadata_dec.dat`), extracts one
`GetCustomMonoBehaviourTypeTree(...)` for `Lens.Gameplay.Modules.BigWorld.PlayableAnimator` /
`AnimationStreamingProfile`, and prints nodes. The README's file-processing implementation is the
unused `MainClass.Test(string[])`, which reads type-tree text files, writes generated classes to
`./out/<original file name>`, and blocks on `Console.ReadKey()` when done.

- If asked to process type-tree files, rewire `Main` to `Test` (or remove the hardcoded block);
  do not assume the README behavior is what runs today.
- `Test` always blocks on `Console.ReadKey()` before returning (and exits `1` with no args), so it
  cannot run unattended as-is.

## Generation pipeline

`Program` -> `ClassCodeGenerator(TypeTreeNode root)` -> `ClassReferenceNode` tree -> `WriteCode`.

- Input line parsing uses `TypeTreeHelper.TryParseExportNodeLine` and `Stack.AddNode` from the
  `Deficuet.AssetParser` NuGet package; tree shape comes from indentation. The last node left on the
  stack is the root. Array/map/pair nodes are normalized into element/key/value children by their
  known child layout in `ClassReferenceNode`'s constructor.
- `ClassReferenceNode` equality is structural (type + direct child type/name). Same-shaped classes
  resolved in the same pass are grouped and emitted once; same-name classes with different shapes
  become `Name_1`, `Name_2`, ...; names are restored (nullable `modifiedClassName`) when unique.
  `OffsetPtr` nodes are always emitted as `OffsetPtr_<target>`.
- `GenerateCSharpClass` **mutates** the root node's `children` while dropping inherited fields, so
  generate a root only once (as `WriteCode` does).
- Well-known value types are not generated: `ColorRGBA`, `Matrix4x4f`, `Quaternionf`, `Vector2f`,
  `Vector3f`, `Vector4f`, `GUID`, `Hash128`, `float3`; `PPtr<...>` is also treated as known.
  `IdentifierUtils` remaps them (`float3` -> `Vector3`, `GUID` -> `Guid`, etc.) and
  `Object`/`Component` -> `UnityObject`/`UnityComponent` to avoid name clashes.
- Root-only: `RootClassPreProcess` infers a base class from the exact leading field sequence
  (`m_Name` -> `NamedObject`; `m_GameObject`/`m_Enabled`/`m_Script`/`m_Name` -> `MonoBehaviour` /
  `Behaviour` / `UnityComponent`; otherwise `UnityObject`) and drops those inherited fields. The
  order checks are fragile—change them only with real exported trees to verify.
- Invalid field names are camel-cased and preserved via `[SerdeMemberOptions(Rename = "...")]`;
  remember to update `IdentifierUtils` keyword/identifier rules if you touch this path.
- Emitted code is **not** standalone-compilable here: it requires Serde.NET, `GenerateSerde`/
  `UseProxy`/`SerdeMemberOptions`, and external proxies such as `Vector2Proxy`/`GuidProxy` supplied by
  the consuming project. Never try to compile `out/` in this repo.

## Dependencies

NuGet-only, no project references: `Deficuet.AssetParser` (type tree/parse utilities),
`Deficuet.AssetsTools.NET` (TPK/class database), `Deficuet.AssetsTools.NET.Cpp2IL` (IL2CPP template
generation). The `TypeTree/` helpers need an actual Unity TPK and game IL2CPP files; they cannot be
exercised with this repo alone.
