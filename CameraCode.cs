using Godot;
using System;

public partial class CameraCode : Camera2D
{
    [Export] float zoomspeed = 0.05f;
    private bool isDragging = false;
    private Vector2 dragStart;
    private Vector2 cameraStart;
    private Vector2 zoomLevel = new Vector2(0.1f, 0.1f);
    private bool draggingTarget = false;
    private float moveRadius = 1200f; // promień ruchu wokół pionka
    Node2D marker;
    public override void _Process(double delta)
    {
        var pawn = GameMNGR_Script.Instance.SelectedPawn;
        if (pawn != null)
        {
            marker = pawn.TargetMarkerRef;
        }
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
            if (scrollEvent.ButtonIndex == MouseButton.WheelDown && scrollEvent.Pressed && zoomLevel.X > 0.1f && zoomLevel.Y > 0.1f)
            {
                zoomLevel -= new Vector2(zoomspeed, zoomspeed) * 0.002f;
                Zoom = zoomLevel;
            }
            else if (scrollEvent.ButtonIndex == MouseButton.WheelUp && scrollEvent.Pressed && zoomLevel.X < 0.4f && zoomLevel.Y < 0.4f)
            {
                zoomLevel += new Vector2(zoomspeed, zoomspeed) * 0.002f;
                Zoom = zoomLevel;
            }
            //GD.Print(Zoom);
        }
        
    }
}
