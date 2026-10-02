# Roster

The Roster screen is the Adventurer Guild: a grid of every living hero, with the selected hero's details beside it. Hero inspection is a panel on this screen rather than a screen of its own, so `ScreenId.Inspection` (4) is unused. It stays in the enum so later values don't shift.

## Layout

```
RosterScreen            (RosterScreen script, ScreenId Roster)
├─ HeaderBar            title and Back button
├─ Grid                 scroll view; tiles are spawned into Grid > Viewport > Content
└─ HeroDetailPanel      the selected hero
```

The grid fills the left 55% of the screen and the detail panel the right 40%, both below the header. Positions are anchor percentages, so the layout scales with screen size.

## How it works

| When | What happens |
| --- | --- |
| The screen opens | Subscribes to `RosterManager` events, spawns one `RosterSlot` tile per living hero, and selects the player hero. |
| A tile is clicked | That tile is highlighted and its hero is shown in the detail panel. |
| The roster changes (recruit, death, dismissal) | `OnRosterChanged` rebuilds every tile. The selection is kept if that hero is still in the roster; otherwise it falls back to the player hero. |
| A hero changes (stats, gear, affinity) | `OnHeroUpdated` refreshes that hero's tile. The detail panel listens for itself. |
| The screen closes | Unsubscribes from every event. |

The screen never polls, everything on it updates from events. Affinity changes are covered by `OnHeroUpdated` too, because `RelationshipSystem` notifies both heroes after every change.

## How heroes are drawn

A hero's look combines two things:

| Part | Comes from | Set where |
| --- | --- | --- |
| Shape | The hero's class | `ClassVisuals.asset` (`Assets/Data/Config/`) |
| Color | The hero (random hue, rolled once when the hero is made) | `HeroData.Color`; saturation and brightness in `BalanceConfig` |

| Class | Placeholder shape |
| --- | --- |
| Warrior | Square |
| Mage | Triangle |
| Rogue | Capsule |
| Healer | Circle |

Placeholder sprites are white and live in `Assets/Art/Sprites/Placeholder/`. Real art goes into the `ClassVisuals` asset with no code changes. Each class can also have a larger `Portrait` (recruit cards, dialogue); until one exists, the small `Sprite` is used. The color of heroes gets generated once upon hero generation.

## Reusable pieces

These are built for other screens too. Use them instead of making new versions.

| Prefab (`Assets/Prefabs/UI/`) | Use it for |
| --- | --- |
| `HeroUI/HeroPortrait` | Drawing any hero: cards, battle, dialogue, talk buttons. Call `Show(hero)`. |
| `HeroUI/Panels/HeroDetailPanel` | A full read-only hero view. Recruit will use it for its swipe-up inspect. |
| `HeroUI/RosterSlot` | One roster tile. Spawned by `RosterScreen`; not placed by hand. |
| `HeaderBar` | A screen's title and Back button. The title sits over the grid column and Back over the detail panel column. |

## Testing

`Assets/Scenes/Feature Scenes/HeroRoster.unity` holds the `Managers` prefab and every screen. To open straight onto Roster, set **Start Screen** to Roster on the `Managers` object's Screen Manager; the setting only affects this scene. In the Editor, `DebugSeed` adds four test heroes (one per class, at each relationship tier), so the grid has something to show.

## Not built yet

- Equipping items and dismissing heroes from the detail panel (it is read-only for now).
- Sorting or filtering the grid.
- Showing fallen heroes.

## Script reference

### ClassVisuals.cs (ScriptableObject, `Create > SwipeStory > Class Visuals`)

| Member | What it does |
| --- | --- |
| `Get(heroClass)` | Returns that class's `ClassVisual`. Never throws: a missing class logs a warning and returns an empty visual (drawn as a white box). |

The Inspector warns if a class has no entry or no sprite.

**ClassVisual** (one row of the asset):

| Member | What it does |
| --- | --- |
| `Class` | The class this row is for. |
| `Sprite` | Small token, used for roster tiles and battle. |
| `Portrait` | Large art for cards and dialogue. Optional; empty in the prototype. |
| `PortraitOrSprite` | `Portrait` if set, otherwise `Sprite`. Read this rather than `Portrait`. |

### HeroPortrait.cs (UI component, needs an Image)

| Member | What it does |
| --- | --- |
| `Show(hero)` | Draws the hero's class shape tinted with `hero.Color`, keeping the shape's proportions. Null hides the image. |
| Inspector `Visuals` | The `ClassVisuals` asset. Already set on the prefab. |
| Inspector `Use Portrait` | Draw the large portrait art instead of the small sprite. |

### HeroDetailPanel.cs (UI component)

| Member | What it does |
| --- | --- |
| `Hero` | The hero on display, or null. Read-only; change it with `Show`. |
| `Show(hero)` | Shows the panel and fills it in: portrait, name, level and class, HP, Attack / Defense / Speed, weapon and hat, traits, and affinity with the player (tier and dating status). |
| `Hide()` | Clears the hero and hides the panel. |

- Stats are base values; equipment and relationship bonuses are added in battle, not shown here.
- The affinity line is hidden for the player hero.
- Works for heroes who aren't in the roster yet (a recruit card): their affinity reads 0.
- Refreshes itself on `OnHeroUpdated` while visible.

### RosterSlot.cs (UI component, needs a Button)

| Member | What it does |
| --- | --- |
| `Hero` | The hero this tile shows. |
| `Bind(hero, onClick)` | Sets the hero, fills the tile (portrait, name with "(You)" on the player, level and class), and stores the method to call with this hero when the tile is clicked. |
| `Refresh()` | Redraws the tile from its hero. |
| `SetSelected(selected)` | Shows or hides the selection highlight. |

The tile knows nothing about the screen; it only reports which hero was clicked.

### RosterScreen.cs (screen, extends ScreenBase)

| Member | What it does |
| --- | --- |
| Inspector `Grid` | Where tiles are spawned: `Grid > Viewport > Content`, which arranges them in a grid. |
| Inspector `Slot Prefab` | The `RosterSlot` prefab asset to copy for each hero. |
| Inspector `Detail Panel` | The `HeroDetailPanel` on this screen. |

It has no public methods; it's driven by being shown and by `RosterManager` events (see How it works).
