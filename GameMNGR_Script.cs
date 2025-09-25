using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class GameMNGR_Script : Node2D
{
    public int Round = 1;
    public int Turn = 1;
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    public class TeamConfig
    {
        public string name { get; set; }
        public string team_colour { get; set; }
        public int PawnCount { get; set; }
        public bool AI_Active { get; set; }
    }

    public class GameConfig
    {
        public List<TeamConfig> teams { get; set; } = new List<TeamConfig>();
    }

    Node2D PopUpRef;
    Node2D PawnBucketRef;
    SamplePopUpScript PopUpRefScript;
    Label GameInfoLabelRef;
    CanvasLayer CamUICanvasRef;
    PackedScene PawnScene;
    public bool SetupDone = false;
    public override void _Ready()
    {
        Instance = this;
    }

    public void SetupGameScene()
    {
        PawnBucketRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/UnitsBucket");

        Vector2I windowSize = DisplayServer.WindowGetSize();
        CamUICanvasRef = GetTree().Root.GetNode<CanvasLayer>("BaseTestScene/Camera2D/CanvasLayer");
        CamUICanvasRef.Offset = new Vector2(windowSize.X / 2, windowSize.Y / 2);
        PopUpRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/Camera2D/CanvasLayer/SamplePopUp");
        GameInfoLabelRef = GetTree().Root.GetNode<Label>("BaseTestScene/Camera2D/CanvasLayer/GameInfoLabel");
        PopUpRefScript = PopUpRef as SamplePopUpScript;
        PawnScene = GD.Load<PackedScene>("res://Prefabs/pawn_base_prefab.tscn");
        if (!File.Exists(SaveFilePath))
        {
            GD.Print("Brak pliku JSON!");
            return;
        }
        string json = File.ReadAllText(SaveFilePath);
        var cfg = JsonSerializer.Deserialize<GameConfig>(json);
        foreach (var team in cfg.teams)
        {
            GD.Print($"Drużyna: {team.name}");
            Node2D Pawn = PawnScene.Instantiate<Node2D>();
            PawnBucketRef.AddChild(Pawn);
            Pawn.Call("SetTeam",team.name);
        }

        SetupDone = true;
    }
    public void SelectPawn(PawnBaseFuncsScript pawn)
    {
        SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
        //GD.Print($"Selected pawn is {SelectedPawn}");
    }
    public void DeselectPawn() => SelectedPawn = null;
    public override void _Process(double delta)
    {
        if (SetupDone) {
           GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny

            if (Input.IsActionJustPressed("MYSPACE") && PopUpRefScript != null)
            {
                PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?");
            } 
        }
    }
    void NextRoundFunc()
    {
        var UnitsBucket = GetNode<Node>("UnitsBucket");

        foreach (Node child in UnitsBucket.GetChildren()) // czy trzeba to zmienić na sprawdzanie listy? nie wiem, sprawdzanie pawnbucket nie jest złym pomysłem  
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                pawn.Call("ResetMP"); // przykładowa funkcja w PawnBaseFuncsScript
            }
        }
        Round++;
    }
}
