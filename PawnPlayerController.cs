using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class PawnPlayerController : Node2D
{
    [Export] public PawnBaseFuncsScript PawnScript;
    [Export] private UNI_ControlOverPawnScript UCOPS;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D MeleeSlcieNode; // to też powinno iść do uni 
    [Export] public Node2D PointerNode;
    [Export] public NavigationAgent2D NavAgent;
    [Export] Sprite2D movementsprite;
    [Export] UNI_LOSRayCalcScript ShootingRayScript;
    [Export] Area2D MeleeAttackArea;
    [Export] Node2D ONB; //overwatch node bucket
    [Export] Node2D UNI_markerRef;
    NodeCompButtonUni1FuncScript YayButton;
    NodeCompButtonUni1FuncScript NayButton;
    Sprite2D UNI_MarkerIcon;
    Node2D MovementAllowenceInyk_ator;
    Node2D OverwatchpointRefNode;
    Node2D OVButtons;
    Node2D OVPoint1Ref;
    Node2D OVPoint2Ref;
    Polygon2D OVDebugPoly;
    GameMNGR_Script gameMNGR_Script;
    Area2D OverwatchArea;
    Area2D WideMelee;
    Area2D StrongMelee;
    CollisionPolygon2D OverwatchTriangleHitbox;
    Label ChanceToHitLabel1;
    //################################# CALLABLES ######################################
    private Callable callableMove;
    private Callable callableShoot;
    private Callable callableAimed;
    private Callable callableOverwatch;
    private Callable callableWideWallop;
    private Callable callableStrongWallop;
    private Callable callableConfirm;
    private Callable callableDecline;
    //################################# CALLABLES ######################################
    enum PlayersChosenAction
    {
        None, MoveAction, RangeAttackAction, AimedRangeAttackAction, OverwatchAction, MeleeAttackAction, StrongMeleeAttackAction
    }
    private PlayersChosenAction ChosenAction = PlayersChosenAction.None;
    private bool isSelected = false;
    private Vector2 texSize;
    private float baseRadius;
    private float ShootingFinalDiceVal = 0;
    private int ShootingTargetLockIndex = 999; // index części ciała która ma być trafona 
    private float PartProbability = 0;
    private float MeleeFinalDiceVal = 0;
    bool Actionconfimrm = false;
    bool DEBUG_UIConnectionStatus = false;
    public override void _Ready()
    {
        UNI_markerRef.Visible = false;
        YayButton = UNI_markerRef.GetNode<NodeCompButtonUni1FuncScript>("SampleButton3");
        NayButton = UNI_markerRef.GetNode<NodeCompButtonUni1FuncScript>("SampleButton4");
        UNI_MarkerIcon = UNI_markerRef.GetNode<Sprite2D>("Uni_Icon");

        callableMove = new Callable(this, nameof(Player_ACT_Move));
        callableShoot = new Callable(this, nameof(Player_ACT_Shoot));
        callableAimed = new Callable(this, nameof(Player_ACT_AimedShot));
        callableOverwatch = new Callable(this, nameof(Player_ACT_Overwatch));
        callableWideWallop = new Callable(this, nameof(Player_ACT_Punch));
        callableStrongWallop = new Callable(this, nameof(Player_ACT_StrongPunch));
        callableConfirm = new Callable(this, nameof(Player_ACT_Confirm));
        callableDecline = new Callable(this, nameof(Player_ACT_Decline));

        ChanceToHitLabel1 = UNI_markerRef.GetNode<Label>("Label");
        ChanceToHitLabel1.Visible = false;
        //GD.Print("PAMIĘTAJ by zawsze sprawdzić na którym pionku testujesz swe dodatki");
        WideMelee = MeleeSlcieNode.GetNode<Area2D>("Area2DMeleeWideAttackRange");
        StrongMelee = MeleeSlcieNode.GetNode<Area2D>("Area2DMeleeStrongAttackRange");
        WideMelee.Visible = false;
        StrongMelee.Visible = false;

        PointerNode.Visible = false;
        MovementAllowenceInyk_ator = GetNode<Node2D>("MoveIndicator");
        MovementAllowenceInyk_ator.Visible = false;
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");

        var statArea = PawnScript.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        StatsUI.Visible = false;

        var circle = PawnScript.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
        NavAgent.Radius = circle.Radius;

        ONB.Visible = false;
        OVDebugPoly = ONB.GetNode<Polygon2D>("Polygon2D");
        OverwatchArea = ONB.GetNode<Area2D>("Area2D");
        //OverwatchArea.BodyEntered += OverwachedAreaEntered;
        OverwatchTriangleHitbox = OverwatchArea.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        OverwatchpointRefNode = ONB.GetNode<Node2D>("NodeForAreaTriangulation");
        OVPoint1Ref = OverwatchpointRefNode.GetNode<Node2D>("Point1");
        OVPoint2Ref = OverwatchpointRefNode.GetNode<Node2D>("Point2");
    }
    public override void _Process(double delta)
    {
        // ################# INFO O PIONKU ######################
        if (StatsUI.Visible) // to może tak być, pierdole 
        {
            string ammoinfo;
            if (PawnScript.ShootingAllowence > 0)
            {
                ammoinfo = $"Ammo ({PawnScript.WeaponAmmo}/{PawnScript.WeaponMaxAmmo})";
            }
            else
            {
                ammoinfo = " ";
            }
            StatsLabel.Text = $"{PawnScript.UnitName}\n{PawnScript.TeamId}\nHP {Mathf.RoundToInt((float)PawnScript.Integrity / (float)PawnScript.BaseIntegrity * 100f)}%\nMP {PawnScript.MP}\nStatus {PawnScript.PawnMoveStatus}\n{ammoinfo}";
        }
        // ################# INFO O PIONKU ######################
        // ################# AKCJE PONIŻEJ I ICH BLOK #########################
        if (ChosenAction == PlayersChosenAction.MoveAction)
        {
            bool CanGoThere;
            if (UNI_markerRef.Visible == false)
            { // jeśli nie ma postawionego punktu ruchu śledź kursor
                MovementAllowenceInyk_ator.GlobalPosition = GetGlobalMousePosition();
                NavAgent.TargetPosition = MovementAllowenceInyk_ator.GlobalPosition;
                NavAgent.GetNextPathPosition();
            }
            if (UCOPS.MovementAllowenceCalculationResult(MovementAllowenceInyk_ator.GlobalPosition) == true) // pokazujemy graczu że może tam stanąć 
            {
                movementsprite.Modulate = new Color(0, 1, 0);
                CanGoThere = true;
            }
            else
            {
                movementsprite.Modulate = new Color(1, 0, 0);
                CanGoThere = false;
            }
            UCOPS.DistanceMovedUpdate(); // podanie ile się rusznął ruch dystansowy resetuje się przy poruszeniu się dwa razy ale to celowo, tu po prostu zaufam instynktowi który mówi mi że tak jest poprawnie
            //GD.Print($"Pionek rusza się o {PawnScript.DistanceMovedByThisPawn}");
            if (Input.IsActionJustPressed("MYMOUSELEFT") && CanGoThere == true && UNI_markerRef.Visible == false) // można wybrać marker tylko wtedy gdy jego pozycja jest potwierdzona przez dystans 
            {
                UNI_markerRef.GlobalPosition = GetGlobalMousePosition();
                UNI_markerRef.Visible = true;
                gameMNGR_Script.PlayerGUIRef.PALO(true, false);
            }
        }
        if (ChosenAction == PlayersChosenAction.RangeAttackAction || ChosenAction == PlayersChosenAction.AimedRangeAttackAction)
        {
            ShootFunc(false); // musiało zostać przesunięte zaśmiecało tu funkcje 
        }
        if (ChosenAction == PlayersChosenAction.MeleeAttackAction || ChosenAction == PlayersChosenAction.StrongMeleeAttackAction) // tu podobnie jak dla akcji aimed range tu też powinna być opcja na jakiś mocny atak 
        {
            if (ChosenAction == PlayersChosenAction.MeleeAttackAction)
            {
                WideMelee.Visible = true;
                StrongMelee.Visible = false;
            }
            else
            {
                WideMelee.Visible = false;
                StrongMelee.Visible = true;
            }
            if (UNI_markerRef.Visible == false)
            {
                MeleeSlcieNode.LookAt(GetGlobalMousePosition());
            }
            else
            {
                MeleeSlcieNode.LookAt(UNI_markerRef.GlobalPosition);
            }
            if (Input.IsActionJustPressed("MYMOUSELEFT") && UNI_markerRef.Visible == false)
            {
                UNI_markerRef.Visible = true;
                UNI_markerRef.GlobalPosition = GetGlobalMousePosition();
                gameMNGR_Script.PlayerGUIRef.PALO(true, false);
            }
        }
        if (ChosenAction == PlayersChosenAction.OverwatchAction)
        {
            // proponuję zrobić to tak jak w quar-ach area fire, to wtedy nie będzie trzeba się certolić z kierunkiem wzroku
            if (UNI_markerRef.Visible == false)
            {
                UNI_markerRef.GlobalPosition = GetGlobalMousePosition();
                OverwatchpointRefNode.LookAt(PawnScript.GlobalPosition);
            }
            if (Input.IsActionJustPressed("MYMOUSELEFT") && UNI_markerRef.Visible == false)
            {
                UNI_markerRef.Visible = true;
                var points = OverwatchTriangleHitbox.Polygon;
                points[0] = OverwatchTriangleHitbox.ToLocal(OVPoint1Ref.GlobalPosition);
                points[1] = OverwatchTriangleHitbox.ToLocal(PawnScript.GlobalPosition);
                points[2] = OverwatchTriangleHitbox.ToLocal(OVPoint2Ref.GlobalPosition);
                //GD.Print($"Pozycja zmieniona dla punktów .: {points[0]},{points[1]},{points[2]}, Pozycja pionka to {PawnScript.GlobalPosition}");
                OverwatchTriangleHitbox.Polygon = points;
                OVDebugPoly.Polygon = points;
                gameMNGR_Script.PlayerGUIRef.PALO(true, false);
            }
        }
    }
    public void SubscribeToUIControlls() // tu pionek subskrybuje UI by in tandem mógł działać bez potrzebu "telefonów" przez gameMNGR
    {
        if(gameMNGR_Script.SelectedPawn != null)
        {
            GD.Print($"Subscription triggered to {gameMNGR_Script.SelectedPawn.UnitName} ID .:{gameMNGR_Script.SelectedPawn.Name}...");
        }
        UnsubscribeFromUIControlls();
        if(gameMNGR_Script.PlayerGUIRef.IsConnected(GUIButtonsToPawnScript.SignalName.MoveAction , callableMove) == false) // by zasubskrybować pawn, musi obecny nie być null oraz musi on nie być obecnym nowym pionkiem 
        {
            DEBUG_UIConnectionStatus = true;
            GD.Print($"conectting subscription to {gameMNGR_Script.SelectedPawn.UnitName} ID .:{gameMNGR_Script.SelectedPawn.Name}... ");
            //GUI_BTPS.Connect(GUIButtonsToPawnScript.SignalName.NAZWA, new Callable(this, nameof(SYGNAŁ)));
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.MoveAction , callableMove);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.NormalShotAction , callableShoot);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.AimedShotAction , callableAimed);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.OverwatchAction , callableOverwatch);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.WideWallopAction , callableWideWallop);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.StrongWallopAction , callableStrongWallop);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.PawnConfirm , callableConfirm);
            gameMNGR_Script.PlayerGUIRef.Connect(GUIButtonsToPawnScript.SignalName.PawnDecline, callableDecline);
            //Current_GUI_BTPS.Call("bullshit"); // przykładowy trigger z powrotem do nadawcy
        }
        else
        {
            GD.Print("dany pionek ma już subskrybcję do gui");
        }
    }
    public void UnsubscribeFromUIControlls()
    {
        if (gameMNGR_Script.PlayerGUIRef.IsConnected(GUIButtonsToPawnScript.SignalName.MoveAction , callableMove) == true)
        {
            GD.Print($"disconectting subscription from prev pawn ... ");
            DEBUG_UIConnectionStatus = false;
            //GUI_BTPS.Disconnect(GUIButtonsToPawnScript.SignalName.NAZWA SYGNAŁU TU , new Callable(this, nameof(NAZWA FUNKCJI)));
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.MoveAction , callableMove);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.NormalShotAction , callableShoot);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.AimedShotAction , callableAimed);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.OverwatchAction , callableOverwatch);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.WideWallopAction , callableWideWallop);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.StrongWallopAction , callableStrongWallop);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.PawnConfirm , callableConfirm);
            gameMNGR_Script.PlayerGUIRef.Disconnect(GUIButtonsToPawnScript.SignalName.PawnDecline, callableDecline);
        }
        else
        {
            GD.Print($"odsubskrybowanie niemożliwe dla już odsubskrybowanego elementu");
        }
    }
    void ShootFunc(bool OV_Active)
    {
        // ################# KALKULOWANIE W UCOPS ######################
        bool AimedOrnot;
        if (ChosenAction == PlayersChosenAction.AimedRangeAttackAction)
        {
            AimedOrnot = true;
        }
        else
        {
            AimedOrnot = false;
        }
        ShootingFinalDiceVal = UCOPS.RangeAttackEffectivenessCalculation(ShootingRayScript.Raylengh,ShootingRayScript.RayHittenTarget,AimedOrnot);
        // ############### PODLICZENIE WYŚWIETLONEGO PROCENTU ##################
        ChanceToHitLabel1.Visible = true;
        ChanceToHitLabel1.Text = $"{UCOPS.PrecentCalculationFunction(ShootingFinalDiceVal).ToString()}%";
        // ############### WIZUALNA REPREZENTACJA LOS DLA PIONKA  ##################
        if ((UNI_markerRef.Visible == false && ChosenAction == PlayersChosenAction.RangeAttackAction) || (UNI_markerRef.Visible == false && ChosenAction == PlayersChosenAction.AimedRangeAttackAction))
        {
            PointerNode.LookAt(GetGlobalMousePosition());
            //GD.Print($"chance is {ChanceToHitLabel1.Text}% (or {ShootingFinalDiceVal})");
        }
        // #################################### KLIKNIĘCIE ###################################
        if (ChosenAction == PlayersChosenAction.AimedRangeAttackAction)
        {
            if (Input.IsActionJustPressed("MYMOUSELEFT") && UNI_markerRef.Visible == false)
            {
                if (UCOPS.PrecentCalculationFunction(ShootingFinalDiceVal) == 0 || ShootingFinalDiceVal >= 10)
                {
                    gameMNGR_Script.PlayerPhoneCallWarning("0% TO HIT TARGET");
                    gameMNGR_Script.PlayerGUIRef.PALO(false, false);
                    ResetActionCommitment(true);
                    return;
                }
                if (ShootingRayScript.RayHittenTarget != null)
                {
                    UNI_markerRef.Visible = true;
                    UNI_markerRef.GlobalPosition = GetGlobalMousePosition();
                    PointerNode.LookAt(UNI_markerRef.GlobalPosition);
                    ShootingRayScript.OverrideTarget = UNI_markerRef;
                    if (UCOPS.EnemyPartsToHit.Count > 0)
                    {
                        gameMNGR_Script.ShowListPopUp(UCOPS.EnemyPartsToHit, this);
                    }
                    else
                    {
                        GD.Print("Nie było wystarczająco cześci ciała na liście EnemyPartsToHit");
                        ResetActionCommitment(true);
                    }
                }
                else
                {
                    gameMNGR_Script.PlayerPhoneCallWarning("NO TARGET");
                    gameMNGR_Script.PlayerGUIRef.PALO(false, false);
                    ResetActionCommitment(true);
                    return;
                }

            }
        }
        else if(ChosenAction == PlayersChosenAction.RangeAttackAction || OV_Active == true )
        {
            if (Input.IsActionJustPressed("MYMOUSELEFT") && UNI_markerRef.Visible == false && OV_Active == false)
            {
                UNI_markerRef.Visible = true;
                UNI_markerRef.GlobalPosition = GetGlobalMousePosition();
                PointerNode.LookAt(UNI_markerRef.GlobalPosition);
                ShootingRayScript.OverrideTarget = UNI_markerRef;
                gameMNGR_Script.PlayerGUIRef.PALO(true, false);
            }
            if (OV_Active == true)
            {
                Player_ACT_Confirm(2);
            }
            if (OverwatchArea.GetOverlappingBodies().Count > 0)
            {
                Node2D FirstEnemy = OverwatchArea.GetOverlappingBodies().First();
                OverwachedAreaEntered(FirstEnemy); 
            }
        }
    }
    void OverwachedAreaEntered(Node2D Enemy)
    {
        if (Enemy == null)
        {
            return;
        }
        if (Enemy is CharacterBody2D && gameMNGR_Script.Turn != PawnScript.TeamId && PawnScript.OverwatchStatus == true) // jeśli to w co strzelamy jest char2d i nie jest nasza tura, oraz overwatch jest włączony 
        {
            GD.Print("Jest tura przeciwnika, przeciwnik to Character2d i overwatch został włączony");
            PawnBaseFuncsScript EnemyStats = Enemy as PawnBaseFuncsScript;
            if (EnemyStats.TeamId != PawnScript.TeamId) // jeśli typo nie jest z naszej drużyny 
            {
                GD.Print("Shootfunc aktywowany ");
                ShootingRayScript.Rayactive = true;
                ShootingRayScript.OverrideTarget = Enemy;
                GD.Print($"Cel ustawiony na  {ShootingRayScript.OverrideTarget}");
                if (ShootingRayScript.RayHittenTarget != null)
                {
                    if (PawnScript.WeaponAmmo > 0)
                    {
                        ShootFunc(true);
                    }
                    else
                    {
                        GD.Print("overwatch wydany ");
                    }
                    PawnScript.OverwatchStatus = false;
                    UNI_markerRef.Visible = false;
                    OVButtons.Visible = true;
                }
                else
                {
                    GD.Print("Promień nie trafił w cel ");
                }
            }
        }
    }
    void AimedShotChosenTargetListTrigger(int inx,float probabitity) // to jest zapewne wywoływane z listy części ciała, pytanie czy AI byłoby w stanie ustalić index bezpośrednio ? jeśli tak to ta funkcja może zostać tu
    {
        ShootingTargetLockIndex = inx;
        PartProbability = 3f - Mathf.Clamp(probabitity,0,2.5f)*18; // te 3 kontroluje że strzał w głowe nie jest zbyt OP
        //Mathf.Clamp(probabitity,0,2.5f)*18
        Player_ACT_Confirm(2);
    }
    private void OnMouseEnter() => StatsUI.Visible = true;
    private void OnMouseExit()
    {
        if (!isSelected)
            StatsUI.Visible = false;
    }
    public void Button_ACT1() // accept move order 
    {
        Player_ACT_Confirm(1);
    }
    public void Button_ACT6() // decline move order
    {
        Player_ACT_Decline(1);
    }
    void Button_ACT4() // accept atack order
    {
        Player_ACT_Confirm(2);
    }
    void Button_ACT5() // decline atack order
    {
        Player_ACT_Decline(2);
    }
    void Button_ACT9() // accept melee atack order
    {
        Player_ACT_Confirm(3);
    }
    void Button_ACT10() // decline melee atack order
    {
        Player_ACT_Decline(3);
    }
    void Button_ACT11() // accept overwatch order ?
    {
        Player_ACT_Confirm(4);
    }
    void Button_ACT12() // decline overwatch order ? 
    {
        Player_ACT_Decline(4);
    }
    void Player_ACT_Move()
    {
        Player_ACT_UNI_ChangeTargetMakerButtonNIcon(1,6);
        gameMNGR_Script.ChosenActionFinished = false;
        ChosenAction = PlayersChosenAction.MoveAction;
        MovementAllowenceInyk_ator.Visible = true;
        NavAgent.DebugEnabled = true;
        GD.Print("teraz gracz wybiera ruch...");
    }
    void Player_ACT_Shoot()
    {
        Player_ACT_UNI_ChangeTargetMakerButtonNIcon(4,5);
        gameMNGR_Script.ChosenActionFinished = false;
        ChosenAction = PlayersChosenAction.RangeAttackAction;
        ShootingRayScript.Rayactive = true;
        PointerNode.Visible = true;
        //GD.Print("teraz gracz wybiera cel...");
    }
    void Player_ACT_Punch()
    {    
        Player_ACT_UNI_ChangeTargetMakerButtonNIcon(9,10);    
        ChosenAction = PlayersChosenAction.MeleeAttackAction;
        gameMNGR_Script.ChosenActionFinished = false;
    }
    void Player_ACT_StrongPunch()
    {    
        Player_ACT_UNI_ChangeTargetMakerButtonNIcon(9,10);    
        ChosenAction = PlayersChosenAction.StrongMeleeAttackAction;
        gameMNGR_Script.ChosenActionFinished = false;
    }
    void Player_ACT_AimedShot()
    {
        Player_ACT_UNI_ChangeTargetMakerButtonNIcon(4,5);
        gameMNGR_Script.ChosenActionFinished = false;
        ChosenAction = PlayersChosenAction.AimedRangeAttackAction;
        ShootingRayScript.Rayactive = true;
        PointerNode.Visible = true;
        GD.Print("teraz gracz wybiera celny strzał ...");
    }
    void Player_ACT_Overwatch(int Dump)
    {
        gameMNGR_Script.ChosenActionFinished = false;
        ChosenAction = PlayersChosenAction.OverwatchAction;
        GD.Print("teraz gracz wybiera overwatch ...");
    }
    void Player_ACT_UNI_ChangeTargetMakerButtonNIcon(int ACTI1,int ACTI2)//(Yay/Nay) to wybiera powyższy button ACT (w przyszłości modyfikuj via string, mniej magicznych liczb pls)
    {
        YayButton.OnChangeButtonFunc(ACTI1);
        NayButton.OnChangeButtonFunc(ACTI2);
    }
    void Player_ACT_Confirm(int Index)
    {
        gameMNGR_Script.PlayerGUIRef.PALO(true, false);
        switch (Index)
        {
            case 1: // potwierdzenie ruchu
                UCOPS.ActionMove();
                ResetActionCommitment(false); // ten reset statusu będzie musiał zostać usunięty z tąd gdyż to że dany pionek zakończył TEN ruch nie oznacza że nie może zrobić kolejnego, więc pomyśl nad sprawdzeniem selekcji i deselekcji by działała poprawnie
                break;
            case 2:
                bool AimedOrnot;
                if (PawnScript.ShootingAllowence <= 0 || PawnScript.WeaponAmmo <= 0)
                {
                    GD.Print("You cant shoot fucko' ");
                    ResetActionCommitment(false);
                    break;
                }
                if(ChosenAction == PlayersChosenAction.AimedRangeAttackAction) // jest Aimed albo nie 
                {
                    AimedOrnot = true;
                }
                else // by AimedOrnot nie było przez przypadek nullem, ale jak co to może być prowodyrem w przyszłości 
                {
                    AimedOrnot = false;
                }
                UCOPS.ActionRangeAttack(AimedOrnot,ShootingFinalDiceVal,PartProbability,ShootingTargetLockIndex);
                ResetActionCommitment(false);
                break;
            case 3:
                if (PawnScript.MeleeAllowence <= 0)
                {
                    GD.Print("You cant melee fucko' ");
                    ResetActionCommitment(false);
                    break;
                }
                if (ChosenAction == PlayersChosenAction.StrongMeleeAttackAction)
                {
                    UCOPS.ActionMeleeAttack(true,ShootingTargetLockIndex);
                }
                else
                {
                    UCOPS.ActionMeleeAttack(false,ShootingTargetLockIndex);
                }
                ResetActionCommitment(false);
                break;
            case 4: // overwatch 
            // jak już rozkminisz jak to zaaranżować to weż to daj do UNIcontroll w UCOPS
                UNI_markerRef.Visible = true;
                PawnScript.OverwatchStatus = true;
                OVButtons.Visible = false;
                PawnScript.MP -= 2;
                gameMNGR_Script.TeamsCollectiveMP -= 2;
                if (PawnScript.MP < 0)
                {
                    GD.PrintErr("Błąd kalkulacji MP przy strzale wycelowanym");
                    gameMNGR_Script.TeamsCollectiveMP++;
                }
                ResetActionCommitment(true);
                break;
            default:
                GD.Print("Nie ma takiej akcji");
                ResetActionCommitment(true);
                break;
        }
        PawnScript.CheckFightingCapability();
    }
    void Player_ACT_Decline(int Index)
    {
        gameMNGR_Script.PlayerGUIRef.PALO(true, false);
        switch (Index)
        {
            case 1:
                PawnScript.DistanceMovedByThisPawn = PawnScript.PrevDistance;
                MovementAllowenceInyk_ator.Visible = false;
                NavAgent.DebugEnabled = false;
                ResetActionCommitment(false);
                break;
            case 2:
                ShootingRayScript.OverrideTarget = null;
                ShootingRayScript.Rayactive = false;
                ResetActionCommitment(false);
                break;
            case 3:
                ResetActionCommitment(false);
                break;
            case 4:
                PawnScript.OverwatchStatus = false;
                OVButtons.Visible = true;
                ResetActionCommitment(false);
                break;
            default:
                GD.Print("Nie ma takiej akcji");
                ResetActionCommitment(true);
                break;
        }
        PawnScript.CheckFightingCapability();
    }
    public void ResetActionCommitment(bool forceDeselect)
    {
        ChosenAction = PlayersChosenAction.None;
        ShootingTargetLockIndex = 999; // na wszelki wypadek
        PartProbability = 0; //prawdopodobieństwo by trafić w daną część ciała
        UNI_markerRef.Visible = false;
        gameMNGR_Script.ChosenActionFinished = true;
        gameMNGR_Script.PlayerGUIRef.PALO(false, true);
        if (PawnScript.MP <= 0 || forceDeselect == true)
        {
            ResetSelectedStatus();
        }
        ShootingRayScript.OverrideTarget = null;
        WideMelee.Visible = false;
        StrongMelee.Visible = false;
        StatsUI.Visible = false;
        ShootingRayScript.Rayactive = false;
        MovementAllowenceInyk_ator.Visible = false;
        NavAgent.DebugEnabled = false;
        PointerNode.Visible = false;
        ChanceToHitLabel1.Visible = false;
        PawnScript.CheckFightingCapability();
    }
    public void ResetSelectedStatus()
    {
        gameMNGR_Script.PlayerGUIRef.PALO(false, false);
        gameMNGR_Script.DeselectPawn();
        UnsubscribeFromUIControlls();
        isSelected = false;
    }

    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx) // selekcja Danego pionka 
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            if (!isSelected && PawnScript.MP > 0 && PawnScript.TeamId == gameMNGR_Script.Turn) // jeśli nie jest zaznaczony, jeśli ma punkty ruchu i jak należy do ciebie 
            {
                gameMNGR_Script.Call("SelectPawn", PawnScript);
                if (gameMNGR_Script.SelectedPawn.Name == PawnScript.Name)
                {
                    isSelected = true;
                    //IBBN.Visible = true;
                }
                else // to już chyba nie potrzebne ale
                {
                    isSelected = false;
                }
            }
            else
            {
                isSelected = false;
            }
        }
    }
}
