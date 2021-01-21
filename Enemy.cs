using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

//FEATURE INCOMPLETE. 
public class Enemy : Sprite
{
    Sprite EnemyNode;
    ColorRect HealthBar;
    Vector2 EnemyTile = Vector2.Zero;
    int FullHP;
    int HP;
    bool Dead = false;


   public Enemy(Sprite Scene, int EnemyLevel, Vector2 Position)
    {
        this.EnemyNode = Scene;
        //this.HealthBar = EnemyNode.GetNode<ColorRect>("HealthBar");
        //this.HealthBar.RectPosition = Position;
        this.FullHP = 1 + EnemyLevel * 2;
        this.HP = this.FullHP;
        this.EnemyTile = Position;
        this.EnemyNode.Position = EnemyTile * World.TILE_SIZE;
       
        
    }

    public Enemy()
    {
        
    }

    public Vector2 GetTile()
    {
        return this.EnemyTile;
    }

    public void PrintReached()
    {
        GD.Print("Script Access Successful");
    }

    public void SetPosition(Vector2 NewPosition)
    {
        EnemyNode.Position = NewPosition;
    }

    public void SetNode(Sprite Node)
    {
        this.EnemyNode = Node;
    }

    public Sprite GetSpriteNode()
    {
        return this.EnemyNode;
    }  
}