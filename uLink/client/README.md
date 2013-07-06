PaperCowboys

============


A network multi-player side-scroller for use as a learning tool for developers


This project includes all assets and code for Paper Cowboys except for sound assets and Master Server information.


PRE-REQUISITES
==============
Unity 3.x or better, free or full are both fine.


GETTING SET UP
==============
1. Download a trial license of uLink at http://www.muchdifferent.com/?page=game-unitypark-products-ulink/
2. Install it on every computer you want to use it on.
3. Get the Paper Cowboys uLink project files
4. Unzip the Library zip file. It should create a folder called Library.
5. Open the project from Unity.
6. Go into GameDirector.cs and change the game version to something that makes sense to you (like "0.1 alpha")
7. Go into NetworkDirector.cs and assign a unique value to GameTypeName.
8. Set the build version to Web
9. Build!
10. On the computer that will host the game: Go to your Start Menu, browse to the uLink folder and open the uLink Policy Server.
11. You should be able to play now. If you can't, try starting a policy server on the client computer, too.


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
* The uLink trial eventually expires, so don't download everything and wait to try it out.