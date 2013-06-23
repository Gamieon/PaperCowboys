PaperCowboys

============


A network multi-player side-scroller for use as a learning tool for developers


This project includes all assets and code for Paper Cowboys except for sound assets and Master Server information.


PRE-REQUISITES
==============
Unity 3.x or better, free or full are both fine.


GETTING SET UP
==============
1. Get the project files
2. Unzip the Library zip file. It should create a folder called Library.
3. Open the project from Unity.
4. Open NetworkDirector.cs
5. Come up with a unique name, and put it in GameTypeName. It doesn't matter what it is; it will be sent to the Unity master server so players can find your game.
6. Build and test!


POINTS OF INTEREST
==================
* This is a 100% client-server model. When the hosting player goes, the game is over.

* There are three communication groups:
0 - Scene objects and state synchronization
1 - Level loading
2 - Player property syncing (names, colors)

* NetworkDirector and NetworkLevelLoader are the primary network management classes

* When objects with networkViews in the scene need to be destroyed, we send a buffered RPC call to do it. If we don't, then new players will see the objects when they join; and those objects won't disappear.


CURRENTLY KNOWN ISSUES
========================
* This project has not been vigorously tested, nor has it been reviewed by any professional Unity developers with lots of networking experience
* When joining a game, any players with existing accessories (hats, bandanas) don't have their colors synced.
* When changing levels, some "lingering RPCs" get carried to the next level. This doesn't seem to inherently harm anything, but it's a clear indictation that something isn't happening correctly.
* Avoid referencing player properties in channel 0 as much as you can. I have not found a way to guarantee that when a client joins, they get everyone's player properties before the level gets loaded and synchronized.
