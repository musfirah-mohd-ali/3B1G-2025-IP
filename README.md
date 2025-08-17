# 3B1G 2025 IP
## Members:
* Musfirah
* Rasuli
* Alecx
* Rayne

## Introduction
Our game, Kiasu Kouriers is a game that teaches people about road safety and the consequences of not following traffic rules. While learning all that, the player gets to experience the life of a delivery driver and learn how to navigate a tight time crunch and sudden traffic violations.

## Controls of the game
#### Car Spawner (CarSpawner.cs)
The TrafficSpawner handles spawning AI cars at multiple points with random offsets to prevent overlap. It supports continuous spawning, limits per spawn point, and runtime control over car numbers. The system ensures a target number of cars is maintained while tracking total spawns for performance.
#### Car AI (CarAI.cs)
CarAI controls vehicle navigation using Unity’s NavMesh. Cars follow waypoints, avoid obstacles, and respond to traffic lights. Coroutines handle rotation, traffic checks, and waypoint updates for smooth, realistic movement and efficient performance.
#### Player Car Controls (CarBehaviour.cs)
CarBehaviour lets players drive cars with physics-based movement, including acceleration, steering, and braking. Players can exit to a first-person controller and re-enter the car seamlessly, with cameras and audio switching automatically for immersive gameplay.
#### Delivery System (DeliveryManager.cs)
Players pick up packages and are guided by a delivery arrow toward random delivery locations. Entering the delivery zone starts a timer, and staying until it completes successfully delivers the package, increases cash, and updates the delivered package count. The UI displays the current delivery location, distance, and progress.
#### Traffic Violations (TrafficLightPoints.cs & MajorOffenseCounter.cs)
Players must obey traffic lights and avoid collisions. Running a red light or hitting pedestrians and AI cars applies cash penalties or counts as major offenses. Reaching the maximum number of major offenses triggers a bad ending, while careful driving helps achieve the good ending.
#### Level Timer (Timer.cs)
A level timer counts down from a set duration. Players must complete the required number of deliveries before time runs out. When the timer reaches zero, a “Time Up” scene or bad ending is triggered. Players can see the remaining time on the UI in minutes and seconds.

#### Controls
WASD to move, E to exit the car, re-enter the car and to accept packages for delivery and Escape button to open up the menu

## FSM Implementation
### All 4 FSM we implemented
#### CarAI.cs
This script controls AI cars driving along waypoints. The car has states like Moving, Stopped, or Waiting. It reacts to traffic lights, obstacles, or other cars. Coroutines check the car’s path, rotation, and traffic rules. The car’s behavior changes depending on its current state and environment, making it a state machine.
#### ChaserAI.cs
ChaserAI is an enemy that switches between Patrol, Idle, and Chase. It patrols points, idles briefly, and starts chasing the player if they enter its trigger. Leaving the trigger delays the return to patrol. Each state controls what the enemy can do, so the script works like a state machine.
#### PedestrianAI.cs
Pedestrians move along waypoints with states like Walking, Running, or Waiting. They can randomly pick the next waypoint and pause at it if needed. Coroutines handle movement and waiting. The pedestrian’s actions depend on its current state, making it a simple state machine.
#### PoliceChaserAI.cs
PoliceChaserAI chases the player with states Chasing or Stopped. It predicts the player’s movement, checks collisions, and handles package loss. Coroutines manage movement, rotation, and collision detection. The police behavior changes depending on the state, so it works as a state machine.

## Bugs in the game
- Car may stop moving and pile up at one area after the traffic light.
- The SFX may or may not trigger properly.
- The traffic light might not detect the offence.
- The police AI may or may not spawn.

## References and Credits
* Car Movement - https://www.youtube.com/watch?v=TBIYSksI10k&t=442s
* Idea of the for the traffic system - https://www.youtube.com/watch?v=qxMqy4DupRA&t=56s , https://www.youtube.com/watch?v=rOrSukrOak8&t=91s
* Sound Effets - Alec made the music for the gameplay level
* Light Manager - https://youtu.be/m9hj9PdO328?si=Al75eD3PHE6O-U6l
* Rain VFX - https://youtu.be/MBVGUD5nZeA?si=3CK_2b7IUfk9MuJt
* Main Menu Page - https://youtu.be/zc8ac_qUXQY?si=L7clxrWc1NKpxrPv
* Volume UI - https://youtu.be/yWCHaTwVblk?si=Bc7MuUCSMQrLi0db
* Timer UI and Controls - https://youtu.be/POq1i8FyRyQ?si=SysgbaXU23skmamm

