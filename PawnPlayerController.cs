using Godot;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class PawnPlayerController : Node2D
{
    [Export] public PawnBaseFuncsScript Pawn;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D MoveMarker; // target marker na ruch
    [Export] public Node2D TargetMarker; // target marker na strzał
    [Export] public Node2D MeleeMarker; // target marker na wpierdol
    [Export] public Node2D MeleeSlcieNode;
    [Export] public NavigationAgent2D NavAgent;
    [Export] Sprite2D movementsprite;
    [Export] UNI_LOSRayCalcScript ShootingRay;
    [Export] Node2D IBBN; // interact buttons bucket node
    [Export] Area2D MeleeAttackArea;
    Node2D MovementAllowenceInyk_ator;
    GameMNGR_Script gameMNGR_Script;
    Area2D OverlapingBodiesArea;
    enum PlayersChosenAction
    {
        None, MoveAction, RangeAttackAction, MeleeAttackAction, UseAction
    }
    private PlayersChosenAction ChosenAction = PlayersChosenAction.None;
    private bool isSelected = false;
    private Vector2 texSize;
    private float baseRadius;
    bool Actionconfimrm = false;
    
    public override void _Ready()
    {
        GD.Print("PAMIĘTAJ by zawsze sprawdzić na którym pionku testujesz swe dodatki");
        MeleeSlcieNode.Visible = false;
        MovementAllowenceInyk_ator = GetNode<Node2D>("MoveIndicator");
        MovementAllowenceInyk_ator.Visible = false;
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");

        var statArea = Pawn.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        StatsUI.Visible = false;
        MoveMarker.Visible = false;

        OverlapingBodiesArea = MoveMarker.GetNode<Area2D>("Area2D");
        var circle = Pawn.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
        NavAgent.Radius = circle.Radius;
        IBBN.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (StatsUI.Visible)
        {
            StatsLabel.Text = $"{Pawn.UnitName}\n{Pawn.TeamId}\nHP {Mathf.RoundToInt((float)Pawn.Integrity / (float)Pawn.BaseIntegrity * 100f)}%\nMP {Pawn.MP}";
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
            if (NavAgent.GetPathLength() <= Pawn.MAD && IsTargetPositionFreeAsync()) // pokazujemy graczu że może tam stanąć 
            {
                movementsprite.Modulate = new Color(0, 1, 0);
                CanGoThere = true;
            }
            else
            {
                movementsprite.Modulate = new Color(1, 0, 0);
                CanGoThere = false;
            }
            if (Input.IsActionJustPressed("MYMOUSELEFT") && CanGoThere == true && MoveMarker.Visible == false) // można wybrać marker tylko wtedy gdy jego pozycja jest potwierdzona przez dystans 
            {
                MoveMarker.GlobalPosition = GetGlobalMousePosition();
                OverlapingBodiesArea.GlobalPosition = MoveMarker.GlobalPosition;
                MoveMarker.Visible = true;
                gameMNGR_Script.PlayerPhoneCallback();
            }
        }
        if (ChosenAction == PlayersChosenAction.RangeAttackAction)
        {
            if (Input.IsActionJustPressed("MYMOUSELEFT") && TargetMarker.Visible == false)
            {
                TargetMarker.Visible = true;
                TargetMarker.GlobalPosition = GetGlobalMousePosition();
                ShootingRay.OverrideTarget = TargetMarker;
                gameMNGR_Script.PlayerPhoneCallback();
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
                gameMNGR_Script.PlayerPhoneCallback();
            }

        }
    }
    private bool IsTargetPositionFreeAsync()
    {
        var overlaps = OverlapingBodiesArea.GetOverlappingBodies();
        foreach (var body in overlaps)
        {
            if (body is StaticBody2D || body is CharacterBody2D)
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
    void Button_ACT2() // ruch wybrano z akcji
    {
        if (isSelected == true)
        {
            Player_ACT_Move(0);
        }
    }
    void Button_ACT3() // atak wybrano z akcji
    {
        if (isSelected == true)
        {
            Player_ACT_Shoot(0);
        }
    }
    void Button_ACT7() // Urzycie wybrano z akcji
    {
        if (isSelected == true)
        {
            Player_ACT_Use(0);
        }
    }
    void Button_ACT8() // Atak wręcz wybrano z akcji
    {
        if (isSelected == true)
        {
            Player_ACT_Punch(0);
        }
    }
    void Button_ACT1() // accept move order 
    {
        Player_ACT_Confirm(1);
    }
    void Button_ACT6() // decline move order
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
        ShootingRay.Rayactive = true;
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
                Pawn.MP--;
                var path = NavAgent.GetCurrentNavigationPath();
                if (path.Length > 0)
                {
                    var targetPos = path[path.Length - 1];
                    Pawn.GlobalPosition = targetPos;
                }
                MoveMarker.Visible = false;
                MovementAllowenceInyk_ator.Visible = false;
                NavAgent.DebugEnabled = false;
                ResetSelectedStatus(); // ten reset statusu będzie musiał zostać usunięty z tąd gdyż to że dany pionek zakończył TEN ruch nie oznacza że nie może zrobić kolejnego, więc pomyśl nad sprawdzeniem selekcji i deselekcji by działała poprawnie
                break;
            case 2:
                Pawn.MP--;
                Pawn.PlayAttackAnim();
                TargetMarker.Visible = false;
                ShootingRay.Rayactive = false;
                if (ShootingRay.RayHittenTarget != null) {
                    ShootingRay.RayHittenTarget.Call("CalculateHit", Pawn.WeaponDamage, 2.5f,Pawn.UnitName);
                    gameMNGR_Script.Call("CaptureAction",Pawn.GlobalPosition,ShootingRay.RayHittenTarget.GlobalPosition);
                }
                ShootingRay.OverrideTarget = null;
                ResetSelectedStatus();
                break;
            case 3:
                Pawn.MP--;
                Pawn.PlayAttackAnim();
                MeleeAttackArea.ForceUpdateTransform();
                var overlaps = MeleeAttackArea.GetOverlappingBodies();
                foreach (var body in overlaps)
                {
                    if (body is CharacterBody2D)
                    {
                        PawnBaseFuncsScript PS = body as PawnBaseFuncsScript;
                        if (PS.TeamId != Pawn.TeamId) // nie wiem po chuj to jest bo pionek uderzający przecierz może przywalić w swojego 
                        {
                            PS.Call("CalculateHit",Pawn.MeleeDamage,2.5f,Pawn.UnitName);
                        }
                    }
                }
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
    void Player_ACT_Decline(int Index)
    {
        switch(Index){
            case 1:
                MoveMarker.Visible = false;
                MovementAllowenceInyk_ator.Visible = false;
                NavAgent.DebugEnabled = false;
                ResetSelectedStatus();
                break;
            case 2:
                ShootingRay.OverrideTarget = null;
                ShootingRay.Rayactive = false;
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
    public void ResetSelectedStatus()
    {
        ChosenAction = PlayersChosenAction.None;
        gameMNGR_Script.Call("DeselectPawn");
        gameMNGR_Script.ChosenActionFinished = true;
        isSelected = false;
        StatsUI.Visible = false;
        IBBN.Visible = false;
        MeleeSlcieNode.Visible = false;
    }

    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx) // selekcja Danego pionka 
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            if (!isSelected && Pawn.MP > 0 && Pawn.TeamId == gameMNGR_Script.Turn) // jeśli nie jest zaznaczony, jeśli ma punkty ruchu i jak należy do ciebie 
            {
                gameMNGR_Script.Call("SelectPawn", Pawn);
                if (gameMNGR_Script.SelectedPawn.Name == Pawn.Name)
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
