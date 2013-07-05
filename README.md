Paper Cowboys is a Unity3D network multi-player side-scroller for use as a learning tool for developers. Each subdirectory of this folder contains the same build, but utilizing a different network engine. 

Each project uses a "semi-authoritative" model: Inputs are processed locally, players relay to the server what their positions and equipment are, and the "master client" relays to all clients the state of non-player entities, the current level, and player drops. 

There are no controls to prevent cheating.

None of these samples include dedicated server sample projects.

The available libraries are:

 * Photon - With the "Photon Cloud" solution, players don't host games. All host management and data exchange is done through 3rd party servers. Players can create games and join them at will. Every game has a "master client" that is responsible for managing the level and non-player entities; usually the master client is the player who joined the game most recently.
 
 * uLink - uLink provides features and increased capabilities over Unity's standard networking library; most of which are not used here. Players host games, and the host is also the "master client" that manages the level and non-player entities.
 
 * Unity Native - This project is 100% native Unity code. Players host games, and the host is also the "master client" that manages the level and non-player entities.



Feel free to try them all and decide which one works best for you.