using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public class World : Godot.Node2D
{
    // Constants    -------------------------------------


    Random Rand = new Random();
    public const int TILE_SIZE = 36;

    public const int BorderX = 100;     //Maximum width of the map.
    public const int BorderY = 100;     //Maximum height of the map. 
    public const int LineOfSight = 5;   //Maximum line of sight of the player.

    int[] LEVEL_ENEMY_COUNTS = { 5, 8, 12, 18, 36 };

    enum Tile { Blank, Blank2, Wall, Stone, Brick, Brick2, Brick3, Floor, Floor2, Floor3, Door };

    // Node refs     ------------------------------------

    private PackedScene EnemyScene;
    Node2D WorldNode;
    TileMap TileMap;
    TileMap VisibilityMap;
    Sprite Player;
    Sprite Stairs;
    Rect2 Borders;

    Walker Walker;


    Vector2 PlayerTile;
    HashSet<Vector2> Map = new HashSet<Vector2>();
    List<Vector2> StepHistory = new List<Vector2>();
    List<Sprite> AllEnemies = new List<Sprite>();


    /** 
        Initialise variables. generates a level. 
    */
    public override void _Ready()
    {
        this.WorldNode = GetNode<Node2D>("World");
        this.TileMap = GetNode<TileMap>("TileMap");
        this.VisibilityMap = GetNode<TileMap>("VisibilityMap");
        this.Player = GetNode<Sprite>("Player");
        this.Stairs = GetNode<Sprite>("Stairs");
        this.EnemyScene = (PackedScene)GD.Load("res://Enemy.tscn");
        this.PlayerTile = new Vector2(Player.Position.x / TILE_SIZE, Player.Position.y / TILE_SIZE);
        OS.SetWindowSize(new Vector2(1280, 720));
        this.NewLevel();
    }

    public void NewLevel()
    {
        this.ResetTiles();
        this.GenerateLevel();
        this.ResetVisibility();
        CallDeferred("UpdateLineOfSight");       //Required so that visibilitymap properly overlays tilemap.     
    }

    /**
        Handles all key inputs.
    */
    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            this.NewLevel();
        }

        if (Input.IsActionPressed("Left"))
        {
            this.TryMove(-1, 0);
            Player.SetFlipH(true);
        }
        else if (Input.IsActionPressed("Right"))
        {
            this.TryMove(1, 0);
            Player.SetFlipH(false);
        }
        else if (Input.IsActionPressed("Up"))
        {
            this.TryMove(0, -1);
        }
        else if (Input.IsActionPressed("Down"))
        {
            this.TryMove(0, 1);
        }
    }

    /**
        Sets all tiles in the tilemap to blank.
    */
    public void ResetTiles()
    {
        this.Borders = new Rect2(0, 0, BorderX, BorderY);

        for (int x = 0; x < BorderX + 1; x++)
        {
            for (int y = 0; y < BorderY + 1; y++)
            {

                TileMap.SetCellv(new Vector2(x, y), -1);
            }
        }
    }


    /**
        Resets all visibility tiles so they show as black. 
    */
    public void ResetVisibility()
    {
        for (int x = 0; x < BorderX + 1; x++)
        {
            for (int y = 0; y < BorderY + 1; y++)
            {

                VisibilityMap.SetCellv(new Vector2(x, y), 0);
            }
        }

    }

    /**
        Calculates the center of the players tile,
        raycasts from the center of the players tile to the nearest corner of surrounding tiles within players line of sight.
        if the ray intersects a tile, the tile will be revealed on the visibility map.
    */
    public void UpdateLineOfSight()
    {

        Vector2 PlayerCenter = TileToPixelCenter(PlayerTile.x, PlayerTile.y);
        var spaceState = GetWorld2d().DirectSpaceState;
        for (int x = (int)PlayerTile.x - LineOfSight; x < PlayerTile.x + LineOfSight; x++)
        {
            for (int y = (int)PlayerTile.y - LineOfSight; y < PlayerTile.y + LineOfSight; y++)
            {
                if (VisibilityMap.GetCell(x, y) == 0)
                {
                    var xDir = 1;
                    var yDir = 1;
                    if (x > PlayerTile.x) { xDir = -1; }
                    if (y > PlayerTile.y) { yDir = -1; }
                    Vector2 TestPoint = TileToPixelCenter(x, y) + (new Vector2(xDir, yDir) * (TILE_SIZE / 2));
                    var Occlusion = spaceState.IntersectRay(PlayerCenter, TestPoint);

                    if (Occlusion.Count < 1 || ((Vector2)Occlusion["position"] - TestPoint).Length() < 1)
                    {
                        VisibilityMap.SetCellv(new Vector2(x, y), -1);
                    }
                }
            }
        }
    }

    /**
        Helper method for UpdateLineOfSight that returns the center coordinates of a tile.
    */

    public Vector2 TileToPixelCenter(float x, float y)
    {
        return new Vector2((float)(x + 0.5) * TILE_SIZE, (float)(y + 0.5) * TILE_SIZE);

    }


    /**
        Checks to see if the goal tile is within the walkable map. Moves the player to the tile if it is.
        If the player lands on stairs, resets all tiles and generates a new level.

    */
    public void TryMove(int dx, int dy)
    {
        int NewX = (int)PlayerTile.x + dx;
        int NewY = (int)PlayerTile.y + dy;

        if (NewX >= 0 && NewX < BorderX
        && NewY >= 0 && NewY < BorderY
        && this.Map.Contains(new Vector2(NewX, NewY)))
        {
            this.PlayerTile = new Vector2(NewX, NewY);

            Player.Position = new Vector2(NewX * TILE_SIZE, NewY * TILE_SIZE);
            if (Player.Position == Stairs.Position)
            {
                this.NewLevel();
            }
        }
        else
        {
            GD.Print("Outside map bounds");
        }
        CallDeferred("UpdateLineOfSight");

    }


    /**
        Creates a new walker object which randomly generates a new room. Creating hallways and rooms.
        Gets the walkable tiles from the walker object and carves out rooms and hallways from the tilemap. 
        Calls draw walls which surrounds the walkable tiles in walls.  
        
    */
    public void GenerateLevel()
    {

        RemoveEnemies();

        Rect2 SmallerBorders = new Rect2(2, 2, BorderX - 2, BorderY - 2);
        this.Walker = new Walker(Player.Position / TILE_SIZE, SmallerBorders);
        Walker.Walk(500);
        this.StepHistory = Walker.getStepHistory();
        Map = new HashSet<Vector2>(this.StepHistory);
        List<int> FloorTiles = new List<int> { 7, 8, 9 };

        foreach (Vector2 Location in Map)
        {

            int Index = Rand.Next(FloorTiles.Count);
            TileMap.SetCellv(Location, FloorTiles[Index]);      //Carves out rooms and halls for each walked cell in the map. 
        }
        this.DrawAllWalls(Map, new List<int> { 2 }, false);
        this.Stairs.Position = (Walker.GetStairsPosition() * TILE_SIZE);
        foreach (HashSet<Vector2> Room in Walker.GetAllRooms())
        {
            this.DrawAllWalls(Room, new List<int> { 4, 5, 6 }, true);
        }

        this.PlaceEnemies();
        //CallDeferred("UpdateLineOfSight");
    }

    /**
        Called by generate level. places enemies in random locations on the map. Won't let two enemies spawn on top of eachother, the player or the stairs.  
    */
    public void PlaceEnemies()
    {
        int TotalEnemies = LEVEL_ENEMY_COUNTS[0];
        var Rooms = Walker.GetAllRooms();

        for (int i = 0; i < TotalEnemies; i++)
        {
            List<Vector2> RoomTiles = new List<Vector2>(Rooms[Rand.Next(Rooms.Count - 1)]);
            var RandomTile = RoomTiles[Rand.Next(RoomTiles.Count - 1)];
            var RandomPosition = RandomTile * TILE_SIZE;
            bool Blocked = false;
            foreach (Enemy Enemy in this.AllEnemies)
            {
                if (RandomPosition == Enemy.Position || RandomPosition == Stairs.Position || RandomPosition == Player.Position)
                    Blocked = true;
                break;
            }
            if (!Blocked)
            {

                //INCOMPLETE. 

                //Enemy Enemy = new Enemy((Sprite) EnemyScene.Instance(), 1, RandomPosition);
                //Enemy.SetPosition(RandomPosition);
                //this.WorldNode.AddChild(Enemy.GetSpriteNode());
                //this.AllEnemies.Add(Enemy);
                //GD.Print("Enemy Spawned");
            }
        }
    }




    //Checks all surrounding tiles in roomtiles to see if they're contained. if not, place a wall.
    // the List contains indexes for different tile sprites. A random sprite is chosen from the list.  

    public void DrawAllWalls(HashSet<Vector2> RoomTiles, List<int> Tiles, bool RoomWall)
    {
        foreach (Vector2 Location in RoomTiles)
        {
            this.TryDrawWall(new Vector2(Location.x - 1, Location.y), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x + 1, Location.y), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x, Location.y - 1), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x, Location.y + 1), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x - 1, Location.y - 1), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x - 1, Location.y + 1), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x + 1, Location.y - 1), Tiles, RoomWall);
            this.TryDrawWall(new Vector2(Location.x + 1, Location.y + 1), Tiles, RoomWall);
        }
    }


    /**
        Draws walls surrounding the map. If the walls are surrounding a room, attempts to place a door in empty 
        squares.    
    */
    public void TryDrawWall(Vector2 Neighbour, List<int> Tiles, bool RoomWall)
    {
        if (Map.Contains(Neighbour))
        {
            if (RoomWall)
            {
                Vector2 LeftTile = new Vector2(Neighbour.x - 1, Neighbour.y);
                Vector2 RightTile = new Vector2(Neighbour.x + 1, Neighbour.y);
                Vector2 TopTile = new Vector2(Neighbour.x, Neighbour.y - 1);
                Vector2 BotTile = new Vector2(Neighbour.x, Neighbour.y + 1);

                if (this.DoorSpaceAvailable(Neighbour, LeftTile, RightTile, TopTile, BotTile))
                    TileMap.SetCellv(Neighbour, (int)Tile.Door);
            }
            return;
        }
        int Index = Rand.Next(Tiles.Count);         //Selects a random variation of the wall tile. 
        TileMap.SetCellv(Neighbour, Tiles[Index]);
    }

    /**
        Checks to see if the main tile is adjacent to another door tile. Returns true if that isn't the case. 
    */
    public bool DoorSpaceAvailable(Vector2 MainTile, Vector2 LeftTile, Vector2 RightTile, Vector2 TopTile, Vector2 BotTile)
    {
        if (!Map.Contains(LeftTile) && !Map.Contains(RightTile)
            && TileMap.GetCellv(LeftTile) != (int)Tile.Door && TileMap.GetCellv(RightTile) != (int)Tile.Door)
        {
            return true;

        }
        if (!Map.Contains(TopTile) && !Map.Contains(BotTile)
            && TileMap.GetCellv(TopTile) != (int)Tile.Door && TileMap.GetCellv(BotTile) != (int)Tile.Door)
        {
            return true;
        }

        return false;
    }

    /**
        removes all enemy children and clears the list of enemies. 
    */
    public void RemoveEnemies()
    {

        foreach (Enemy Enemy in AllEnemies)
        {
            this.RemoveChild(Enemy.GetSpriteNode());
        }
        this.AllEnemies = new List<Sprite>();
    }

}
