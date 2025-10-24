using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class GameMNGR_Script : Node2D
{
    [Export] Camera2D FocusCam;
    [Export] Label UnitInfoGuiLabel;
    [Export] GUIButtonsToPawnScript GBTPS;
    [Export] VBoxContainer LogBucket;
    [Export] ScrollContainer KontenrLogów;
    string SceneToLoad = "res://Scenes/MultiGameOverScreen.tscn";
    public int Round = 1;
    public string Turn = "";
    public bool ChosenActionFinished = true;
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    public class TeamConfig
    {
        public string name { get; set; }
        public Color team_colour { get; set; }
        public int PawnCount { get; set; }
        public bool AI_Active { get; set; }
        public int Spawn_ID { get; set; }
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
    List<PawnBaseFuncsScript> ActiveTeamPawns = new List<PawnBaseFuncsScript>();
    Node2D PopUpRef;
    Node2D PawnBucketRef;
    SamplePopUpScript PopUpRefScript;
    Label GameInfoLabelRef;
    CanvasLayer CamUICanvasRef;
    PawnSpawnerScript pawnSpawnerScript;
    public bool SetupDone = false;
    private bool AccessNextTurnPopup;
    private int IntForUnitSelection = 0;
    public string Winner;
    public override void _Ready()
    {
        Instance = this;
    }

    public void SetupGameScene()
    {
        UnitInfoGuiLabel.Text = "";
        PawnBucketRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/UnitsBucket");
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
        CalculateActiveTeamPawns(true);
        SetupDone = true;
    }
    public void SelectPawn(PawnBaseFuncsScript pawn)
    {
        if (ChosenActionFinished == true)
        {
            if (SelectedPawn != null) // trzeba wysłać reset do skryptu gracza bo inaczej zaznaczenie się zduplikuje 
            {
                SelectedPawn.Call("ShowSelection", false);
                SelectedPawn.Call("RSSP");
            }
            UnitInfoGuiLabel.Text = $"{pawn.UnitType}\n{pawn.UnitName}\nHP ({Mathf.RoundToInt((float)pawn.Integrity / (float)pawn.BaseIntegrity * 100f)}%)\nMP ({pawn.MP})";
            SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
            GBTPS.ShowActions(); // pokaż akcje które może podjąć pionek na GUI
            //GD.Print($"Selected Pawn Now is {SelectedPawn}");
            if (FocusCam != null)
            {
                FocusCam.Position = pawn.GlobalPosition;
                pawn.Call("ShowSelection" , true);
            }
            //GD.Print($"Selected pawn is {SelectedPawn}");
        }
        else
        {
            GD.Print("Nie można zaznaczyć pionka bo gracz nie zfinalizował akcji");
        }
    }
    public void DeselectPawn()
    {
        SelectedPawn = null;
        UnitInfoGuiLabel.Text = "";
    }
    public void PlayerPhoneCallback()
    {
        GBTPS.PALO(); // aktywacja widoczności potwierdzenia akcji
    }
    public void CaptureAction(Vector2 Giver, Vector2 Recypiant)
    {
        FocusCam.GlobalPosition = Recypiant; // to jest leprzy placeholder, bo upiększanie tego byłoby trochę teraz nieistotne
        //FocusCam.GlobalPosition = (Giver + Recypiant) / 2; // zmęczony jestem i mam wydupione 
    }
    void Button_ACT1() // to samo co spacja robi
    {
        AccessNextTurnPopup = true;
    }
    void Button_ACT2() // next unit
    {
        CalculateActiveTeamPawns(false);
        //ActiveTeamPawns.RemoveAll(p => p == null || !p.IsInsideTree());
        if (ActiveTeamPawns.Count == 0)
        {
            GD.Print("Nie ma pionka do którego możnaby przejść");
            return;
        }

        if (IntForUnitSelection >= ActiveTeamPawns.Count)
        {
            IntForUnitSelection = 0;
        }
        // Znajdź najbliższą nie-null jednostkę
        int startIndex = IntForUnitSelection;
        do
        {
            var NextPawn = ActiveTeamPawns[IntForUnitSelection];
            if (NextPawn != null && NextPawn.IsInsideTree())
            {
                SelectPawn(NextPawn);

                IntForUnitSelection = (IntForUnitSelection + 1) % ActiveTeamPawns.Count;
                //GD.Print($"Wybrano jednostkę o indeksie {IntForUnitSelection}");
                return;
            }
            IntForUnitSelection = (IntForUnitSelection + 1) % ActiveTeamPawns.Count;
        }
        while (IntForUnitSelection != startIndex);       
    }
    void Button_ACT3() // prev unit 
    {
        CalculateActiveTeamPawns(false);
        //ActiveTeamPawns.RemoveAll(p => p == null || !p.IsInsideTree());
        if (ActiveTeamPawns.Count == 0)
        {
            GD.Print("Nie ma pionka do którego możnaby przejść");
            return;
        }

        if (IntForUnitSelection <= 0)
        {
            IntForUnitSelection = ActiveTeamPawns.Count - 1; // pierdole, działa częściowo, mam wyjebane
        }
        // Znajdź najbliższą nie-null jednostkę
        int startIndex = IntForUnitSelection;
        do
        {
            var NextPawn = ActiveTeamPawns[IntForUnitSelection];
            if (NextPawn != null && NextPawn.IsInsideTree())
            {
                SelectPawn(NextPawn);

                IntForUnitSelection = (IntForUnitSelection - 1) % ActiveTeamPawns.Count;
                //GD.Print($"Wybrano jednostkę o indeksie {IntForUnitSelection}");
                return;
            }
            IntForUnitSelection = (IntForUnitSelection - 1) % ActiveTeamPawns.Count;
        }
        while (IntForUnitSelection != startIndex);   
    }
    public override void _Process(double delta)
    {
        if (SetupDone)
        {
            GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny
            if (Input.IsActionJustPressed("MYSPACE"))
            {
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
    public void GenerateActionLog(string Message)
    {
        Label Log = new Label();
        Log.Text = Message;
        Log.AddThemeFontSizeOverride("font_size", 110);
        LogBucket.AddChild(Log);
        KontenrLogów.ScrollVertical = (int)KontenrLogów.GetVScrollBar().MaxValue;
    }
    void CalculateActiveTeamPawns(bool FirstTime)
    {
        if (FirstTime == true)
        {
            IntForUnitSelection = 0;
        }        
        ActiveTeamPawns.Clear();
        foreach (PawnBaseFuncsScript TeamPawn in PawnBucketRef.GetChildren())
        {
            if (TeamPawn.TeamId == Turn)
            {
                //GD.Print($"do listy dodano {TeamPawn.Name}");
                ActiveTeamPawns.Add(TeamPawn);
            }
        }
        //GD.Print("Dodano pionki do listy odczytu dla teamu");
    }
    void NextRoundFunc()
    {
        GD.Print("New round!");
        RecalculationTeamStatus();
        foreach (var log in LogBucket.GetChildren())
        {
            log.QueueFree();
        }
        Round++;
        // przetasuj kolejność wg pionków
        ActiveTeams.Sort((a, b) => b.PawnCount.CompareTo(a.PawnCount));
        TeamTurnTable.Clear();
        foreach (var team in ActiveTeams)
        {
            TeamTurnTable.Add(team.name);
        }

        Turn = TeamTurnTable[0];
        GenerateActionLog($"## Round {Round} ##");
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
        if (ActiveTeams.Count <= 1)
        {
            Winner = ActiveTeams[0].name;
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
        CalculateActiveTeamPawns(true);
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
        GenerateActionLog($"## Team {Turn} starts thier Turn ##");
    }
}
