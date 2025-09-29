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
    public override void _Ready()
    {
        // zbyt późno niektóre ustawienia się ustawiały zostały przeniesione wyżej 
    }
    void SpawnSelectedPawns()
    {
        PawnScene = GD.Load<PackedScene>("res://Prefabs/pawn_base_prefab.tscn");
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
            Node2D Pawn = PawnScene.Instantiate<Node2D>();
            PawnBucketRef.AddChild(Pawn);
            Pawn.Call("SetTeam", team.name, team.team_colour);
            
        }
    }
}
