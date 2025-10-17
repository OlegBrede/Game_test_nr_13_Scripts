using Godot;
using System;
using System.Collections;

public partial class PawnPlayerController : Node2D
{
    [Export] public PawnBaseFuncsScript Pawn;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D MoveMarker; // target marker na ruch
    [Export] public Node2D TargetMarker; // target marker na strzał
    [Export] public NavigationAgent2D NavAgent;
    [Export] Sprite2D movementsprite;
    [Export] UNI_LOSRayCalcScript ShootingRay;
    [Export] Node2D IBBN; // interact buttons bucket node 
    Node2D MovementAllowenceInyk_ator;
    GameMNGR_Script gameMNGR_Script;
    Area2D area;
    enum PlayersChosenAction
    {
        None, MoveAction, AttackAction 
    }
    private PlayersChosenAction ChosenAction = PlayersChosenAction.None;
    private bool isSelected = false;
    private Vector2 texSize;
    private float baseRadius;
    bool Actionconfimrm = false;
    
    public override void _Ready()
    {
        GD.Print("PAMIĘTAJ by zawsze sprawdzić na którym pionku testujesz swe dodatki");
        MovementAllowenceInyk_ator = GetNode<Node2D>("MoveIndicator");
        
        MovementAllowenceInyk_ator.Visible = false;
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");

        var statArea = Pawn.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        StatsUI.Visible = false;
        MoveMarker.Visible = false;

        area = MoveMarker.GetNode<Area2D>("Area2D");
        var circle = Pawn.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
        NavAgent.Radius = circle.Radius;
        IBBN.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (StatsUI.Visible)
        {
            StatsLabel.Text = $"{Pawn.UnitName}\n{Pawn.TeamId}\nHP {Pawn.HP}\nMP {Pawn.MP}";
        }
        if (ChosenAction == PlayersChosenAction.MoveAction)
        {
            bool CanGoThere;
            if (MoveMarker.Visible == false) { // jeśli nie ma postawionego punktu ruchu śledź kursor
                MovementAllowenceInyk_ator.GlobalPosition = GetGlobalMousePosition();
                NavAgent.TargetPosition = MovementAllowenceInyk_ator.GlobalPosition;
                NavAgent.GetNextPathPosition();
            }
            if (NavAgent.GetPathLength() <= Pawn.MAD) // pokazujemy graczu że może tam stanąć 
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
                MoveMarker.Visible = true;
                MoveMarker.GlobalPosition = GetGlobalMousePosition();
            }
        }
        if (ChosenAction == PlayersChosenAction.AttackAction)
        {
            if (Input.IsActionJustPressed("MYMOUSELEFT") && TargetMarker.Visible == false)
            {
                TargetMarker.Visible = true;
                TargetMarker.GlobalPosition = GetGlobalMousePosition();
                ShootingRay.OverrideTarget = TargetMarker;
            }
        }
    }

    private void OnMouseEnter() => StatsUI.Visible = true;
    private void OnMouseExit()
    {
        if (!isSelected)
            StatsUI.Visible = false;
    }
    private bool IsTargetPositionFree(Vector2 pos) // chwilowo nieurzywany natomiast potrzebny później
    {
        // poczekaj jedną fizyczną klatkę żeby silnik zaktualizował overlapy
        
        area.ForceUpdateTransform();
        var overlaps = area.GetOverlappingBodies();

        foreach (var body in overlaps)
        {
            if (body is StaticBody2D || body is CharacterBody2D)
            {
                return false; // kolizja
            }
        }
        return true;
    }
    void Button_ACT2() // ruch wybrano z akcji
    {
        if (isSelected == true)
        {
            gameMNGR_Script.ChosenActionFinished = false;
            IBBN.Visible = false;
            ChosenAction = PlayersChosenAction.MoveAction;
            MovementAllowenceInyk_ator.Visible = true;
            NavAgent.DebugEnabled = true;
            //GD.Print("teraz gracz wybiera ruch...");
        }
    }
    void Button_ACT3() // atak wybrano z akcji
    {
        if (isSelected == true)
        {
            gameMNGR_Script.ChosenActionFinished = false;
            IBBN.Visible = false;
            ChosenAction = PlayersChosenAction.AttackAction;
            ShootingRay.Rayactive = true;
            //GD.Print("teraz gracz wybiera cel...");
        }
    }
    void Button_ACT7() // Urzycie wybrano z akcji
    {
        GD.Print("teraz gracz wybiera urzycie...");
    }
    void Button_ACT8() // Atak wręcz wybrano z akcji
    {
        GD.Print("teraz gracz wybiera atak wręcz...");
    }
    void Button_ACT1() // accept move order 
    {
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
        ResetSelectedStatus();
    }
    void Button_ACT6() // decline move order
    {
        MoveMarker.Visible = false;
        MovementAllowenceInyk_ator.Visible = false;
        NavAgent.DebugEnabled = false;
        ResetSelectedStatus();
    }
    void Button_ACT4() // accept atack order
    {
        Pawn.MP--;
        Pawn.PlayAttackAnim();
        TargetMarker.Visible = false;
        ShootingRay.Rayactive = false;
        if (ShootingRay.RayHittenTarget != null) {
            ShootingRay.RayHittenTarget.Call("TakeDamage",Pawn.WeaponDamage);
        }
        ShootingRay.OverrideTarget = null;
        ResetSelectedStatus();
    }
    void Button_ACT5() // decline atack order
    {
        ShootingRay.OverrideTarget = null;
        ShootingRay.Rayactive = false;
        TargetMarker.Visible = false;
        ResetSelectedStatus();
    }
    public void ResetSelectedStatus()
    {
        ChosenAction = PlayersChosenAction.None;
        gameMNGR_Script.Call("DeselectPawn");
        gameMNGR_Script.ChosenActionFinished = true;
        isSelected = false;
        StatsUI.Visible = false;
        IBBN.Visible = false;
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
                    IBBN.Visible = true;
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
