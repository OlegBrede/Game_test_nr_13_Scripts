using Godot;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class PawnPlayerController : Node2D
{
    [Export] public PawnBaseFuncsScript PawnScript;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D MoveMarker; // target marker na ruch TO DO .: - na co tyle markerów ? zrób jeden i miej spokój
    [Export] public Node2D TargetMarker; // target marker na strzał
    [Export] public Node2D MeleeMarker; // target marker na wpierdol
    [Export] public Node2D MeleeSlcieNode;
    [Export] public NavigationAgent2D NavAgent;
    [Export] Sprite2D movementsprite;
    [Export] UNI_LOSRayCalcScript ShootingRayScript;
    [Export] Node2D IBBN; // interact buttons bucket node
    [Export] Area2D MeleeAttackArea;
    Node2D MovementAllowenceInyk_ator;
    GameMNGR_Script gameMNGR_Script;
    Area2D OverlapingBodiesArea;
    Label ChanceToHitLabel1;
    enum PlayersChosenAction
    {
        None, MoveAction, RangeAttackAction, MeleeAttackAction, UseAction
    }
    private PlayersChosenAction ChosenAction = PlayersChosenAction.None;
    private bool isSelected = false;
    private Vector2 texSize;
    private float baseRadius;
    private float ShootingFinalDiceVal = 0;
    private float MeleeFinalDiceVal = 0;
    bool Actionconfimrm = false;
    
    public override void _Ready()
    {
        TargetMarker.Visible = false;
        ChanceToHitLabel1 = TargetMarker.GetNode<Label>("Label");
        GD.Print("PAMIĘTAJ by zawsze sprawdzić na którym pionku testujesz swe dodatki");
        MeleeSlcieNode.Visible = false;
        MovementAllowenceInyk_ator = GetNode<Node2D>("MoveIndicator");
        MovementAllowenceInyk_ator.Visible = false;
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");

        var statArea = PawnScript.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        StatsUI.Visible = false;
        MoveMarker.Visible = false;

        OverlapingBodiesArea = MoveMarker.GetNode<Area2D>("Area2D");
        var circle = PawnScript.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
        NavAgent.Radius = circle.Radius;
        IBBN.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (StatsUI.Visible)
        {
            StatsLabel.Text = $"{PawnScript.UnitName}\n{PawnScript.TeamId}\nHP {Mathf.RoundToInt((float)PawnScript.Integrity / (float)PawnScript.BaseIntegrity * 100f)}%\nMP {PawnScript.MP}\nStatus {PawnScript.PawnMoveStatus}\nDistance ({PawnScript.DistanceMovedByThisPawn})";
        }
        if (ChosenAction == PlayersChosenAction.MoveAction)
        {
            bool CanGoThere;
            if (MoveMarker.Visible == false)
            { // jeśli nie ma postawionego punktu ruchu śledź kursor
                MovementAllowenceInyk_ator.GlobalPosition = GetGlobalMousePosition();
                NavAgent.TargetPosition = MovementAllowenceInyk_ator.GlobalPosition;
                NavAgent.GetNextPathPosition();
                OverlapingBodiesArea.GlobalPosition = GetGlobalMousePosition();
            }
            if (NavAgent.GetPathLength() <= PawnScript.MAD && IsTargetPositionFreeAsync()) // pokazujemy graczu że może tam stanąć 
            {
                movementsprite.Modulate = new Color(0, 1, 0);
                CanGoThere = true;
            }
            else
            {
                movementsprite.Modulate = new Color(1, 0, 0);
                CanGoThere = false;
            }
            PawnScript.DistanceMovedByThisPawn = NavAgent.GetPathLength();
            //GD.Print($"Pionek rusza się o {PawnScript.DistanceMovedByThisPawn}");
            if (Input.IsActionJustPressed("MYMOUSELEFT") && CanGoThere == true && MoveMarker.Visible == false) // można wybrać marker tylko wtedy gdy jego pozycja jest potwierdzona przez dystans 
            {
                MoveMarker.GlobalPosition = GetGlobalMousePosition();
                OverlapingBodiesArea.GlobalPosition = MoveMarker.GlobalPosition;
                MoveMarker.Visible = true;
                gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",true);
            }
        }
        if (ChosenAction == PlayersChosenAction.RangeAttackAction)
        {
            // ################# KALKULACJA NA PODSTAWIE ZASIĘGU ######################
            bool ShotPosibility;
            float TargetRangeModifier;
            float TargetOwnMoveModifier;
            float TargetEnemyMoveModifier;
            float ModiRayLenghCorrector = ShootingRayScript.Raylengh - PawnScript.DistanceZero;
            if (ModiRayLenghCorrector < PawnScript.WeaponRange)
            {
                ShotPosibility = true;
                //TargetRangeModifier = Mathf.Clamp(ShootingRayScript.Raylengh / PawnScript.WeaponRange, 0, 1f);
                TargetRangeModifier = Mathf.Clamp(ModiRayLenghCorrector / PawnScript.WeaponRange,0,1f) * PawnScript.Penalty_range;
                //GD.Print($"TargetRangeModifier {TargetRangeModifier}");
            }
            else
            {
                ShotPosibility = false;
                TargetRangeModifier = 11;
                //GD.Print("Pionek nie trafi");
            }
            // ################# KALKULACJA NA PODSTAWIE WŁASNEGO RUCHU ######################
            if (PawnScript.PawnMoveStatus == PawnMoveState.Moving)
            {
                TargetOwnMoveModifier = Mathf.Clamp(PawnScript.DistanceMovedByThisPawn / PawnScript.MAD, 0, 1f) * PawnScript.Penalty_shooter;
                //GD.Print($"Na cel wpływa modyfikator bo strzelec się rusza {TargetOwnMoveModifier}");
            }
            else
            {
                TargetOwnMoveModifier = 0; 
            }
            // ################# KALKULACJA NA PODSTAWIE RUCHU PRZECIWNIKA ######################
            if (ShootingRayScript.RayHittenTarget != null)
            {
                PawnBaseFuncsScript PBFS = ShootingRayScript.RayHittenTarget as PawnBaseFuncsScript; //TO DO .: ten skrypt zakłada że każdy characterbody ma ten skrypt, sprawdź najpierw czy ma ten skrypt  
                if (PBFS.PawnMoveStatus == PawnMoveState.Moving)
                {
                    TargetEnemyMoveModifier = Mathf.Clamp(PBFS.DistanceMovedByThisPawn / PBFS.MAD, 0, 1f) * PawnScript.Penalty_target;
                    //GD.Print($"Na cel wpływa modyfikator bo cel się rusza {TargetEnemyMoveModifier}");
                }
                else
                {
                    TargetEnemyMoveModifier = 0;
                }
            }
            else
            {
                TargetEnemyMoveModifier = 0;
            }
            // ############################# KULMINACJA WARTOŚCI KOŃCOWEJ #######################
            float penaltyTotal;
            if (ShotPosibility == true)
            {
                penaltyTotal = Mathf.Clamp(TargetRangeModifier + TargetOwnMoveModifier + TargetEnemyMoveModifier, 0f, 1f);
                ShootingFinalDiceVal = penaltyTotal * 10;
            }
            else
            {
                penaltyTotal = 0;
                ShootingFinalDiceVal = 11;
            }

            int Precent;
            if (ShootingFinalDiceVal < 10)
            {
                Precent = Mathf.RoundToInt(100f - (ShootingFinalDiceVal * 10f));
                ChanceToHitLabel1.Text = $"{Precent}%";
            }
            else
            {
                Precent = 0;
                ChanceToHitLabel1.Text = $"{Precent}%";
            }
            if (TargetMarker.Visible == false)
            {
                //GD.Print($"Range {ModiRayLenghCorrector} so chance is {Precent}% (or {ShootingFinalDiceVal})with mod1 = {TargetOwnMoveModifier} & mod2 = {TargetEnemyMoveModifier}");
            }
            // #################################### KLIKNIĘCIE ###################################
            if (Input.IsActionJustPressed("MYMOUSELEFT") && TargetMarker.Visible == false)
            {
                TargetMarker.Visible = true;
                TargetMarker.GlobalPosition = GetGlobalMousePosition();
                ShootingRayScript.OverrideTarget = TargetMarker;
                gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",true);
            }
        }
        if (ChosenAction == PlayersChosenAction.MeleeAttackAction)
        {
            if (MeleeMarker.Visible == false)
            {
                MeleeSlcieNode.LookAt(GetGlobalMousePosition());
            }
            else
            {
                MeleeSlcieNode.LookAt(MeleeMarker.GlobalPosition);
            }
            if (Input.IsActionJustPressed("MYMOUSELEFT") && MeleeMarker.Visible == false)
            {
                MeleeMarker.Visible = true;
                MeleeMarker.GlobalPosition = GetGlobalMousePosition();
                gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",true);
            }

        }
    }
    private bool IsTargetPositionFreeAsync()
    {
        var overlaps = OverlapingBodiesArea.GetOverlappingBodies();
        foreach (var body in overlaps)
        {
            if (body is StaticBody2D)
                return false;
            if (body is CharacterBody2D && body.GetInstanceId != PawnScript.GetInstanceId)
                return false;
        }
        return true;
    }
    private void OnMouseEnter() => StatsUI.Visible = true;
    private void OnMouseExit()
    {
        if (!isSelected)
            StatsUI.Visible = false;
    }
    void Button_ACT1() // accept move order 
    {
        Player_ACT_Confirm(1);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Button_ACT6() // decline move order
    {
        Player_ACT_Decline(1);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Button_ACT4() // accept atack order
    {
        Player_ACT_Confirm(2);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Button_ACT5() // decline atack order
    {
        Player_ACT_Decline(2);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Button_ACT9() // accept melee atack order
    {
        Player_ACT_Confirm(3);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Button_ACT10() // decline melee atack order
    {
        Player_ACT_Decline(3);
        gameMNGR_Script.PlayerPhoneCallbackFlag("PALO",false);
    }
    void Player_ACT_Move(int Dump)
    {
        gameMNGR_Script.ChosenActionFinished = false;
        IBBN.Visible = false;
        ChosenAction = PlayersChosenAction.MoveAction;
        MovementAllowenceInyk_ator.Visible = true;
        NavAgent.DebugEnabled = true;
        //GD.Print("teraz gracz wybiera ruch...");
    }
    void Player_ACT_Shoot(int Dump)
    {
        gameMNGR_Script.ChosenActionFinished = false;
        IBBN.Visible = false;
        ChosenAction = PlayersChosenAction.RangeAttackAction;
        ShootingRayScript.Rayactive = true;
        //GD.Print("teraz gracz wybiera cel...");
    }
    void Player_ACT_Punch(int Dump)
    {        
        ChosenAction = PlayersChosenAction.MeleeAttackAction;
        MeleeSlcieNode.Visible = true;
        gameMNGR_Script.ChosenActionFinished = false;
        IBBN.Visible = false;
    }
    void Player_ACT_Use(int Dump)
    {
        GD.Print("teraz gracz wybiera urzycie...");
    }
    void Player_ACT_Confirm(int Index)
    {
        switch(Index){
            case 1:
                PawnScript.MP--;
                var path = NavAgent.GetCurrentNavigationPath();
                if (path.Length > 0)
                {
                    var targetPos = path[path.Length - 1];
                    PawnScript.GlobalPosition = targetPos;
                    if (PawnScript.PawnMoveStatus == PawnMoveState.Moving)
                    {
                        float Addtive = PawnScript.PrevDistance + PawnScript.DistanceMovedByThisPawn;
                        //GD.Print(Addtive);// nie mam pojęcia czemu to nie działa , ale nie jest to na tyle inwazyjne żeby się za to raptownie brać, bo pionek zurzuwając dwa punkty ruchu na to by zresetować prędkość nic nie osiągnie 
                        PawnScript.PrevDistance = Addtive;
                    }
                    else
                    {
                        PawnScript.PrevDistance = PawnScript.DistanceMovedByThisPawn;
                    }
                    PawnScript.PawnMoveStatus = PawnMoveState.Moving;
                }
                ResetSelectedStatus(); // ten reset statusu będzie musiał zostać usunięty z tąd gdyż to że dany pionek zakończył TEN ruch nie oznacza że nie może zrobić kolejnego, więc pomyśl nad sprawdzeniem selekcji i deselekcji by działała poprawnie
                break;
            case 2:
                if (PawnScript.ShootingAllowence <= 0)
                {
                    GD.Print("You cant shoot fucko' ");
                    ResetSelectedStatus();
                    break;
                }
                PawnScript.MP--;
                PawnScript.PlayAttackAnim();
                if (ShootingRayScript.RayHittenTarget != null)
                {
                    ShootingRayScript.RayHittenTarget.Call("CalculateHit", PawnScript.WeaponDamage, ShootingFinalDiceVal, PawnScript.UnitName);
                    gameMNGR_Script.Call("CaptureAction", PawnScript.GlobalPosition, ShootingRayScript.RayHittenTarget.GlobalPosition);
                }
                ResetSelectedStatus();
                break;
            case 3:
                if (PawnScript.MeleeAllowence <= 0)
                {
                    GD.Print("You cant melee fucko' ");
                    ResetSelectedStatus();
                    break;
                }
                PawnScript.MP--;
                PawnScript.PlayAttackAnim();
                MeleeAttackArea.ForceUpdateTransform();
                var overlaps = MeleeAttackArea.GetOverlappingBodies();
                foreach (var body in overlaps)
                {
                    if (body is CharacterBody2D)
                    {
                        PawnBaseFuncsScript PS = body as PawnBaseFuncsScript;
                        if (PS.TeamId != PawnScript.TeamId) // nie wiem po chuj to jest bo pionek uderzający przecierz może przywalić w swojego 
                        {
                            PS.Call("CalculateHit",PawnScript.MeleeDamage,2.5f,PawnScript.UnitName);
                        }
                    }
                }
                ResetSelectedStatus();
                break;
            default:
                GD.Print("Nie ma takiej akcji");
                ResetSelectedStatus();
                break;
        }
    }
    void Player_ACT_Decline(int Index)
    {
        switch(Index){
            case 1:
                PawnScript.DistanceMovedByThisPawn = PawnScript.PrevDistance;
                MoveMarker.Visible = false;
                MovementAllowenceInyk_ator.Visible = false;
                NavAgent.DebugEnabled = false;
                ResetSelectedStatus();
                break;
            case 2:
                ShootingRayScript.OverrideTarget = null;
                ShootingRayScript.Rayactive = false;
                TargetMarker.Visible = false;
                ResetSelectedStatus();
                break;
            case 3:
                MeleeSlcieNode.Visible = false;
                MeleeMarker.Visible = false;
                ResetSelectedStatus();
                break;
            default:
                GD.Print("Nie ma takiej akcji");
                ResetSelectedStatus();
                break;
        }
    }
    public void ResetSelectedStatus() // TO DO .: POSORTUJ BOOLE 
    {
        ChosenAction = PlayersChosenAction.None;
        gameMNGR_Script.Call("DeselectPawn");
        ShootingRayScript.OverrideTarget = null;
        gameMNGR_Script.ChosenActionFinished = true;
        isSelected = false;
        StatsUI.Visible = false;
        IBBN.Visible = false;
        MeleeSlcieNode.Visible = false;
        MeleeSlcieNode.Visible = false;
        MeleeMarker.Visible = false;
        TargetMarker.Visible = false;
        ShootingRayScript.Rayactive = false;
        MoveMarker.Visible = false;
        MovementAllowenceInyk_ator.Visible = false;
        NavAgent.DebugEnabled = false;
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
                    IBBN.Visible = false;
                }
            }
            else
            {
                isSelected = false;
                IBBN.Visible = false;
            }
        }
    }
}
