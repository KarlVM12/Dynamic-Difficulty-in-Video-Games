# Dynamic Difficulty in Video Games
### Integrating Deep Neural Networks into Games to Promote Dynamic Difficulty & Increase Player Engagement
One of the struggles of modern game development lies in finding different ways to keep their players engaged. Instead of designing new game mechanics that could fail or not resonate with the players, a different approach can focus on designing the game to learn the player. 
This project proposed increased integration of true enemy AI through Deep Neural Networks (DNNs) and Reinforcement Learning (RL). 
To demonstrate this concept, a classic action mech runner game was designed with a tutorial, first level, boss sequence, each containing various enemies and environmental hazards. 
Each different enemy type received their own fully trained AI model implemented from Unity’s ML Agents package and a local Pytorch training environment. 
It was concluded that the inclusion of these models did enhance player engagement as each enemy would dynamically adjust their behavior to the skill level which a player is exhibiting, optimizing the gameplay experience without the need for a hardcoded dynamic difficulty adjustment system.

## Details
The purpose of this project was to see how one could apply **_Dynamic Difficulty_**, the ability to adjust the skill level of a game on the fly according to player interaction, to video game mechanics to increase player engagement through Deep Neural Networks.
In order to achieve this goal, a fully fleshed out first level of a run & gun shooter game was created, **_K7 Mech Runner_**, including a demo, multiple enemy types, and a final boss. The game was created in Unity and each enemy was modeled with unique abilities.
Each enemy type was trained with its own Machine Learned model, utilizing Reinforcement Learning & Deep Neural Networks to train each enemy leveraging Pytorch and Unity's ML Agent. This involved the entire pipeline of the Machine Learning and Game Development pipeline.

In order to test dynamic difficulty, player's had to actually be able to have a hard time first playing and be able to get gradually better. The game and enemies had to be fun in order to properly test player engagement. 
This is why we went with the Run & Gun Mech Runner genre. These types of games are fast paced, but are open to any type of player and skill level. This fast paced nature also more easily lends itself to adjust level difficulty and examining player engagement. To this point, three main enemies existed throughout the level:

