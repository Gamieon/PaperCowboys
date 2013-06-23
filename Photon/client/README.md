PaperCowboys

============


A network multi-player side-scroller for use as a learning tool for developers


This project includes all assets and code for Paper Cowboys except for sound assets and Photon App information.


PRE-REQUISITES
==============
Unity 3.x or better, free or full are both fine.


GETTING SET UP
==============
1. Go to the Photon Cloud website at http://cloud.exitgames.com/ and sign up for a free account.
2. Create a new app for your photon cloud account. Come up with a name other than Paper Cowboys.
3. Get the app ID of your new app.
4. Get the project files
5. Unzip the Library zip file. It should create a folder called Library.
6. Open the project from Unity.
7. Go to Window => Photon Unity Networking
8. Click on Setup
9. Type in your AppID, and set your Cloud Region to your closest locale.
10. Go into GameDirector.cs and change the game version to something that makes sense to you (like "0.1 alpha")
11. Set the build version to Web
12. Build and test!


POINTS OF INTEREST
==================
* The actual hosting server is in the Photon Cloud. In every game there is a "master client" that is in charge of enemies and environmental scene objects. The master client seems to always be the newest player that is still in the game.

* There are three communication groups:
0 - Scene objects and state synchronization
1 - Level loading
