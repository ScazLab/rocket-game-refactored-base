# Build-A-Rocket Game

This is the source code for Sarah's Rocket Collaboraiton Project (run during the summer of 2015) that's been refactored for use in the SAR Year 5 Project. 

## Contributors

[Sarah Strohkorb](mailto:sarah.strohkorb@yale.edu), [Alex Ringlein](mailto:alexander.ringlein@yale.edu), and [Shreyas Tirumala](mailto:shreyas.tirumala@yale.edu) at various points helped to develop/refactor this game. Sarah and Alex developed the original game (repo can be found [here](https://github.com/sstrohkorb/rocketgame_v01)). Sarah and Shreyas refactored the code for its use in the SAR year 5 project. 

## Unity

This game was developed in Unity 5.3.5f1. 

## Gameplay

The goal of the build-a-rocket game is to build a rocket that flies as high as possible. Players touch a part of the rocket (body, boosters, fins, or cone) they want to place a piece on, after which the side panels display pieces that can be placed on that part of the rocket. Players drag and drop pieces onto the rocket and dispose of pieces by moving the pieces to the trash cans or the side panels. Players may also drag a piece over the question mark to ask the robot how much that particular piece weighs.

Players have 7 trials to try and make the rocket fly as high as they can. Each trial lasts 2.5 minutes, after which the rocket has a ‘blastoff’ animation and then displays the height the rocket reached. There is a 45-second pause between each trial where the only visual on the screen is a list of the heights the rocket has reached for each completed trial. Once the 45 seconds have elapsed, the next trial automatically begins.

The rocket distance (*D*) is calculated with the following formula: *D = p (α<sub>1</sub>F + α<sub>2</sub>(F ∗ P ) − α<sub>3</sub>W − α<sub>4</sub>R<sub>air</sub> + β)*, where *F* is the rocket fuel, *P* is the rocket power, *R<sub>air</sub>* is the rocket air-resistance, *W* is the rocket weight, *p* is a penalty for not having pieces filled in, and *α* and *β* are constants. This equation is not meant to simulate real-world rocket dynamics, but rather, the intuitive relationship of each of the four factors highlighted in the game (fuel, power, weight, and air resistance). Weight (*W*) and air resistance (*R<sub>air</sub>*) are negatively correlated with rocket distance. Fuel (*F*) and power (*P*) are positively correlated with rocket distance, where power is dependent on fuel and the presence of boosters. Additionally, just as any rocket with pieces missing would not perform as well, we penalize any rocket that does not have all of its pieces filled in with *p*, a proportion of the pieces on the rocket to the total number of possible pieces that the rocket could hold.