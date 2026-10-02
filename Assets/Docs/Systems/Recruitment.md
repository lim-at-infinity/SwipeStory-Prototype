# Recruitment

Heroes join the guild at the **Inn**. Recruiting draws a hand of hero cards on the **card swipe screen**, and the player picks one. The same swipe screen can run any deck of cards, so it is also ready for item rewards.

## The flow

```
Town ──Inn──▶ Inn ──Recruit (night)──▶ Card swipe ──Continue──▶ Inn
               ├──Roster──▶ Roster ──Back──▶ Inn
               ├──Back──▶ Town
               └──Sleep (night only)──▶ next day, Town
```

| Rule | Detail |
| --- | --- |
| Open hours | The Inn is open day and night (Roster and Back always work); at night it is the only place open in Town. |
| Draws | Recruiting is **night only**: one draw per night of `RecruitCardsPerNight` cards (5) from `BalanceConfig`. Once used, the button is disabled until the next night. |
| Picking | Swiping right on a hero recruits them and ends the draw. Only one hero joins per draw. |
| Passing | Swiping left skips a card. If every card is passed, no one joins. |
| Undo | Spends an undo token to bring back the card that was just passed. Only the most recent pass can come back. |
| Full roster | Recruiting is blocked ("Roster is full"); passing still works. |
| Leaving | The swipe screen has no Back button: the draw ends with a pick, or with Continue once every card is decided. |
| Sleep | Only shown at night. Starts the next day and returns to Town. |

## The card swipe screen

| Gesture | Button | What happens |
| --- | --- | --- |
| Drag right | Accept ("Recruit") | Accepts the card. |
| Drag left | Pass ("Reject") | Skips the card. |
| Drag up | Inspect | Opens the full hero detail panel over the screen. The card stays. |
| | Undo | Brings back the last passed card (costs one undo token). |

While a card is dragged, a hint word fades in on the side it is heading for (Reject, Recruit, Inspect), so the player can see what letting go will do. A drag that falls short of the swipe distance snaps back. After the deck ends, the screen shows the result ("Dain the Warrior joined your guild!") and a Continue button.

## Decks

The swipe screen does not know what its cards are. It is handed a **deck**, and the deck decides what the cards show and what accepting does:

| Deck | Cards | Accept | Pass | Ends when |
| --- | --- | --- | --- | --- |
| `RecruitDeck` | Heroes rolled by `HeroGenerator` | Adds the hero to the roster | Skips | One hero is picked, or every card is passed |
| `ItemRewardDeck` | Item assets | Adds the item to the inventory | Leaves it behind | Every card is decided |

`ItemRewardDeck` is built but not used yet: Rewards currently adds item rewards straight to the inventory. To hand out items by swiping, Rewards would call:

```csharp
CardSwipeScreen.Open(new ItemRewardDeck(items), onFinished);
```

A new kind of deck only needs a new `SwipeDeck` subclass; the screen and card stay the same.

## Prefabs

| Prefab | Use |
| --- | --- |
| `Prefabs/Screens/InnScreen` | The Inn (`ScreenId.Recruit`). |
| `Prefabs/Screens/SwipeScreen` | The card swipe screen (`ScreenId.CardSwipe`). |
| `Prefabs/UI/Swipe/SwipeCard` | The draggable card, with a hero layout, an item layout and three hint labels. |

## Script reference

### SwipeDeck.cs (abstract, plain class)

| Member | What it does |
| --- | --- |
| `Index`, `Count`, `IsFinished` | The current card, how many there are, and whether the deck is done. |
| `Title`, `AcceptLabel`, `PassLabel` | Words shown in the header, on the buttons and as drag hints. |
| `FinishedMessage` | Shown once the deck ends. |
| `FinishOnAccept` | True: accepting one card ends the deck (pick one). False: every card gets its own decision. |
| `CanInspect` | Whether swiping up shows more about a card. |
| `BlockedReason` | Why the current card can't be accepted (e.g. "Roster is full"), or null. |
| `CanAccept`, `CanUndo` | Whether accepting, or undoing the last pass, is possible right now. |
| `TryAccept()` | Accepts the current card and moves on. False if blocked. |
| `Pass()` | Skips the current card. |
| `TryUndo()` | Spends an undo token and brings back the last passed card. False if there is none or no token. |
| `ShowCurrent(view)`, `TryInspect(panel)` | Draw the current card, or show its details. Implemented by each deck. |

### RecruitDeck.cs / ItemRewardDeck.cs

| Member | What it does |
| --- | --- |
| `RecruitDeck(count)` | Rolls `count` heroes up front, so undoing a pass brings back the same hero. Pick one; inspectable. |
| `ItemRewardDeck(items)` | One card per item (nulls skipped). Take or leave each one. |

### CardSwipeScreen.cs (screen)

| Member | What it does |
| --- | --- |
| `Open(deck, onFinished)` (static) | Shows the swipe screen running `deck`. `onFinished` runs when the player presses Continue (the Inn uses it to return to itself). |

Swipes and buttons go through the same path. Updates the undo count from `OnUndoTokensChanged`.

### InnScreen.cs (screen)

| Member | What it does |
| --- | --- |
| Inspector `Undo Tokens Text` | Shows "Undo's Left: N". |
| Inspector `Recruit Button`, `Recruit Label` | Starts the night's draw. Unavailable during the day, and disabled once used. |
| Inspector `Sleep Button` | Shown only at night. |
| Inspector `Back Button` | Returns to Town. |

### SwipeCard.cs (UI component)

| Member | What it does |
| --- | --- |
| `OnSwiped` | Event fired with the direction (Left, Right, Up) when a drag passes the swipe distance. |
| `SetHints(left, right, up)` | The words that fade in while dragging. Null hides one. |
| `SnapBack()`, `FlyOff(direction, onDone)`, `ResetCard()` | Return to the middle, fly off screen, or jump back instantly. |
| Inspector feel values | Swipe distance, max tilt, fly-off distance and animation time. UI feel, so they live here rather than in BalanceConfig. |

### SwipeCardView.cs (UI component)

| Member | What it does |
| --- | --- |
| `ShowHero(hero)` | Shows the hero layout: portrait, name, level and class, stats. |
| `ShowItem(item)` | Shows the item layout: icon, name, type and stars, description. |
