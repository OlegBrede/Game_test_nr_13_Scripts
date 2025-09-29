using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class GameMNGR_Script : Node2D
{
    public int Round = 1;
    public string Turn = "";
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    public class TeamConfig
    {
        public string name { get; set; }
        public Color team_colour { get; set; }
        public int PawnCount { get; set; }
        public bool AI_Active { get; set; }
    }

    public class GameConfig
    {
        public List<TeamConfig> teams { get; set; } = new List<TeamConfig>(); // lista do inicjalizacji w konfiguracji
    }
    public List<string> TeamTurnTable = new List<string>(); // lista do tur
    public List<TeamConfig> ActiveTeams = new List<TeamConfig>(); // aktywne w danej grze 
    Node2D PopUpRef;
    SamplePopUpScript PopUpRefScript;
    Label GameInfoLabelRef;
    CanvasLayer CamUICanvasRef;
    PawnSpawnerScript pawnSpawnerScript;
    public bool SetupDone = false;
    public override void _Ready()
    {
        Instance = this;
    }

    public void SetupGameScene()
    {
        pawnSpawnerScript = GetNode<PawnSpawnerScript>("SpawnPoints");
        Vector2I windowSize = DisplayServer.WindowGetSize();
        CamUICanvasRef = GetTree().Root.GetNode<CanvasLayer>("BaseTestScene/Camera2D/CanvasLayer");
        CamUICanvasRef.Offset = new Vector2(windowSize.X / 2, windowSize.Y / 2);
        PopUpRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/Camera2D/CanvasLayer/SamplePopUp");
        GameInfoLabelRef = GetTree().Root.GetNode<Label>("BaseTestScene/Camera2D/CanvasLayer/GameInfoLabel");
        PopUpRefScript = PopUpRef as SamplePopUpScript;
        pawnSpawnerScript.Call("SpawnSelectedPawns");
        string json = File.ReadAllText(SaveFilePath);
        var cfg = JsonSerializer.Deserialize<GameConfig>(json);
        InitTurnOrder(cfg);
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
        if (SetupDone)
        {
            GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny

            if (Input.IsActionJustPressed("MYSPACE") && PopUpRefScript != null)
            {
                if (TeamTurnTable.Count <= 1)
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end the round ?", true);
                }
                else
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?", false);
                }
            }
        }
    }
    void NextRoundFunc()
    {
        GD.Print("New round!");
        Round++;

        // reset MP wszystkim pionkom
        var UnitsBucket = GetNode<Node>("UnitsBucket");
        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                pawn.Call("ResetMP");
            }
        }

        // przefiltruj drużyny – usuń martwe
        ActiveTeams.RemoveAll(t => t.PawnCount <= 0);
        // jeśli jedna drużyna została → koniec gry
        if (ActiveTeams.Count <= 1)
        {
            GD.Print("Game Over! Winner: " + (ActiveTeams.Count == 1 ? ActiveTeams[0].name : "None"));
            return;
        }

        // przetasuj kolejność wg pionków
        ActiveTeams.Sort((a, b) => b.PawnCount.CompareTo(a.PawnCount));

        TeamTurnTable.Clear();
        foreach (var team in ActiveTeams)
        {
            TeamTurnTable.Add(team.name);
        }

        Turn = TeamTurnTable[0];
    }

    public void InitTurnOrder(GameConfig cfg)
    {
        ActiveTeams = new List<TeamConfig>(cfg.teams);

        // sortuj wg liczby pionków malejąco
        ActiveTeams.Sort((a, b) => b.PawnCount.CompareTo(a.PawnCount));

        // wyciągnij same nazwy do kolejki tur
        TeamTurnTable.Clear();
        foreach (var team in ActiveTeams)
        {
            if (team.PawnCount > 0)
                TeamTurnTable.Add(team.name);
        }

        if (TeamTurnTable.Count > 0)
        {
            Turn = TeamTurnTable[0]; // zaczyna pierwsza drużyna
        }
    }
    void NextTurnFunc()
    {
        if (TeamTurnTable.Count == 0)
            return;
        // przesuwamy obecną drużynę na koniec kolejki
        TeamTurnTable.RemoveAt(0);
        // nowa drużyna
        Turn = TeamTurnTable[0];
    }
}
