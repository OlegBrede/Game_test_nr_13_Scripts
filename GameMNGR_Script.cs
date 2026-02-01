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
    bool CameraShowingAction = false;
    bool TrueisShowAction = true;
    private Tween.TransitionType transition = Tween.TransitionType.Sine;
    private Tween.EaseType ease = Tween.EaseType.InOut;
    private Tween activeTween;
    Vector2 ActionView;
    Vector2 ReactionView;
    // ####################### KAMERA #######################
    // ####################### SOUNDS #######################
    [Export] public SoundControlScript SCS;
    [Export] UNI_AudioStreamPlayer2d UASP;
    // ####################### SOUNDS #######################
    // ####################### AI #######################
    public Node2D AIPlayersBucket;
    // ####################### AI #######################
    [Export] Label UnitInfoGuiLabel;
    [Export] Label TotalMPLabel;
    [Export] Label ULTIMATENAMELABEL;
    [Export] Label SNTWN; // Show no target warning node
    [Export] VBoxContainer LogBucket;
    [Export] ScrollContainer KontenrLogów;
    [Export] public GUIButtonsToPawnScript PlayerGUIRef;
    [Export] public Node2D bucketForTheDead;
    [Export] Node2D PickupsBucket;
    string SceneToLoad = "res://Scenes/MultiGameOverScreen.tscn";
    public int Round = 0; // zmienić by kod wchodząc do sceny zaczynał next round i zobaczyć gdzie to nas zaniesie 
    public string Turn = "";
    public bool ChosenActionFinished = true;
    public int TeamsCollectiveMP = 0;
    public static GameMNGR_Script Instance { get; private set; }
    public PawnBaseFuncsScript SelectedPawn { get; private set; }
    private string SaveFilePath => ProjectSettings.GlobalizePath("user://teams.json");
    public string Teamcolor = "";
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
    public class CaptureActionInfo
    {
        public Vector2 C_Giver { get; set; }
        public Vector2 C_Recypiant { get; set; }
        public bool C_TrueisWideShotNeeded { get; set; }
    }
    //Stack<CaptureActionInfo> captureActionInfoQueue = new Stack<CaptureActionInfo>();
    public List<string> TeamTurnTable = new List<string>(); // lista do tur
    public List<TeamConfig> ActiveTeams = new List<TeamConfig>(); // aktywne w danej grze
    List<PawnBaseFuncsScript>[] ActiveTeamPawns = {new List<PawnBaseFuncsScript>(),new List<PawnBaseFuncsScript>()};
    private PawnBaseFuncsScript PrevSelectedPawn;
    Node2D PopUpRef;
    Node2D ScrollPopUpRef;
    public Node2D PawnBucketRef;
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
        CamShowActionTimer.WaitTime = waitDuration;
        UASP.SCS = SCS;
        //UASP.PlaySound(0,false);
        //PlayerGUIRef = GetTree().Root.GetNode<GUIButtonsToPawnScript>("BaseTestScene/Camera2D/GUI_to_Pawn_Input_Translator");
        CamShowActionTimer.Timeout += ShowReactionAfterTimeout; // powinno być tu CameraActionTimerSwitch ale ten na to pomysł jest chwilowo zaryglowany albo na sequel albo na rewrite czy coś w ten deseń 
        SNTWN.Visible = false;
        ESCMenu.Visible = false;
        UnitInfoGuiLabel.Text = "";
        AIPlayersBucket = GetNode<Node2D>("AIPlayersBucket");
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
        if (IsInstanceValid(pawn) == false || pawn == null)
        {
            GD.Print("Nie można zaznaczyć pionka bo nie istnieje");
            return;
        }
        if (ChosenActionFinished == true)
        {
            if (SelectedPawn != null) // trzeba wysłać reset do skryptu gracza bo inaczej zaznaczenie się zduplikuje 
            {
                SelectedPawn.Call("ShowSelection", false); // animacja
                SelectedPawn.Call("RSSP"); // reset selekcji, teoretycznie niepotrzebny ale... TO DO .: - sprawdź czy usunięcie resetu zepsuje grę 
            }
            SelectedPawn = pawn; // możesz też emitować sygnał tutaj jeśli kto inny chce reagować
            SelectedPawn.OnSellectSay();
            SelectedPawn.SetUISubscription(); // Subskrypcja do UI
            PrevSelectedPawn = SelectedPawn;
            PlayerGUIRef.PALO(false,true); // pokaż akcje które może podjąć pionek na GUI
            PlayerGUIRef.SelectionUpdater();
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
    // ########################################### KAMERA ###########################################
    /*
    public void CaptureAction(PawnBaseFuncsScript GiverTrigger,Vector2 Giver, Vector2 Recypiant,bool TrueisWideShotNeeded) // tu ma być pobrana akcja która następnie czeka w kolejce na swą kolej odegrania
    {
        if (TrueisWideShotNeeded == true) // nie jest chyba koniecznym by tam w CaptureActionInfo był TrueisWideShotNeeded
        {
            FocusCam.GlobalPosition = Recypiant;
            return;
        }
        if (CameraShowingAction == true)
        {
            GD.Print($"Akcja musi czekać w kolejce obecny czas czekania {captureActionInfoQueue.Count}");
            captureActionInfoQueue.Push(new CaptureActionInfo{C_Giver = Giver,C_Recypiant = Recypiant,C_TrueisWideShotNeeded = TrueisWideShotNeeded});
        }
        else
        {
            CameraShowingAction = true;
            GD.Print("Capture action wywołało ukazywanie akcji");
            captureActionInfoQueue.Push(new CaptureActionInfo{C_Giver = Giver,C_Recypiant = Recypiant,C_TrueisWideShotNeeded = TrueisWideShotNeeded});
            //CamShowActionTimer.WaitTime = 0.1f;
            FocusCam.GlobalPosition = Giver;
            LockNloadCamAction(captureActionInfoQueue.Peek());
        }
    }
    void LockNloadCamAction(CaptureActionInfo CAIQ)
    {
        CamShowActionTimer.Start();
        ChosenActionFinished = false;
        ActionView = CAIQ.C_Giver;
        ReactionView = CAIQ.C_Recypiant;
        //FocusCam.GlobalPosition = CAIQ.C_Recypiant; // to miało być na bool C_TrueisWideShotNeeded ale to za chwilę ogarnę 
    }
    void CameraActionTimerSwitch()
    {
        if (captureActionInfoQueue.Count <= 0)
        {
            CameraShowingAction = false;
            CamShowActionTimer.Stop();
            ChosenActionFinished = true;
            GD.Print("Koniec pokazywania akcji");
            return;
        }
        if (TrueisShowAction == true)
        {
            ShowActionAfterTimeout();
            return; // to powinno dezaktywować ten if na samym dole by nie zmieniał kolejności rzeczy 
        }
        else
        {
            ShowReactionAfterTimeout();
        }
        if (captureActionInfoQueue.Count > 0 && TrueisShowAction == true) // że ten tu zatrzymać <---
        {
            GD.Print("kolejna akcja wykonuje się - stara idzie, nowa wchodzi");
            captureActionInfoQueue.Pop();// zabicie akcji zaraz po tym jak zostaje ona zaprezentowana
            if (captureActionInfoQueue.Count > 0) // ta idiotycznie podwójnie sprawdzać ale... 
            {
                LockNloadCamAction(captureActionInfoQueue.Peek());
            }
        }
    }
    void ShowActionAfterTimeout() // tu pokazana jest akcja 
    {
        //CamShowActionTimer.WaitTime = waitDuration;
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ActionView,0.05f).SetTrans(transition).SetEase(ease);
        TrueisShowAction = false;
        GD.Print("Rzut kamerą na akcję");
    }
    void ShowReactionAfterTimeout() // tu pokazana jest reakcja 
    {
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ReactionView,0.05f).SetTrans(transition).SetEase(ease);
        TrueisShowAction = true;
        GD.Print("Rzut kamerą na reakcję");
    }
    */
    public void CaptureAction(Vector2 Giver, Vector2 Recypiant,bool TrueisWideShotNeeded) // tu ma być pobrana akcja która następnie czeka w kolejce na swą kolej odegrania
    {
        //captureActionInfoQueue.Add(new CaptureActionInfo{C_Giver = Giver,C_Recypiant = Recypiant,C_TrueisWideShotNeeded = TrueisWideShotNeeded});
        // to na dole, musi być dane do LockNloadCamAction by wszystko działało sprawnie
        ActionView = Giver;
        ReactionView = Recypiant;
        if (TrueisWideShotNeeded == true)
        {
            CamShowActionTimer.WaitTime = waitDuration;
            ShowActionAfterTimeout();
        }
        else
        {
            FocusCam.GlobalPosition = Recypiant;
        }
    }
    void ShowActionAfterTimeout() // tu pokazana jest akcja 
    {
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ActionView,0.05f).SetTrans(transition).SetEase(ease);
        CamShowActionTimer.Start();
        //GD.Print("Timer włączony");
    }
    void ShowReactionAfterTimeout() // tu pokazana jest reakcja 
    {
        activeTween?.Kill();
        activeTween = GetTree().CreateTween();
        activeTween.TweenProperty(FocusCam,"global_position",ReactionView,0.05f).SetTrans(transition).SetEase(ease);
        CamShowActionTimer.Stop();
        //GD.Print("Timer skończcył");
    }
    // ########################################### KAMERA ###########################################
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
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
        AccessNextTurnPopup = true;
    }
    void Button_ACT2() // next unit
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
        UltimatePawnSwitchingFunc(true, false);
    }
    void Button_ACT3() // prev unit 
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
        UltimatePawnSwitchingFunc(false, false);
    }
    void Button_ACT4() //current active
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
        if (PrevSelectedPawn != null)
        {
            SelectPawn(PrevSelectedPawn);
        }
    }
    void Button_ACT5() // prev active
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
        UltimatePawnSwitchingFunc(true, true);
    }
    void Button_ACT6() // next active
    {
        if (ChosenActionFinished == false)
        {
            GD.Print("Gracz nie zakończył akcji");
            return;
        }
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
                ULTIMATENAMELABEL.Text = "Selected\n" +SelectedPawn.UnitName;
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
                    PopUpRefScript.Call("PopUpContentsFunc", $"Do you want to end round {Round} ?");
                }
                else
                {
                    PopUpRefScript.Call("PopUpContentsFunc", "Do you want to end your turn ?");
                }
                AccessNextTurnPopup = false;
            }
        }
    }
    void UltimatePawnSwitchingFunc(bool TrueisNext,bool TrueisMPActive) // Next/prev    active/inactive
    {
        //bool MPHavers = true;
        CalculateActiveTeamPawns(false); // kalkulacja pionków do list, tej z aktywnymi pionkami i tej z ieaktywnymi pionkami 
        if (ActiveTeamPawns[0].Count == 0) //guziki nie mogą operować gdy ne ma pionków na których można opreować
        {
            GD.Print("Nie ma pionków do kalkulacji");
            return;
        }
        if (ChosenActionFinished == false)
        {
            GD.Print("Nie można zaznaczyć pionka bo gracz nie zfinalizował akcji");
            return;
        }
        if (SelectedPawn != null) // Deselekcja tego pionka co teraz jest zaselekcjonowany
        {
            //DeselectPawn();
        }
        int ActiveOrnot; // z której listy ma czerpać index info o tym gdzie przejść teraz 
        if (TrueisMPActive == true) // wybrano wybieranie po tych co mają MP 
        {
            if (ActiveTeamPawns[1].Count == 0) // tu także musi być przynajmniej jeden pionek 
            {
                GD.Print("Nie ma aktywnych pionków do kalkulacji");
                return;
            }
            else
            {
                ActiveOrnot = 1;
            }
        }
        else
        {
            ActiveOrnot = 0;
        }
        if (TrueisNext == true) // Następny pionek
        {   
            //GD.Print("Następn pionek");
            //GD.Print($"ActiveTeamPawns.Count jest {ActiveTeamPawns[ActiveOrnot].Count}, IndexNum to {IntForUnitSelection + 1} więc {ActiveTeamPawns[ActiveOrnot].Count >= IntForUnitSelection + 2}");
            if (ActiveTeamPawns[ActiveOrnot].Count >= IntForUnitSelection + 2 == true)
            {
                IntForUnitSelection++;
            }
            else
            {
                //GD.Print("Reset do zera");
                IntForUnitSelection = 0;
            }
            SelectPawn(ActiveTeamPawns[ActiveOrnot][IntForUnitSelection]);
        }
        else  // Poprzedni pionek
        {
            //GD.Print("Poprzedni pionek");
            //GD.Print($"ActiveTeamPawns.Count jest {ActiveTeamPawns[ActiveOrnot].Count}, IndexNum to {IntForUnitSelection - 1} więc {ActiveTeamPawns[ActiveOrnot].Count <= IntForUnitSelection - 1}");
            if (0 <= IntForUnitSelection - 1 == true)
            {
                IntForUnitSelection--;
            }
            else
            {
                //GD.Print("reset na początek");
                IntForUnitSelection = ActiveTeamPawns[ActiveOrnot].Count - 1;
            }
            SelectPawn(ActiveTeamPawns[ActiveOrnot][IntForUnitSelection]);
        }
        //GD.Print($"IndexNum nonactive is {IntForUnitSelection} IndexNum active is {IntForUnitSelection}");
    }
    public void GenerateActionLog(string Message)
    {
        RichTextLabel Log = new RichTextLabel();
        Log.BbcodeEnabled = true;
        Log.FitContent = true;
        Log.ClipContents = false;
        Log.AutowrapMode = TextServer.AutowrapMode.Off;
        Log.Text = Message;
        Log.AddThemeFontSizeOverride("normal_font_size", 150);
        LogBucket.AddChild(Log);
        KontenrLogów.ScrollVertical = (int)KontenrLogów.GetVScrollBar().MaxValue;      
    }
    void CalculateActiveTeamPawns(bool FirstTime)
    {
        if (FirstTime == true)
        {
            IntForUnitSelection = 0;
        }        
        ActiveTeamPawns[0].Clear();
        ActiveTeamPawns[1].Clear();
        foreach (PawnBaseFuncsScript TeamPawn in PawnBucketRef.GetChildren())
        {
            if (TeamPawn.TeamId == Turn)
            {
                //GD.Print($"do listy dodano {TeamPawn.Name}");
                ActiveTeamPawns[0].Add(TeamPawn);
                if (TeamPawn.MP > 0)
                {
                    ActiveTeamPawns[1].Add(TeamPawn);
                }
            }
        }
        //GD.Print("Dodano pionki do listy odczytu dla teamu");
    }
    void NextRoundFunc() // Runda (To dłuższe)
    {
        GD.Print("###################### New round! ######################");
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
        TeamConfig TeamToAccount = ActiveTeams.Find(a => a.name.Contains(Turn));
        Teamcolor = TeamToAccount.team_colour.ToHtml();
        CalculateAllTeamMP();
        
        UltimatePawnSwitchingFunc(true, false);
        GenerateActionLog($"## Round {Round} ##");
        GenerateActionLog($"##[color={Teamcolor}] Team {Turn} [/color]starts thier Turn ##");
        foreach (Node child in PawnBucketRef.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                //GD.Print("reset MP dokonany");
                pawn.ApllyStatusEffects();
                pawn.ResetMP();
                if (pawn.PawnMoveStatus == PawnMoveState.Fainted)
                {
                    GD.Print("Fainted set to recovery");
                    pawn.FaintRecoveryBool = true;
                }
                if (pawn.TeamId == Turn)
                {
                    pawn.Call("ResetMoveStatus");
                    if (pawn.EntryWounds > 0)
                    {
                        GD.Print("Krwawienie aktywowane dla tego pionka");
                        pawn.FuncBleed();
                    }
                }
            }
        }
        ActivateAICommanders();
    }
    void RecalculationTeamStatus() // podlicza żywe drużyny, wyznacza wygraną, jeśli drużyna jest kontrolowana przez ai to mówi tej drużynie że może zaczynac kalkulowanie 
    {
        foreach (TeamConfig ActiveTeam in ActiveTeams)
        {
            ActiveTeam.PawnCount = 0;
            ActiveTeam.CollectiveMPCount = 0;
        }
        foreach (Node child in PawnBucketRef.GetChildren())
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                foreach (TeamConfig ActiveTeam in ActiveTeams)
                {
                    if (ActiveTeam.name == pawn.TeamId)
                    {
                        ActiveTeam.PawnCount++;
                        if (pawn.PawnMoveStatus != PawnMoveState.Fainted)
                        {
                            ActiveTeam.CollectiveMPCount += 2;
                        }
                        if (pawn.OVStatus == true)
                        {
                            //pawn.OverwatchNodeBucket.Visible = true; // dla tych których jest teraz tura
                        }
                    }
                    else
                    {
                        GD.Print($"Reset Widoczności Overwatch dla {pawn.UnitName}");
                        //pawn.OverwatchNodeBucket.Visible = false; // dla tych dla których nie ma teraz tury 
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
    void ActivateAICommanders()
    {
        foreach (var AI_Team in ActiveTeams)
        {
            if (AI_Team.AI_Active == true)
            {
                foreach(AI_StategyBotScript AI_Commander in AIPlayersBucket.GetChildren())
                {
                    if (AI_Commander.MyteamID == Turn)
                    {
                        AI_Commander.Call("Activate");
                    }
                }
            }
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
        GD.Print($"###################### koniec rundy dla drużyny {TeamTurnTable[0]} ######################");
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
        TeamConfig TeamToAccount = ActiveTeams.Find(a => a.name.Contains(Turn));
        Teamcolor = TeamToAccount.team_colour.ToHtml();
        CalculateAllTeamMP();
        UltimatePawnSwitchingFunc(true, false);
        GenerateActionLog($"##[color={Teamcolor}] Team {Turn} [/color]starts thier Turn ##");
        foreach (Node child in PawnBucketRef.GetChildren()) // TO DO .: - dać to do funkcji bo się powtarza, ale w tym drugim jest reset MP
        {
            if (child is PawnBaseFuncsScript pawn)
            {
                if (pawn.TeamId == Turn)
                {
                    pawn.Call("ResetMoveStatus");
                    if (pawn.EntryWounds > 0)
                    {
                        GD.Print("Krwawienie aktywowane dla tego pionka");
                        pawn.FuncBleed();
                    }
                }
            }
        }
        ActivateAICommanders();
    }
    public void RefreshTableSituationStatus(CharacterBody2D MoveReportee) // tak, wiem, robię to chujowo
    {
        foreach (PawnBaseFuncsScript Pawn in PawnBucketRef.GetChildren())
        {
            if (Pawn.OVStatus == true && Pawn.TeamId != Turn)
            {
                GD.Print($"jest pionek który ma overwatch, jest tura {Turn}, pionek jest drużyny {Pawn.TeamId}, sprawdzane jest Overwatch");
                Pawn.CheckOV_LOS(MoveReportee);
            }
        }
        foreach (UNI_pickupscript Pickup in PickupsBucket.GetChildren())
        {
            GD.Print("Dokonano sprawdzenia pickupu");
            Pickup.StartTimer();
        }
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
