# SwipeStory Dev Practices

Sep 29, 2026 · @Brian Lim

How we work on this project. The tech contract (`DataCheatSheet.md`) says *what* is public; this doc says *how* we build and change things.

## Git and branches

- One branch per task, PR into `main`, the other person reviews before merge.
- Pull `main` before starting work; commit small and often.
- **Commit `.meta` files together with their asset or folder.** A missing `.meta` breaks references for the other person.
- `ProjectSettings/` changes: mention them in the PR description, since they affect both of us.
- Never commit `Library/`, `Temp/`, `Logs/` or `UserSettings/` (already in `.gitignore`).

## Scenes and prefabs

- `Main.unity` is the only game scene. **One person edits it at a time**; say so in chat before opening it for edits, and again when you're done.
- Each screen is a prefab in `Assets/Prefabs/Screens/`. Edit the prefab, not the instance in the scene.
- To test a feature in isolation, make a scene in `Assets/Scenes/Feature Scenes/` instead of changing `Main.unity`.

## Creating assets

- **Always create ScriptableObject assets through the Editor:** right-click in the Project window > Create > SwipeStory > (type). This gives each asset a correct `.meta` file and GUID. Never hand-write `.asset` YAML.
- Every ScriptableObject class gets a `[CreateAssetMenu(menuName = "SwipeStory/<Type>")]` attribute so it shows up in that menu.
- Where assets go:

| Asset | Folder |
| --- | --- |
| BalanceConfig | `Assets/Data/Config/` (only one) |
| Items | `Assets/Data/Items/` |
| Traits | `Assets/Data/Traits/` |
| Adventures | `Assets/Data/Adventures/` |
| Dialogue | `Assets/Data/Dialogue/` |

- Name asset files in PascalCase after the display name (`OldSword.asset`).

## ScriptableObjects vs plain classes

- **ScriptableObject = definition.** Designed in the Editor, shared, never changes at runtime (items, traits, adventures, BalanceConfig). In the Editor, runtime changes to an asset are saved into the file, so writing to one during play corrupts it.
- **Plain C# class = runtime state.** Anything that changes during a run (`HeroData`: level, HP, affinity, equipped items).
- Runtime state can *reference* definitions (`hero.Weapon` points at the `OldSword` asset); it never copies them.

## Enums

- Unity saves enums as ints. **Every value is explicit** (`Warrior = 0`) and new values are **only appended**. Never reorder, renumber or delete a value that has shipped to `main`.
- `ScreenId` belongs to the `screen-manager` side. Ask before appending to it.

## Code

- Style: global namespace, Allman braces, PascalCase for types / methods / properties / events, `_camelCase` for private fields.
- Events start with `On`; methods that can fail start with `Try` and return `bool`.
- **No magic numbers.** Tuning values live in `BalanceConfig`.
- **Randomness:** pass a `System.Random` into anything that rolls, instead of calling `UnityEngine.Random`. Tests can then use a fixed seed.
- **UI never polls.** Subscribe to events in `OnEnable`, unsubscribe in `OnDisable`.

## Managers and startup order

- Managers are singletons on the `Managers` GameObject, reached through `ClassName.Instance`, which is set in `Awake`.
- Execution order: data managers (GameManager, RosterManager, Inventory) run at `[DefaultExecutionOrder(-200)]`, ScreenManager and DayCycle at `-100`, screens at the default `0`.
- Don't read another manager's `Instance` inside your own `Awake`; use `Start` or `OnEnable`.

## Assemblies

- `Assets/Scripts/Core/Data/` compiles into `SwipeStory.Data`, which has **no references**. Only pure data goes there: enums, data classes, ScriptableObject definitions. It cannot see managers, screens or TextMeshPro.
- Everything else stays in Assembly-CSharp, which sees `SwipeStory.Data` automatically.
- Don't add new `.asmdef` files without talking first; they easily create reference cycles.

## Tests

- EditMode tests go in `Assets/Tests/EditMode/` (assembly `SwipeStory.Tests.EditMode`). Run them from Window > General > Test Runner > EditMode.
- Test pure logic: thresholds, lookups, rolls, level-ups. Screens and managers are checked by playing with `DebugSeed`.
- Run the tests before opening a PR that touches `Core/Data`.

## Docs

- Changing the public API: update `DataCheatSheet.md` in the same PR.
- Designing something that won't be built yet: add it to `DeferredSystems.md` with the decisions made so far.
