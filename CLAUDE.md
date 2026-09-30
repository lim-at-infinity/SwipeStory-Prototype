# SwipeStory Prototype

Two-person Unity prototype for USC CSCI526 (repo `lim-at-infinity/SwipeStory-Prototype`, public). A town-management RPG: recruit heroes by swiping cards, build relationships with them, and send parties on auto-battle adventures. There are 3 adventures in unlock order; the run is won by clearing the 3rd (the boss) and lost if the player hero dies.

## Prototype demo focus

The demo is one loop: **recruit heroes > go on an adventure > get loot**, three times. The 3rd adventure is the boss. Anything that doesn't serve this loop waits until after the prototype; content like items is hand-made placeholder assets for now.

## Read first

| Doc | What it is |
| --- | --- |
| `Assets/Docs/DataCheatSheet.md` | Tech contract: every public class, field, method, event and enum. **Source of truth.** |
| `Assets/Docs/DevPractices.md` | How we work: assets, enums, scenes, git, tests |
If code and the contract disagree, ask before "fixing" either one.

## Stack

- Unity 6000.3.22f1, URP 17.3, 2D, uGUI + TextMeshPro, Test Framework 1.6
- C#, **global namespace**, Allman braces, `_camelCase` private fields
- One scene (`Assets/Scenes/Main.unity`); each screen is a prefab under `Canvas/Screens`

## Who owns what

| Area | Owner | Branch |
| --- | --- | --- |
| Data models, enums, BalanceConfig, ScriptableObject definitions, GameManager, RosterManager, Inventory, HeroGenerator, DebugSeed, EditMode tests | Brian Lim | `Core-Data` |
| ScreenId, ScreenManager, DayCycle, ScreenBase, screens, NavButton / DayCycleButton / DayLabel | zyang02 | `screen-manager` |
| Adventure and battle mechanics (how fights play out, rewards flow). The data they use (`AdventureData`, `EncounterData`, `BattleResult`, `AdventureResult`, `AdventureContext`) lives in `Core/Data` as drafts he can reshape | zyang02 | his branches |

Don't edit the other person's files on your branch. Propose the change in chat instead; they make it on theirs.

## Hard rules

- **Contract first.** Adding to the public API: update `DataCheatSheet.md` in the same PR. Renaming or removing: heads-up to the other person first.
- **Enums are saved as ints.** Every enum value is explicit; only append, never reorder or renumber. `ScreenId` lives on `screen-manager`.
- **`Main.unity`: one editor at a time.** Say so in chat before touching it.
- **ScriptableObject assets are read-only at runtime.** Definitions (items, traits, adventures, BalanceConfig) never change during play; runtime state lives in plain classes like `HeroData`.
- **Create assets through the Editor** (Create > SwipeStory > ...), never by hand-writing `.asset` YAML. Commit `.meta` files with their assets.
- **No magic numbers.** Tuning values go in `BalanceConfig`.
- **UI never polls.** Screens update from events.

## For Claude

- Never commit, push or open a PR unless the person you're working with explicitly asks. Show what changed instead.
- Never delete assets or data without asking.
- **No em dashes** in anything you write: docs, code comments, commit messages, chat replies. Use a colon, comma, period or parentheses instead.
- `Assets/Scripts/Core/Data/` is its own assembly (`SwipeStory.Data`) with **no references**: pure data only, no managers, no UI. Everything else compiles into Assembly-CSharp.
- Unity must import new scripts before they exist to the Editor; after creating files, tell the person to switch to Unity and check the Console.

## Folder map

```
Assets/
  Docs/                  contract, dev practices
  Scripts/Core/          managers (GameManager, RosterManager, Inventory, ScreenManager, DayCycle)
  Scripts/Core/Data/     SwipeStory.Data assembly: enums, HeroData, ItemData, TraitData, BalanceConfig, AdventureData
  Scripts/Screens/       one script per screen, plus ScreenBase
  Scripts/Systems/       relationships, battle, rewards
  Scripts/UI/            shared widgets
  Scripts/Debug/         DebugSeed (Editor only)
  Tests/EditMode/        SwipeStory.Tests.EditMode assembly
  Data/Config/           BalanceConfig.asset
  Data/Items/, Data/Traits/, Data/Adventures/, Data/Dialogue/   ScriptableObject assets
  Prefabs/Screens/, Prefabs/UI/
  Scenes/Main.unity, Scenes/Feature Scenes/   (isolated test scenes)
```
