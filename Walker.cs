using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public class Walker : Godot.Node
{

     Random Rand = new Random();
     Vector2[] DIRECTIONS = { Vector2.Right, Vector2.Left, Vector2.Up, Vector2.Down };
    Vector2 Position = Vector2.Zero;
    Vector2 Direction = Vector2.Right;
    Rect2 Borders = new Rect2();        //Sets the boundaries of the map. 
    List<Vector2> StepHistory;
    List<HashSet<Vector2>> RoomWalls = new List<HashSet<Vector2>>();    //Contains all the rooms in a list. Each room has a hashset of wall cells. 
    Vector2 StairsPosition = Vector2.Zero;
    int StepsSinceTurn = 0;

    int MaxStepsUntilTurn = 5;

    int RoomOdds = 6;       // Sets the probability of a room being generated when the walker takes a step. 
   

    List<HashSet<Vector2>> Rooms;


    public Walker(Vector2 StartingPosition, Rect2 NewBorders)
    {

        this.Borders = NewBorders;
        this.StepHistory = new List<Vector2>();
        this.Rooms = new List<HashSet<Vector2>>();
        int Index = Rand.Next(DIRECTIONS.Length);       //Choose a random starting direction.
        this.Direction = DIRECTIONS[Index];
        if (!NewBorders.HasPoint(StartingPosition))
        {
            GD.Print("Error: Starting point not contained in borders");
            return;
        }
        this.Position = StartingPosition;
        this.StepHistory.Add(Position);

    }

    /**
        Main driving method of the walker class. Randomly walks a given number of steps. 
        The walker will change direction when MaxSteptUntilTurn is reached. 
    */
    public List<Vector2> Walk(int Steps)
    {
        this.CreateRoom(Position);

        while (StepHistory.Count < Steps)
        {
            if (StepsSinceTurn >= MaxStepsUntilTurn)
            {
                this.ChangeDirection();
            }

            if (this.Step())
            {
                StepHistory.Add(Position);
            }
            else
            {
                this.ChangeDirection();
            }

        }

        this.StairsPosition = Position;
        this.CreateRoom(Position);
        return StepHistory;
    }

    /**
        Returns the walkers stair position. 
    */
    public Vector2 GetStairsPosition()
    {
        return this.StairsPosition;
    }


    /**
        Returns the walkers step history. 
    */
    public List<Vector2> getStepHistory()
    {
        return StepHistory;
    }

    
    /** 
        Takes a step in the walkers given direction. Returns true if the step was successful. 
    */
    public bool Step()
    {
        Vector2 TargetPosition = Position + Direction;
        if (Borders.HasPoint(TargetPosition))
        {
            StepsSinceTurn += 1;
            Position = TargetPosition;
            return true;
        }
        else
        {
            return false;
        }
    }

    /**
        Randomly chooses a new direction for the walker. Has a chance to create a new room at the walkers current location.
    */
    public void ChangeDirection()
    {
        if (Rand.Next(RoomOdds) <= 1) { this.CreateRoom(Position); }

        StepsSinceTurn = 0;
        List<Vector2> OtherDirections = new List<Vector2>(DIRECTIONS);
        OtherDirections.Remove(Direction);

        Direction = OtherDirections[Rand.Next(OtherDirections.Count)];
        while (!Borders.HasPoint(Position + Direction))
        {
            Direction = OtherDirections[Rand.Next(OtherDirections.Count)];
        }

    }

    /**
        Creates a room with a random width and height. Adds a hashset of the room tiles to the list of rooms. 
    */
    public List<Vector2> CreateRoom(Vector2 RoomPosition)
    {

        Vector2 Size = new Vector2(3 + Rand.Next(4), 3 + Rand.Next(4));
        Vector2 TopLeftCorner = new Vector2(Position.x - ((int)Size.x / 2), Position.y - ((int)Size.y / 2));
        HashSet<Vector2> CurrentRoomTiles = new HashSet<Vector2>();
        HashSet<Vector2> CurrentRoomWalls = new HashSet<Vector2>();
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2 CurrentTile = new Vector2(x, y);
                Vector2 NewStep = TopLeftCorner + new Vector2(x, y);
                

                if (Borders.HasPoint(NewStep))
                {
                    this.StepHistory.Add(NewStep);
                    CurrentRoomTiles.Add(NewStep);
                }
            }
        }
        this.Rooms.Add(CurrentRoomTiles);
        return StepHistory;
    }

    /**
        Returns a list of all rooms. 
    */
    public List<HashSet<Vector2>> GetAllRooms()
    {
        return this.Rooms;
    }

 
    /**
        Returns the hashset of room walls. 
    */
    public List<HashSet<Vector2>> GetAllRoomWalls()
    {
        return this.RoomWalls;
    }



}






