using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class PawnSpawnerScript : Node2D
{
    GameMNGR_Script gameMNGR_Script;
    
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    PackedScene PawnScene;
    Node2D PawnBucketRef;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    [Export] RngNameToolScript rngNameToolScript;
    [Export] Node2D Spawn1;
    [Export] Node2D Spawn2;
    [Export] Node2D Spawn3;
    [Export] Node2D Spawn4;
    [Export] Node2D Spawn5;
    [Export] Node2D Spawn6;
    [Export] Node2D Spawn7;
    [Export] Node2D Spawn8;

    public override void _Ready()
    {
        RNGGEN.Randomize();
    }

    void SpawnSelectedPawns()
    {
        rngNameToolScript.LoadNames("res://Mem Bank/RNGNameList.json"); // pierdole
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        PawnBucketRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/UnitsBucket");
        if (!File.Exists(SaveFilePath))
        {
            GD.Print("Brak pliku JSON!");
            return;
        }
        string json = File.ReadAllText(SaveFilePath);
        var cfg = JsonSerializer.Deserialize<GameMNGR_Script.GameConfig>(json);
        foreach (var team in cfg.teams)
        {
            GD.Print($"Drużyna: {team.name}");
            foreach (var TeamsPawn in team.UnitsForThisTeam)
            {
                for (int i = 0; i < TeamsPawn.Count; i++)
                {
                    PawnScene = GD.Load<PackedScene>(TeamsPawn.ScenePath);
                    Node2D Pawn = PawnScene.Instantiate<Node2D>();
                    PawnBucketRef.AddChild(Pawn);
                    Pawn.Call("SetTeam", team.name, team.team_colour);
                    Pawn.Call("ActivateCollision");
                    Pawn.Call("DeleteUnusedControlNodes", team.AI_Active);
                    int Gender = RNGGEN.RandiRange(0, 1);
                    string Category;
                    string Surname;
                    if (Gender == 1)
                    {
                        Category = "male";
                        Surname = rngNameToolScript.GetRandomName("surname");
                    }
                    else
                    {
                        Category = "female";
                        Surname = rngNameToolScript.GetRandomName("surname");
                        char lastChar = Surname[Surname.Length - 1];
                        // jeśli kończy się na "i" – zamień na "a"
                        if (lastChar == 'i')
                        {
                            Surname = Surname.Substring(0, Surname.Length - 1) + "a";
                        }
                        if (lastChar == 'y')
                        {
                            Surname = Surname.Substring(0, Surname.Length - 1) + "a";
                        }
                    }
                    Pawn.Call("Namechange",rngNameToolScript.GetRandomName(Category) + " " + Surname);
                    Pawn.GlobalPosition = new Vector2(SpawnPointPos(team.Spawn_ID).X + RNGGEN.RandfRange(-1000f, 1000f),SpawnPointPos(team.Spawn_ID).Y + RNGGEN.RandfRange(-1000f, 1000f));
                    //GD.Print($"This pawns global pos .: {Pawn.GlobalPosition}");
                }
            }
        }
    }
    Vector2 SpawnPointPos(int ChosenSpawnIDNum)
    {
        switch (ChosenSpawnIDNum)
        {
            case 1:
                return Spawn1.GlobalPosition;
                case 2:
                return Spawn2.GlobalPosition;
                case 3:
                return Spawn3.GlobalPosition;
                case 4:
                return Spawn4.GlobalPosition;
                case 5:
                return Spawn5.GlobalPosition;
                case 6:
                return Spawn6.GlobalPosition;
                case 7:
                return Spawn7.GlobalPosition;
                case 8:
                return Spawn8.GlobalPosition;
            default:
                GD.Print("spawn nie został ustalony");
                return new Vector2(0,0);
        }
    }
}
