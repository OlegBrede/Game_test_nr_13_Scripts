using Godot;
using System;

public partial class PawnBaseFuncsScript : CharacterBody2D
{
    [Export] string unitname = "Princess";
    [Export] int HP = 100; //bullshit
    [Export] float MA = 3750; // movement allowance
    [Export] public Node2D StatsUI;
    [Export] public Node2D TargetMarker;
    [Export] public Label StatsLabel;

    GameMNGR_Script GameManager;
    Area2D MAarea;
    Sprite2D MACircleSprite;
    private bool isSelected = false;
    private bool waitingForTarget = false;
    Vector2 texSize;
    float baseRadius;
    public override void _Ready()
    {
        MACircleSprite = GetNode<Sprite2D>("MASpritePolly");
        MACircleSprite.Visible = false;

        var Statarea = GetNode<Area2D>("Area2DStatBox");
        Statarea.MouseEntered += OnMouseEnter;
        Statarea.MouseExited += OnMouseExit;
        Statarea.InputEvent += OnAreaInputEvent;

        MAarea = GetNode<Area2D>("Area2DMovementRadius");
        MAarea.InputEvent += OnPawnClicked;

        StatsUI.Visible = false;
        TargetMarker.Visible = false;
        MAarea.Visible = false; // obszar ruchu niewidoczny na start
    }

    public override void _Process(double delta)
    {
        if (StatsUI.Visible)
        {
            StatsLabel.Text = $"{unitname}\nHP {HP}\nMA {MA}";
        }
    }

    private void OnMouseEnter()
    {
        StatsUI.Visible = true;
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            StatsUI.Visible = false;
        }
    }
    void ChangeVisibleMASpriteSize()
    {
        texSize = MACircleSprite.Texture.GetSize();
        baseRadius = texSize.X / 2f;
        float scale = MA / baseRadius;
        MACircleSprite.Scale = new Vector2(scale, scale);
    }
    private void OnAreaInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            if (!isSelected)
            {
                GameMNGR_Script.Instance.SelectPawn(this);
                isSelected = true;
                waitingForTarget = true;   // teraz czekamy na klik w koło ruchu
                MAarea.Visible = true;     // pokaż zasięg
                ChangeVisibleMASpriteSize();
                MACircleSprite.Visible = true;
            }
        }
    }

    private void OnPawnClicked(Node viewport, InputEvent @event, long shapeIdx)
    {
        // Ten handler odpala się TYLKO jeśli klik był w Area2DMovementRadius
        if (!waitingForTarget) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            var worldPos = GetGlobalMousePosition();
            var localPos = ToLocal(worldPos);

            var circle = MAarea.GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D;
            circle.Radius = MA;
            if (circle != null && localPos.Length() <= circle.Radius)
            {
                TargetMarker.Visible = true;
                TargetMarker.GlobalPosition = worldPos;
                //GD.Print($"Nowy target: {TargetMarker.GlobalPosition}");
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
