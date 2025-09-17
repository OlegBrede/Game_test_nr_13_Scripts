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
            StatsLabel.Text = $"{Pawn.UnitName}\nHP {Pawn.HP}\nMA {Pawn.MA}";
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

    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx)
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            if (!isSelected)
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
                circle.Radius = Pawn.MA;
                if (localPos.Length() <= circle.Radius)
                {
                    TargetMarker.Visible = true;
                    TargetMarker.GlobalPosition = worldPos;
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
