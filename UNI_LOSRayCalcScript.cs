using Godot;
using System;

public partial class UNI_LOSRayCalcScript : RayCast2D
{
    public bool Rayactive;
    public CharacterBody2D RayHittenTarget;
    public Node2D OverrideTarget;
    public override void _Process(double delta)
    {
        if (Rayactive == true)
        {
            Visible = true;
            Vector2 direction;
            if (OverrideTarget == null){
                direction = GetGlobalMousePosition() - GlobalPosition;
            } else {
                direction = OverrideTarget.GlobalPosition - GlobalPosition;
            }
            // Ustaw końcowy punkt RayCast2D
            TargetPosition = direction * 10;
            // Wywołaj ponowne rysowanie
            QueueRedraw();
        }
        else
        {
            Visible = false;
        }
    }
    public override void _Draw()
    {
        Vector2 startPoint = Vector2.Zero;
        Vector2 endPoint = TargetPosition;
        if (IsColliding())
        {
            var collider = GetCollider();
            if (collider is StaticBody2D)
            {
                endPoint = ToLocal(GetCollisionPoint());
            }
            if (collider is CharacterBody2D Charachter)
            {
                endPoint = ToLocal(GetCollisionPoint());
                RayHittenTarget = Charachter;
            }
            else
            {
                RayHittenTarget = null;
            }
        }
		startPoint  = endPoint - (endPoint.Normalized() * 75);
        DrawLine(startPoint , endPoint, Colors.Blue, 20);
    }
}
