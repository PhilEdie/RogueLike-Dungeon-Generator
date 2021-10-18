![Examples of dungeons produced by the dungeon generator. ](https://user-images.githubusercontent.com/58746884/105277196-d3a0f680-5c07-11eb-8829-1643c22313bc.gif)

The Rogue-like Dungeon Generator produces a grid-based dungeon consisting of rooms and corridors. 

The generator uses a random walker to generate paths around the map. The walker has a chance to generate a room at its current location. 

Once the walker as reached its maximum step count, the walker will generate an exit square at its current position. 

The player can navigate the map and generate new dungeons by reaching the exit. 

**Controls:**
* W = Up
* S = Down
* A = Left
* D = Right
* ENTER = Generate new dungeon

I developed the project using the Godot engine. 

The tile set was provided by vurmux under the Creative Commons Zero license. 
More information can be found [**here**](https://vurmux.itch.io/urizen-onebit-tilesets). 
