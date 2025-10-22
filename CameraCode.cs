using Godot;
using System;

public partial class CameraCode : Camera2D
{
    [Export] float zoomspeed = 0.05f;
    [Export] Area2D NonoScroolZone;
    private bool isDragging = false;
    private bool CanScroll = true;
    private Vector2 dragStart;
    private Vector2 cameraStart;
    private Vector2 zoomLevel = new Vector2(0.1f, 0.1f);
    private Vector2 ScaleLevel = new Vector2(1f, 1f);
    private bool draggingTarget = false;
    private float moveRadius = 1600f; // promień ruchu wokół pionka
    Node2D marker;
    GameMNGR_Script gameMNGR_Script;
    public override void _Ready()
    {
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene");
        NonoScroolZone.MouseEntered += OnMouseEnter;
        NonoScroolZone.MouseExited += OnMouseExit;
    }

    public override void _Process(double delta)
    {
        if (gameMNGR_Script.SetupDone == true)
        {
            var pawn = gameMNGR_Script.SelectedPawn;
            if (pawn != null)
            {
                marker = pawn.TargetMarkerRef;
            }
        }
    }
    void OnMouseEnter()
    {
        CanScroll = false;
    }
    void OnMouseExit()
    {
        CanScroll = true;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (mouseButton.Pressed)
                {
                    isDragging = true;
                    dragStart = mouseButton.Position;
                    cameraStart = Position;
                }
                else
                {
                    isDragging = false;
                }
            }
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    // Zaczynamy drag jeśli klik na pionku
                    draggingTarget = true;
                    if (marker != null)
                    {
                        marker.Visible = true;
                    }
                }
                else
                {
                    // Koniec drag
                    draggingTarget = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && isDragging)
        {
            Vector2 offset = mouseMotion.Position - dragStart;
            Position = cameraStart - new Vector2(offset.X * zoomspeed, offset.Y * zoomspeed);
        }
        if (@event is InputEventMouseButton scrollEvent)
        {
            if (CanScroll == true)
            {
                if (scrollEvent.ButtonIndex == MouseButton.WheelDown && scrollEvent.Pressed && zoomLevel.X > 0.0825f && zoomLevel.Y > 0.0825f)
                {
                    zoomLevel -= new Vector2(zoomspeed, zoomspeed) * 0.002f;
                    Zoom = zoomLevel;
                }
                else if (scrollEvent.ButtonIndex == MouseButton.WheelUp && scrollEvent.Pressed && zoomLevel.X < 0.4f && zoomLevel.Y < 0.4f)
                {
                    zoomLevel += new Vector2(zoomspeed, zoomspeed) * 0.002f;
                    Zoom = zoomLevel;
                }
            }
            //GD.Print(Scale);
        }
        
    }
}
