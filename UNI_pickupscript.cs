using Godot;
using System;
using System.ComponentModel;

public partial class UNI_pickupscript : Node2D
{
    [Export] string FuncToCall;
    [Export] string GraphicPath;
    [Export] Area2D Hitbox;
    [Export] Timer ActTimer;
    [Export] Sprite2D PickupGraphic;
    GameMNGR_Script gameMNGR_Script;
    public override void _Ready()
    {
        if (GetTree().CurrentScene.Name != "BaseTestScene") // hack ale powinien naprawić błąd wyskakujący przy wczytywaniu
        {
            return;
        }
        gameMNGR_Script = GetTree().Root.GetNode<GameMNGR_Script>("BaseTestScene"); // to dalej daje error, pomyślę nad rozwiązaniem
        Texture2D ButTex = GD.Load<Texture2D>(GraphicPath);
        PickupGraphic.Texture = ButTex;
        ActTimer.Timeout += CheckForTakers;
    }
    public void StartTimer()
    {
        ActTimer.Start();
    }
    void CheckForTakers()
    {
        bool Pickuped = false;
        foreach (PawnBaseFuncsScript Podnoszący in Hitbox.GetOverlappingBodies())
        {
            Podnoszący.Call(FuncToCall);
            gameMNGR_Script.GenerateActionLog($"[color={Podnoszący.ColoredPartsNode.Modulate.ToHtml()}]{Podnoszący.UnitName}[/color] picked up Ammo");
            Pickuped = true;
        }
        if (Pickuped == true)
        {
            QueueFree();
        }
        ActTimer.Stop();
    }
}
