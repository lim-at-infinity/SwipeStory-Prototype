# Screens and the Day Cycle

The game is one scene, `Assets/Scenes/Main.unity`. Every screen is a prefab under `Canvas/Screens`, and `ScreenManager` shows one at a time. `DayCycle` tracks the day and whether it is night.

## Screens

| ScreenId | Value | Screen | Reached from |
| --- | --- | --- | --- |
| `Town` | 1 | Town hub; the game starts here | Start, Sleep, Back from the Inn, end of every day |
| `Recruit` | 2 | The Inn (named Recruit in code) | Town |
| `Roster` | 3 | Hero grid and detail panel | Inn |
| `Inspection` | 4 | Unused (inspection is a panel on Roster); kept so later values don't shift | |
| `Dialogue` | 5 | Talk scene | Town's talk buttons |
| `Shop` | 6 | Shop | Town |
| `AdventureSelect` | 7 | Pick an adventure | Town |
| `Battle` | 8 | Auto battle | Adventure Select |
| `Rewards` | 9 | Adventure results | Battle |
| `Summary` | 10 | End of the run | Rewards |
| `CardSwipe` | 11 | Card swipe | Inn's Recruit button |

**Values are fixed** because Unity saves them as numbers in scenes and prefabs. Only add new ones at the end; never reorder or renumber.

## Navigation

- `ScreenManager.Show(id)` hides the current screen and shows `id`. It remembers where you came from, so `Back()` returns there.
- **Showing Town or the Inn clears that history.** They are fresh starting points, so Back never leads out of them; the Inn's Back button goes to Town directly.
- Showing the screen that is already open does nothing (it does not re-run that screen's `OnEnable`).
- Screens hand data to each other through `ScreenManager`: `SelectedHero` (Town to Dialogue) and `Adventure` (Adventure Select to Battle to Rewards).
- For plain navigation, put a `NavButton` on a button instead of writing code.

## Day and night

Each day has a daytime and a night. Doing **one** daytime activity ends the day.

```
Day N:   Town ──▶ Adventure (Select, Battle, Rewards) or Talk (Dialogue) ──EndDay──▶
Night N: Town (only the Inn open) ──▶ Inn ──Sleep──▶
Day N+1: Town ...
```

| Phase | Town shows | Inn shows |
| --- | --- | --- |
| Day | Adventure, Shop, Inn, Talk (when the guild has NPC heroes) | Roster, Back (recruiting is night only) |
| Night | Inn only | Recruit, Roster, Back, Sleep |

A new run (`GameManager.NewGame`) resets the day cycle to day 1, daytime.

## Header bar

`Prefabs/UI/HeaderBar` is the shared top strip: the screen title on the left 55% and a Back button on the right 40%, lined up with Roster's grid and detail panel. Each screen sets its own title text and hides Back where it doesn't make sense (the card swipe screen).

## Script reference

### ScreenManager.cs (manager)

| Member | What it does |
| --- | --- |
| `Current` | The screen showing now (`None` before the first one). |
| `Show(id)` | Shows a screen and records the previous one for Back. Town and the Inn clear the history. |
| `Back()` | Returns to the previous screen, if there is one. |
| `SelectedHero` | The hero passed from Town's talk buttons to Dialogue. |
| `Adventure` | The current adventure (an `AdventureContext`): party, fights and result. Reset on a new run. |
| Inspector `Screen Root`, `Start Screen` | Where the screens are (`Canvas/Screens`) and which one opens first (Town). |
| Events | `OnScreenChanged(from, to)`. |

### DayCycle.cs (manager)

| Member | What it does |
| --- | --- |
| `Day`, `IsNight` | The current day (starts at 1) and phase. |
| `PhaseLabel` | "Day 2 · Night", for display. |
| `EndDay()` | Called after the day's activity (Rewards or Dialogue). Switches to night and shows Town. Ignored at night. |
| `StartNextDay()` | Called by the Inn's Sleep button. Moves to the next day and shows Town. Ignored during the day. |
| Events | `OnPhaseChanged(day, isNight)`. |

### ScreenBase.cs

Base class for every screen. Set its `Id` in the Inspector; `ScreenManager` finds every `ScreenBase` under `Canvas/Screens` when the game starts.

### TownScreen.cs (screen)

| Member | What it does |
| --- | --- |
| Inspector `Talk Buttons` | Up to `TalkCandidatesPerDay` NPC heroes to talk to, picked at random each day. Hidden at night. |
| Inspector `Day Only` | Objects hidden at night (the Adventure and Shop buttons). |

Refreshes from `OnPhaseChanged`, so it updates even when the phase changes while Town is open.

### DialogueScreen.cs (screen)

Placeholder talk scene for the hero in `SelectedHero`: one kind or rude choice raises or lowers affinity by `AffinityPerTalk`, then the day ends.

### Shared buttons and labels

| Script | What it does |
| --- | --- |
| `NavButton` | On a Button: shows a chosen `ScreenId`, or goes Back when `Go Back` is ticked. |
| `DayCycleButton` | On a Button: calls `EndDay` or `StartNextDay`. The Inn's Sleep button uses it. |
| `DayLabel` | On a text: shows `PhaseLabel` and updates on `OnPhaseChanged`. |
