using Godot;
using System;

public partial class PawnPlayerController : Node
{
    [Export] public PawnBaseFuncsScript Pawn;
    [Export] public Node2D StatsUI;
    [Export] public Label StatsLabel;
    [Export] public Node2D TargetMarker;
    [Export] public Area2D MAarea;
    [Export] public Sprite2D MACircleSprite;

    private bool isSelected = false;
    private bool waitingForTarget = false;

    private Vector2 texSize;
    private float baseRadius;
    bool Actionconfimrm = false;
    public override void _Ready()
    {
        MACircleSprite.Visible = false;

        var statArea = Pawn.GetNode<Area2D>("Area2DStatBox");
        statArea.MouseEntered += OnMouseEnter;
        statArea.MouseExited += OnMouseExit;
        statArea.InputEvent += OnAreaInputEvent;

        MAarea.InputEvent += OnPawnClicked;

        StatsUI.Visible = false;
        TargetMarker.Visible = false;
        MAarea.Visible = false; // obszar ruchu niewidoczny na start
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
        var area = TargetMarker.GetNode<Area2D>("Area2D");
        area.GlobalPosition = pos;

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
        Pawn.GlobalPosition = TargetMarker.GlobalPosition;
        TargetMarker.Visible = false;
        Pawn.MP--;
    }

    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx)
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            if (!isSelected && Pawn.MP > 0)
            {
                isSelected = true;
                waitingForTarget = true;   // czekamy na klik w obszar ruchu
                MAarea.Visible = true;     // pokaż zasięg
                ChangeVisibleMASpriteSize();
                MACircleSprite.Visible = true;
            }
        }
    }

    private void OnPawnClicked(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (!waitingForTarget) return;

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            var worldPos = Pawn.GetGlobalMousePosition();
            var localPos = Pawn.ToLocal(worldPos);

            var circle = MAarea.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
            if (circle != null)
            {
                if (Pawn.MP > 0) {
                    circle.Radius = Pawn.MA;
                    if (localPos.Length() <= circle.Radius)
                    {
                        TargetMarker.Visible = true;
                        TargetMarker.GlobalPosition = worldPos;
                        /*
                        TO DO , ogarnąć jak podejżeć collisionshape w edytorze bo chyba zmienili a potem naprawić to szajstwo 
                        if (IsTargetPositionFree(worldPos))
                        {
                             TargetMarker.Visible = true;
                            TargetMarker.GlobalPosition = worldPos;
                        }
                        else
                        {
                            GD.Print("Nie moge tu stanąć");
                        }
                        */
                    }  
                }
            }
            // zakończ wybór
            isSelected = false;
            waitingForTarget = false;
            MAarea.Visible = false;
            StatsUI.Visible = false;
            MACircleSprite.Visible = false;
        }
    }
}
