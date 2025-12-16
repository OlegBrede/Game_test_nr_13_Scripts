using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

public partial class GameMNGR_Script : Node2D
{
    [Export] Node2D ESCMenu;
    // ####################### KAMERA #######################
    [Export] Camera2D FocusCam;
    [Export] Timer CamShowActionTimer;
    private float waitDuration = 1f;
    private Tween.TransitionType transition = Tween.TransitionType.Sine;
    private Tween.EaseType ease = Tween.EaseType.InOut;
    private Tween activeTween;
    Vector2 ActionView;
    Vector2 ReactionView;
    // ####################### KAMERA #######################
    [Export] Label UnitInfoGuiLabel;
    [Export] Label TotalMPLabel;
    [Export] Label ULTIMATENAMELABEL;
    [Export] Label SNTWN; // Show no target warning node
    [Export] VBoxContainer LogBucket;
    [Export] ScrollContainer KontenrLogów;
    [Export] public GUIButtonsToPawnScript PlayerGUIRef;
    string SceneToLoad = "res://Scenes/MultiGameOverScreen.tscn";
    public int Round = 0; // zmienić by kod wchodząc do sceny zaczynał next round i zobaczyć gdzie to nas zaniesie 
    public string Turn = "";
    public bool ChosenActionFinished = true;
    public int TeamsCollectiveMP = 0;
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    public class TeamConfig
    {
        public string name { get; set; }
        public Color team_colour { get; set; }
        public int PawnCount { get; set; }
        public int CollectiveMPCount { get; set; }
        public bool AI_Active { get; set; }
        public int Spawn_ID { get; set; }
        public List<UnitSelection> UnitsForThisTeam { get; set; } = new List<UnitSelection>();
    }
    public class UnitSelection
    {
        public string ScenePath { get; set; } // ścierzka do prefabu pionka
        public int Count { get; set; } // iloćś pionków dla danego teamu (liczba na typ) TO DO .: (chwilowo nie uwzględnia wyposarzenia) poprawić na system uwzględniający wyposarzenie jednostki
        public float ThisSpecificPawnsRadius { get; set; }
    }
    public class GameConfig
    {
        public List<TeamConfig> teams { get; set; } = new List<TeamConfig>(); // lista do inicjalizacji w konfiguracji
    }
    public List<string> TeamTurnTable = new List<string>(); // lista do tur
    public List<TeamConfig> ActiveTeams = new List<TeamConfig>(); // aktywne w danej grze
    List<PawnBaseFuncsScript> ActiveTeamPawns = new List<PawnBaseFuncsScript>();
    private PawnBaseFuncsScript PrevSelectedPawn;
    Node2D PopUpRef;
    Node2D ScrollPopUpRef;
    Node2D PawnBucketRef;
    Node UnitsBucket;
    SamplePopUpScript PopUpRefScript;
    Label GameInfoLabelRef;
    CanvasLayer CamUICanvasRef;
    PawnSpawnerScript pawnSpawnerScript;
    public bool SetupDone = false;
    private bool FirstRoundDone = false;
    private bool AccessNextTurnPopup;
    private int IntForUnitSelection = 0;
    public string Winner;
    public override void _Ready()
    {
        Instance = this;
    }

    public void SetupGameScene()
    {
        //PlayerGUIRef = GetTree().Root.GetNode<GUIButtonsToPawnScript>("BaseTestScene/Camera2D/GUI_to_Pawn_Input_Translator");
        CamShowActionTimer.Timeout += ShowReactionAfterTimeout;
        SNTWN.Visible = false;
        UnitsBucket = GetNode<Node>("UnitsBucket");
        ESCMenu.Visible = false;
        UnitInfoGuiLabel.Text = "";
        PawnBucketRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/UnitsBucket");
        pawnSpawnerScript = GetNode<PawnSpawnerScript>("SpawnPoints");
        CamUICanvasRef = GetTree().Root.GetNode<CanvasLayer>("BaseTestScene/Camera2D/CanvasLayer");
        Vector2I windowSize = DisplayServer.WindowGetSize();
        //CamUICanvasRef.Offset = new Vector2(windowSize.X / 2, windowSize.Y / 2);
        PopUpRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/Camera2D/CanvasLayer/SamplePopUp");
        GameInfoLabelRef = GetTree().Root.GetNode<Label>("BaseTestScene/Camera2D/CanvasLayer/GameInfoLabel");
        PopUpRefScript = PopUpRef as SamplePopUpScript;
        ScrollPopUpRef = GetTree().Root.GetNode<Node2D>("BaseTestScene/Camera2D/CanvasLayer/ScrollPopUp");
        ScrollPopUpRef.Visible = false;
        pawnSpawnerScript.Call("SpawnSelectedPawns");
        string json = File.ReadAllText(SaveFilePath);
        var cfg = JsonSerializer.Deserialize<GameConfig>(json);
        InitTurnOrder(cfg);
        CalculateActiveTeamPawns(true);
        NextRoundFunc();
        SetupDone = true;
    }
    public void SelectPawn(PawnBaseFuncsScript pawn)
    {
        if (ChosenActionFinished == true)
        {
            if (SelectedPawn != null) // trzeba wysłać reset do skryptu gracza bo inaczej zaznaczenie się zduplikuje 
            {
                SelectedPawn.Call("ShowSelection", false); // animacja
                SelectedPawn.Call("RSSP"); // reset selekcji, teoretycznie niepotrzebny ale... TO DO .: - sprawdź czy usunięcie resetu zepsuje grę 
            }
            SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
            SelectedPawn.SetUISubscription(); // Subskrypcja do UI
            PrevSelectedPawn = SelectedPawn;
            PlayerGUIRef.PALO(false,true); // pokaż akcje które może podjąć pionek na GUI
            PlayerGUIRef.RecivePaperdoll(pawn.PathToPaperDoll);
            pawn.CheckFightingCapability();
            foreach (PawnPart part in pawn.PawnParts)
            {
                PlayerGUIRef.ReciveWellBeingInfo(part.Name,part.HP,part.MAXHP);
            }
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
        PlayerGUIRef.DeletePaperDoll();
        SelectedPawn = null;
        UnitInfoGuiLabel.Text = "";
    }
    string PawnInfoToGUITransmision(PawnBaseFuncsScript pawn)
    {
        if (pawn != null)
        {
            if (pawn.ShootingAllowence > 0)
            {
                return $"Unit({pawn.UnitType})\nHP ({Mathf.RoundToInt((float)pawn.Integrity / (float)pawn.BaseIntegrity * 100f)}%)\nMP ({pawn.MP})\nAmmo({pawn.WeaponAmmo}/{pawn.WeaponMaxAmmo})";
            }
            else
            {
                return $"Unit({pawn.UnitType})\nHP ({Mathf.RoundToInt((float)pawn.Integrity / (float)pawn.BaseIntegrity * 100f)}%)\nMP ({pawn.MP})";
            }
        }
        else
        {
            return "";
        }
    }
    public void PlayerPhoneCallback2Flag(string CalledFuncName, bool Flag1,bool Flag2)
    {
        PlayerGUIRef.Call(CalledFuncName, Flag1,Flag2);
    }
    public void PlayerPhoneCallbackInt(string CalledFuncName, int NumVal)
    {
        PlayerGUIRef.Call(CalledFuncName, NumVal); 
    }
    public void PlayerPhoneCallbackIntBool(string CalledFuncName,int NumVal,bool Flag)
    {
        PlayerGUIRef.Call(CalledFuncName,NumVal,Flag);
    }
    public void CaptureAction(Vector2 Giver, Vector2 Recypiant)
    {
        ActionView = Giver;
        ReactionView = Recypiant;
        CamShowActionTimer.WaitTime = waitDuration;
        ShowActionAfterTimeout();
        //FocusCam.GlobalPosition = Recypiant; // to jest leprzy placeholder, bo upiększanie tego byłoby trochę teraz nieistotne
    }
    void ShowActionAfterTimeout()
    {
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ActionView,0.05f).SetTrans(transition).SetEase(ease);
        CamShowActionTimer.Start();
        //GD.Print("Timer włączony");
    }
    void ShowReactionAfterTimeout()
    {
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ReactionView,0.05f).SetTrans(transition).SetEase(ease);
        CamShowActionTimer.Stop();
        //GD.Print("Timer skończcył");
    }
    public void PlayerPhoneCallWarning(string messig)
    {
        SNTWN.Call("ShowFadeWarning", messig);
    }
    public void ShowListPopUp(List<PawnPart> PartsToShow, PawnPlayerController TwatCallin,int Hitprecent)
    {
        ScrollPopUpRef.Visible = true;
        ScrollPopUpScript SPS = ScrollPopUpRef as ScrollPopUpScript;
        SPS.GeneratePartButtons(PartsToShow, TwatCallin);
        SPS.ShowHitPrecent(Hitprecent);
    }
    public void HideListPupUp()
    {
        ScrollPopUpRef.Visible = false;
    }
    void Button_ACT1() // to samo co spacja robi
    {
        AccessNextTurnPopup = true;
    }
    void Button_ACT2() // next unit
    {
        UltimatePawnSwitchingFunc(true, false);
    }
    void Button_ACT3() // prev unit 
    {
        UltimatePawnSwitchingFunc(false, false);
    }
    void Button_ACT4() //current active
    {
        if (PrevSelectedPawn != null)
        {
            SelectPawn(PrevSelectedPawn);
        }
    }
    void Button_ACT5() // prev active
    {
        UltimatePawnSwitchingFunc(true, true);
    }
    void Button_ACT6() // next active
    {
        UltimatePawnSwitchingFunc(false, true);
    }
    void Button_ACT7() // Escape Menu
    {
        if (ESCMenu.Visible == false)
        {
            EscapeThisShithole(true);
        }
        else
        {
            EscapeThisShithole(false);
        }
    }
    void Button_ACT8() // Go to main menu
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn"); // TEMP powinien być tu jakiś zapisa aktualnego stanu lub pop-up z info że gra się nie zapisze 
    }
    void Button_ACT9() // send quit game flag
    {
        GetTree().Quit(); // TEMP tu powinien być prompt przy niezapisanej grze 
    }
    void EscapeThisShithole(bool Visibility)
    {
        ESCMenu.Visible = Visibility;
    }
    public override void _Process(double delta)
    {
        if (SetupDone)
        {
            GameInfoLabelRef.Text = $" Round {Round} | Turn {Turn}"; // zamienić na odwołanie się do tablicy statycznej z nazwą drużyny
            UnitInfoGuiLabel.Text = PawnInfoToGUITransmision(SelectedPawn);
            if (SelectedPawn != null)
            {
                ULTIMATENAMELABEL.Text = SelectedPawn.UnitName;
            }
            else
            {
                ULTIMATENAMELABEL.Text = "";
            }
            TotalMPLabel.Text = TeamsCollectiveMP.ToString();
            if (TeamsCollectiveMP <= 0)
            {
                TotalMPLabel.Modulate = new Color(255,0,0);
            }
            else
            {
                TotalMPLabel.Modulate = new Color(255,255,255);
            }
            if (Input.IsActionJustPressed("MYSPACE"))
            {
                AccessNextTurnPopup = true;
            }
            if(Input.IsActionJustPressed("MYESC"))
            {
                if (ESCMenu.Visible == false)
                {
                    EscapeThisShithole(true);
                }
                else
                {
                    EscapeThisShithole(false);
                }
            }
            if (AccessNextTurnPopup == true && PopUpRefScript != null && ChosenActionFinished == true)
            {
                if (TeamTurnTable.Count <= 1)
                {
                    PopUpRefScript.Call("PopUpContentsFunc", $"Do you want to end round {Round} ?", true);
                }
                else
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?", false);
                }
                AccessNextTurnPopup = false;
            }
        }
    }
    void UltimatePawnSwitchingFunc(bool TrueisNext,bool TrueisMPActive)// tym bardziej pierdole, liczba razy ile mi się nie udało tego kodu naprawić .: 3
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Nie można zaznaczyć pionka bo gracz nie zfinalizował akcji");
            return;
        }
        CalculateActiveTeamPawns(false);
        if (TrueisNext)
        {
            //GD.Print("Kliknięty został przycisk (Następny pionek)");
        }
        else
        {
            //GD.Print("Kliknięty został przycisk (Poprzedni pionek)");
        }
        if (ActiveTeamPawns.Count == 0)
        {
            GD.Print("Nie ma pionka do którego możnaby przejść");
            return;
        }

        if (IntForUnitSelection >= ActiveTeamPawns.Count)
        {
            IntForUnitSelection = 0;
        }
        else if (IntForUnitSelection <= 0)
        {
            IntForUnitSelection = (IntForUnitSelection + ActiveTeamPawns.Count) % ActiveTeamPawns.Count; 
        }
        
        // Znajdź najbliższą nie-null jednostkę
        int startIndex = IntForUnitSelection;
        do
        {
            var NextPawn = ActiveTeamPawns[IntForUnitSelection];
            if (NextPawn != null && NextPawn.IsInsideTree())
            {
                SelectPawn(NextPawn);
                //GD.Print($"int indeksowy to {IntForUnitSelection} dla {NextPawn.Name}"); // naprawięto jednego dnia 
                if (TrueisNext) // jeśli następny pionek 
                {
                    IntForUnitSelection = (IntForUnitSelection + 1) % ActiveTeamPawns.Count;
                }
                else // jeśli poprzedni pionek 
                {
                    IntForUnitSelection = (IntForUnitSelection - 1) % ActiveTeamPawns.Count;
                }
                return;
            }
        }
        while (IntForUnitSelection != startIndex);    
    }
    public void GenerateActionLog(string Message)
    {
        Label Log = new Label();
        Log.Text = Message;
        Log.AddThemeFontSizeOverride("font_size", 140);
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
    void NextRoundFunc() // Runda (To dłuższe)
    {
        GD.Print("New round!");
        RecalculationTeamStatus();
        foreach (var log in LogBucket.GetChildren())
        {
            log.QueueFree();
        }
        Round++;
        // przetasuj kolejność wg pionków
        if (FirstRoundDone == false)
        {
            ActiveTeams.Sort((a, b) => b.PawnCount.CompareTo(a.PawnCount));
            FirstRoundDone = true;
        }
        TeamTurnTable.Clear();
        foreach (var team in ActiveTeams)
        {
            TeamTurnTable.Add(team.name);
        }

        Turn = TeamTurnTable[0];
        CalculateAllTeamMP();
        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                //GD.Print("reset MP dokonany");
                pawn.Call("ResetMP");
                if (pawn.TeamId == Turn)
                {
                    pawn.Call("ResetMoveStatus");
                }
            }
        }
        UltimatePawnSwitchingFunc(true, false);
        GenerateActionLog($"## Round {Round} ##");
    }
    void RecalculationTeamStatus() // podlicza żywe drużyny, wyznacza wygraną
    {
        foreach (TeamConfig ActiveTeam in ActiveTeams)
        {
            ActiveTeam.PawnCount = 0;
            ActiveTeam.CollectiveMPCount = 0;
        }
        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                foreach (TeamConfig ActiveTeam in ActiveTeams)
                {
                    if (ActiveTeam.name == pawn.TeamId)
                    {
                        ActiveTeam.PawnCount++;
                        ActiveTeam.CollectiveMPCount += 2;
                    }
                }
            }
        }
        ActiveTeams.RemoveAll(t => t.PawnCount <= 0); //nieaktywna drużyna generalnie 
        TeamTurnTable.RemoveAll(name => !ActiveTeams.Exists(t => t.name == name)); //nieaktywna drużyna tej rundy 
        // jeśli obecna tura należy do martwej drużyny — przeskocz
        if (!ActiveTeams.Exists(t => t.name == Turn))
        {
            if (TeamTurnTable.Count > 0)
            {
                Turn = TeamTurnTable[0];
            }else{
                GD.Print("Brak drużyn do gry");
            }
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
        TeamsCollectiveMP = 0;
        RecalculationTeamStatus();
        CalculateActiveTeamPawns(true);
        GD.Print($"koniec rundy dla drużyny {TeamTurnTable[0]}");
        TeamTurnTable.RemoveAt(0);
        while (TeamTurnTable.Count > 0 && !ActiveTeams.Exists(t => t.name == TeamTurnTable[0]))
        {
            GD.Print($"drużyna {TeamTurnTable[0]} usunięta z racji na brak pionków");
            TeamTurnTable.RemoveAt(0);
        }
        if (TeamTurnTable.Count == 0)
        {
            GD.Print("No teams left to take turns!");
            return;
        }
        // nowa drużyna
        Turn = TeamTurnTable[0];
        CalculateAllTeamMP();
        UltimatePawnSwitchingFunc(true, false);
        foreach (Node child in UnitsBucket.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                if (pawn.TeamId == Turn)
                {
                    pawn.Call("ResetMoveStatus");
                }
            }
        }
        GenerateActionLog($"## Team {Turn} starts thier Turn ##");
    }
    void CalculateAllTeamMP()
    {
        TeamConfig TeamToAccount = ActiveTeams.Find(a => a.name.Contains(Turn)); // zakładając że drużyna jeszcze jest ale to powinno się wyprostować uprzednio 
        TeamsCollectiveMP = TeamToAccount.CollectiveMPCount;
        //GD.Print($"Drużyna {TeamToAccount.name} powinna mieć przypisane {TeamToAccount.CollectiveMPCount} MP");
    }
    public void SetResolution(int option)
    {
        Vector2I resolution = Vector2I.Zero;
        switch (option)
        {
            case 0: resolution = new Vector2I(854, 480); break;   // 480p
            case 1: resolution = new Vector2I(1280, 720); break;  // 720p
            case 2: resolution = new Vector2I(1600, 900); break;  // 900p
            case 3: resolution = new Vector2I(1920, 1080); break; // 1080p
            case 4: resolution = new Vector2I(2560, 1440); break; // 1440p
            case 5: resolution = new Vector2I(3840, 2160); break; // 4K
            default: resolution = new Vector2I(1280, 720); break;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(resolution);

        GD.Print($"Ustawiono rozdzielczość na: {resolution}");
    }
}
