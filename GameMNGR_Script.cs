using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class GameMNGR_Script : Node2D
{
    string SceneToLoad = "res://Scenes/MultiGameOverScreen.tscn";
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
        public List<UnitSelection> UnitsForThisTeam { get; set; } = new List<UnitSelection>();
    }
    public class UnitSelection
    {
        public string ScenePath { get; set; } // ścierzka do prefabu pionka
        public int Count { get; set; } // iloćś pionków dla danego teamu (liczba na typ) TO DO .: (chwilowo nie uwzględnia wyposarzenia) poprawić na system uwzględniający wyposarzenie jednostki
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
    private bool AccessNextTurnPopup;
    public override void _Ready()
    {
        Instance = this;
    }

    public void SetupGameScene()
    {
        pawnSpawnerScript = GetNode<PawnSpawnerScript>("SpawnPoints");
        CamUICanvasRef = GetTree().Root.GetNode<CanvasLayer>("BaseTestScene/Camera2D/CanvasLayer");
        Vector2I windowSize = DisplayServer.WindowGetSize();
        //CamUICanvasRef.Offset = new Vector2(windowSize.X / 2, windowSize.Y / 2);
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
    void Button_ACT1() // to samo co spacja robi
    {
        AccessNextTurnPopup = true;
    }
    public override void _Process(double delta)
    {
        if (SetupDone)
        {
            GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny
            if (Input.IsActionJustPressed("MYSPACE")){
                AccessNextTurnPopup = true;
            }
            if (AccessNextTurnPopup == true && PopUpRefScript != null)
            {
                if (TeamTurnTable.Count <= 1)
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end the round ?", true);
                }
                else
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?", false);
                }
                AccessNextTurnPopup = false;
            }

        }
    }
    void NextRoundFunc()
    {
        GD.Print("New round!");
        RecalculationTeamStatus();
        Round++;
        // przetasuj kolejność wg pionków
        ActiveTeams.Sort((a, b) => b.PawnCount.CompareTo(a.PawnCount));

        TeamTurnTable.Clear();
        foreach (var team in ActiveTeams)
        {
            TeamTurnTable.Add(team.name);
        }

        Turn = TeamTurnTable[0];
    }
    void RecalculationTeamStatus() // podlicza żywe drużyny, dodaje pionkom MP, wyznacza wygraną
    {
        foreach (TeamConfig ActiveTeam in ActiveTeams)
        {
            ActiveTeam.PawnCount = 0;
        }
        var UnitsBucket = GetNode<Node>("UnitsBucket");
        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                foreach (TeamConfig ActiveTeam in ActiveTeams)
                {
                    if (ActiveTeam.name == pawn.TeamId)
                    {
                        ActiveTeam.PawnCount++;
                    }
                }
                GD.Print("reset MP dokonany");
                pawn.Call("ResetMP");
            }
        }
        ActiveTeams.RemoveAll(t => t.PawnCount <= 0); //nieaktywna drużyna generalnie 
        TeamTurnTable.RemoveAll(name => !ActiveTeams.Exists(t => t.name == name)); //nieaktywna drużyna tej rundy 
        // jeśli obecna tura należy do martwej drużyny — przeskocz
        if (!ActiveTeams.Exists(t => t.name == Turn))
        {
            if (TeamTurnTable.Count > 0)
                Turn = TeamTurnTable[0];
            else
                GD.Print("Brak drużyn do gry");
        }
        // jeśli jedna drużyna została → koniec gry
        if (ActiveTeams.Count <= 1)
        {
            GD.Print("Game Over! Winner: " + (ActiveTeams.Count == 1 ? ActiveTeams[0].name : "None"));
            GetTree().ChangeSceneToFile(SceneToLoad);
            return;
        }

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
        RecalculationTeamStatus();
        GD.Print($"koniec rundy dla drużyny {TeamTurnTable[0]}");
        TeamTurnTable.RemoveAt(0);
        while (TeamTurnTable.Count > 0 && !ActiveTeams.Exists(t => t.name == TeamTurnTable[0]))
        {
            GD.Print($"drużyna {TeamTurnTable[0]} usunięta z racji na brak pionków ");
            TeamTurnTable.RemoveAt(0);
        }
        if (TeamTurnTable.Count == 0)
        {
            GD.Print("No teams left to take turns!");
            return;
        }
        // nowa drużyna
        Turn = TeamTurnTable[0];
    }
}
