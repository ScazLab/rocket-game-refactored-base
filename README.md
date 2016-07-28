# Build-A-Rocket Game

This is the source code for Sarah's Rocket Collaboraiton Project (run during the summer of 2015) that's been refactored for use in the SAR Year 5 Project. 

## Contributors

[Sarah Strohkorb](mailto:sarah.strohkorb@yale.edu), [Alex Ringlein](mailto:alexander.ringlein@yale.edu), and [Shreyas Tirumala](mailto:shreyas.tirumala@yale.edu) at various points helped to develop/refactor this game. Sarah and Alex developed the original game (repo can be found [here](https://github.com/sstrohkorb/rocketgame_v01)). Sarah and Shreyas refactored the code for its use in the SAR year 5 project. 

## Unity Platform

This game was developed in Unity 5.3.5f1. 

## Gameplay

The goal of the build-a-rocket game is to build a rocket that flies as high as possible. Players touch a part of the rocket (body, boosters, fins, or cone) they want to place a piece on, after which the side panels display pieces that can be placed on that part of the rocket. Players drag and drop pieces onto the rocket and dispose of pieces by moving the pieces to the trash cans or the side panels. Players may also drag a piece over the question mark to ask the robot how much that particular piece weighs.

Players have 7 trials to try and make the rocket fly as high as they can. Each trial lasts 2.5 minutes, after which the rocket has a ‘blastoff’ animation and then displays the height the rocket reached. There is a 45-second pause between each trial where the only visual on the screen is a list of the heights the rocket has reached for each completed trial. Once the 45 seconds have elapsed, the next trial automatically begins.

The rocket distance (*D*) is calculated with the following formula: *D = p (α<sub>1</sub>F + α<sub>2</sub>(F ∗ P ) − α<sub>3</sub>W − α<sub>4</sub>R<sub>air</sub> + β)*, where *F* is the rocket fuel, *P* is the rocket power, *R<sub>air</sub>* is the rocket air-resistance, *W* is the rocket weight, *p* is a penalty for not having pieces filled in, and *α* and *β* are constants. This equation is not meant to simulate real-world rocket dynamics, but rather, the intuitive relationship of each of the four factors highlighted in the game (fuel, power, weight, and air resistance). Weight (*W*) and air resistance (*R<sub>air</sub>*) are negatively correlated with rocket distance. Fuel (*F*) and power (*P*) are positively correlated with rocket distance, where power is dependent on fuel and the presence of boosters. Additionally, just as any rocket with pieces missing would not perform as well, we penalize any rocket that does not have all of its pieces filled in with *p*, a proportion of the pieces on the rocket to the total number of possible pieces that the rocket could hold.

## Current bugs/todos
- The pieces don't always size correctly when put into their slots. 
- The results panel's stats change location each round as each round's stats are added. 
- There are always 4 jet emissions under each booster slot whether or not boosters are present. 
- When we build the game, everything is not placed properly (problems with the dimensions of the screen in build)

## Guide to the code
This section provides a breif overview of the organization of the code. 

### Drag Handler

Each item that can be dragged (i.e. all of the rocket pieces) have this piece of code attached to their game object. The `OnBeginDrag()`, `OnDrag()`, and `OnEndDrag()` imported event functions (from UnityEngine.EventSystems) control the dragging behavior of the pieces. In the `OnEndDrag()` function we either set the piece's location to somewhere on the rocket or we send it back to the panel from which it came. If we're putting the piece on the rocket, we also clone a new piece to replace the one placed on the rocket and put the clone in the position in the panel from which the original piece came. 

### Slots

Each place an item can go (spot on the rocket, trash, or question mark) has a slot script attached to it (`Slot.cs`, `TrashSlot.cs`, and `QuestionMarkSlot.cs` respectively). These slots take action once a piece has been dropped on it within the `OnDrop()` event function (also imported from UnityEngine.EventSystems). When we drop a piece onto a slot, the slot becomes that piece's parent (in the hierarchy). The rocket slos (`Slot.cs`) also trigger the left and right panels to change to that piece type if they're clicked using the built in `OnPointerClick()` function. 

### Events in Drag Handler and Slots
In both `DragHandler.cs` and `Slot.cs` (and its variants), events are declared and called within these functions. For example, there is an event fired when a piece has been added to the rocket. This event is fired from within `Slot.cs`. The event is declared as follows: 
```cs
public delegate void PieceAddedToRocket(GameObject pieceAdded);
public static event PieceAddedToRocket OnPieceAddedToRocket;
```

The event design for this game was based on a [Unity tutorial](https://unity3d.com/learn/tutorials/topics/scripting/events). A delegate is declared and then an event. Within `Slot.cs` we call `OnPieceAddedToRocket(DragHandler.itembeingdragged)`. This publishes this event to all methods subscribing. Within our `GameManager.cs`, we're subscribing to all events triggered in the game and responding to them in the Game Manager.  

### Game Manager
The Game Manager (`GameManager.cs`) does all of the main game control. Anything that is based off of the timing of the game is triggered in the `Update()` function, and everything else is handled through events that are subscribed to by the Game Manager. The Game Manager subscribes to events that are fired in `Slots.cs`, `DragHandler.cs`, and `PanelAnimationEventHandler`. The Game Manager subscribes to these events by defining a function (`PieceAddedToRocket()`) in `GameManager.cs` that will be executed whenever it receives an event: 
```cs
Slot.OnPieceAddedToRocket += PieceAddedToRocket;
```






