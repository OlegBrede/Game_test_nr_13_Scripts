using Godot;
using System;

public partial class PawnPlayerController : Node2D
{
    [Export] public PawnBaseFuncsScript Pawn;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D MoveMarker; // target marker na ruch
    [Export] public Node2D TargetMarker; // target marker na strzał
    [Export] public Area2D MAarea;
    [Export] public Sprite2D MACircleSprite;
    [Export] public Node2D BFPC1; // button for player controll
    [Export] public Node2D BFPC2; // button for player controll
    [Export] public NavigationAgent2D NavAgent;
    GameMNGR_Script gameMNGR_Script;
    private bool isSelected = false;
    private bool waitingForMoveTarget = false;
    private bool waitingForAimTarget = false;
    private Vector2 texSize;
    private float baseRadius;
    bool Actionconfimrm = false;
    Area2D area;
    public override void _Ready()
    {
        BFPC1.Visible = false;
        BFPC2.Visible = false;
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        MACircleSprite.Visible = false;

        var statArea = Pawn.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        MAarea.InputEvent += OnPawnClicked;

        StatsUI.Visible = false;
        MoveMarker.Visible = false;
        MAarea.Visible = false; // obszar ruchu niewidoczny na start

        area = MoveMarker.GetNode<Area2D>("Area2D");
        var circle = Pawn.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
        NavAgent.Radius = circle.Radius;
    }

    public override void _Process(double delta)
    {
        if (StatsUI.Visible)
        {
            StatsLabel.Text = $"{Pawn.UnitName}\n{Pawn.TeamId}\nHP {Pawn.HP}\nMP {Pawn.MP}";
        }
        
    }

    private void OnMouseEnter() => StatsUI.Visible = true;
    private void OnMouseExit()
    {
        if (!isSelected)
            StatsUI.Visible = false;
    }
    private void ChangeVisibleMASpriteSize()
    {
        texSize = MACircleSprite.Texture.GetSize();
        baseRadius = texSize.X / 2f;
        float scale = Pawn.MA / baseRadius;
        MACircleSprite.Scale = new Vector2(scale, scale);
    }
    private bool IsTargetPositionFree(Vector2 pos)
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
    void Button_ACT1() // Move order complete
    {
        var path = NavAgent.GetCurrentNavigationPath();
        if (path.Length > 0)
        {
            var targetPos = path[path.Length - 1];
            Pawn.GlobalPosition = targetPos;
        }
        MoveMarker.Visible = false;
        Pawn.MP--;
    }
    void Button_ACT6() // decline move order
    {
        isSelected = false;
        waitingForMoveTarget = false;
        StatsUI.Visible = false;

        MAarea.Visible = false;
        MACircleSprite.Visible = false;
        MoveMarker.Visible = false;
    }
    void Button_ACT2() // ruch wybrano z akcji
    {
        if (isSelected == true)
        {
            BFPC1.Visible = false;
            BFPC2.Visible = false;

            waitingForMoveTarget = true;    // czekamy na klik w obszar ruchu
            MAarea.Visible = true;          // pokaż zasięg
            ChangeVisibleMASpriteSize();    // zmień sprite kółka
            MACircleSprite.Visible = true;
        }
    }
    void Button_ACT3() // atak wybrano z akcji
    {
        if (isSelected == true)
        {
            waitingForAimTarget = true;
            BFPC1.Visible = false;
            BFPC2.Visible = false;
            GD.Print("teraz gracz wybiera cel...");
        }
    }
    void Button_ACT4() // accept atack order
    {
        Pawn.MP--;
    }
    void Button_ACT5() // decline atack order
    {
        isSelected = false;
        waitingForAimTarget = false;
        StatsUI.Visible = false;
    }

    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx) // selekcja Danego pionka 
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            if (!isSelected && Pawn.MP > 0 && Pawn.TeamId == gameMNGR_Script.Turn) // jeśli nie jest zaznaczony, jeśli ma punkty ruchu i jak należy do ciebie 
            {
                isSelected = true;

                BFPC1.Visible = true;
                BFPC2.Visible = true;
            }
        }
    }

    private void OnPawnClicked(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (waitingForMoveTarget == true) {
            if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) // zmiana sposobu poruszania będzie wymagać zmiany tej metody
            {
                var worldPos = Pawn.GetGlobalMousePosition();
                var localPos = Pawn.ToLocal(worldPos);

                var circle = MAarea.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
                if (circle != null)
                {
                    if (Pawn.MP > 0)
                    {
                        circle.Radius = Pawn.MA;
                        if (localPos.Length() <= circle.Radius)
                        {
                            MoveMarker.Visible = true;
                            MoveMarker.GlobalPosition = worldPos;
                            NavAgent.TargetPosition = MoveMarker.GlobalPosition;
                            NavAgent.GetNextPathPosition();
                            //TO DO , ogarnąć jak podejżeć collisionshape w edytorze bo chyba zmienili a potem naprawić to szajstwo 
                            if (IsTargetPositionFree(worldPos))
                            {
                                //TargetMarker.Visible = true;
                                //TargetMarker.GlobalPosition = worldPos;
                            }
                            else
                            {
                                GD.Print("Nie moge tu stanąć");
                            }
                        }
                    }
                }
                // zakończ wybór
                isSelected = false;
                waitingForMoveTarget = false;
                MAarea.Visible = false;
                StatsUI.Visible = false;
                MACircleSprite.Visible = false;
            }
        } else if (waitingForAimTarget == true) {
            if(@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
            {
                GD.Print("teraz gracz klika cel...");
                var worldPos = Pawn.GetGlobalMousePosition();
                if (Pawn.MP > 0)
                {
                    GD.Print("teraz graczu pojawia się prompt na cel...");
                    TargetMarker.Visible = true;
                    TargetMarker.GlobalPosition = worldPos;
                }
                isSelected = false;
                waitingForAimTarget = false;
                StatsUI.Visible = false;
            }
        }
        
    }
}
